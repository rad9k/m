using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using m0.Foundation;
using m0.Store;
using m0;
using m0.Util;
using m0.ZeroCode;
using System.Runtime.InteropServices;
using m0.ZeroCode.Helpers;
using m0.DotNetIntegration;
using m0.Graph.Internal;
using m0.Graph.ExecutionFlow;
using static m0.Graph.GraphUtil;
using m0.ZeroTypes;

namespace m0.Graph
{
    internal enum VertexIdentifierRegistrationMode
    {
        Registered,
        Ephemeral
    }

    internal enum OutEdgesRebuildKind
    {
        LogicalEdges,
        DirectMeta,
        QueryMeta,
        Value,
        QueryMetaAndValue
    }

    [Serializable]
    public class EasyVertex: VertexBase, IDisposable, IImplementedVertex, ISecondStageCommitAction
    {
        private const byte LogicalOutIndexMask = 1;
        private const byte DirectMetaOutIndexMask = 2;
        private const byte QueryMetaOutIndexMask = 4;
        private const byte ValueOutIndexMask = 8;
        private const byte MetaAndValueOutIndexMask = 16;

        private enum InheritedOutIndexKind
        {
            LogicalEdges,
            DirectMeta,
            QueryMeta,
            Value,
            QueryMetaAndValue
        }

        private readonly struct ParentOutDependencyVersion
        {
            public ParentOutDependencyVersion(EasyVertex vertex)
            {
                Vertex = vertex;
                StructureGeneration =
                    Volatile.Read(
                        ref vertex.outStructureGeneration);
                DirectMetaGeneration =
                    Volatile.Read(
                        ref vertex.outDirectMetaGeneration);
                QueryMetaGeneration =
                    Volatile.Read(
                        ref vertex.outQueryMetaGeneration);
                ValueGeneration =
                    Volatile.Read(
                        ref vertex.outValueGeneration);
            }

            public EasyVertex Vertex { get; }

            public long StructureGeneration { get; }

            public long DirectMetaGeneration { get; }

            public long QueryMetaGeneration { get; }

            public long ValueGeneration { get; }
        }

        private sealed class InheritedOutDependencyStamp
        {
            public InheritedOutDependencyStamp()
            {
            }

            public InheritedOutDependencyStamp(
                ParentOutDependencyVersion firstParent)
            {
                FirstParent = firstParent;
            }

            public InheritedOutDependencyStamp(
                ParentOutDependencyVersion firstParent,
                ParentOutDependencyVersion[]
                    additionalParents)
            {
                FirstParent = firstParent;
                AdditionalParents = additionalParents;
            }

            public ParentOutDependencyVersion FirstParent
            {
                get;
            }

            public ParentOutDependencyVersion[]
                AdditionalParents
            {
                get;
            }

            public int ParentCount
            {
                get
                {
                    if (FirstParent.Vertex == null)
                        return 0;

                    return 1 +
                        (AdditionalParents?.Length ?? 0);
                }
            }
        }

        private static long inheritanceDependencyEpoch = 1;

        [NonSerialized]
        private long outStructureGeneration;

        [NonSerialized]
        private long outDirectMetaGeneration;

        [NonSerialized]
        private long outQueryMetaGeneration;

        [NonSerialized]
        private long outValueGeneration;

        [NonSerialized]
        private InheritedOutDependencyStamp outEdgesDependencyStamp;

        [NonSerialized]
        private InheritedOutDependencyStamp directMetaDependencyStamp;

        [NonSerialized]
        private InheritedOutDependencyStamp queryMetaDependencyStamp;

        [NonSerialized]
        private InheritedOutDependencyStamp valueDependencyStamp;

        [NonSerialized]
        private InheritedOutDependencyStamp metaAndValueDependencyStamp;

        [NonSerialized]
        private long outEdgesDependencyCheckedEpoch;

        [NonSerialized]
        private long directMetaDependencyCheckedEpoch;

        [NonSerialized]
        private long queryMetaDependencyCheckedEpoch;

        [NonSerialized]
        private long valueDependencyCheckedEpoch;

        [NonSerialized]
        private long metaAndValueDependencyCheckedEpoch;

        [NonSerialized]
        private byte currentOutIndexMask;

        [NonSerialized]
        private int consecutiveIncrementalOutIndexMutations;

        [NonSerialized]
        private int incrementalOutIndexMutationBudget = 1;

        [NonSerialized]
        private int outIndexMutationsSinceLastQuery;

        [NonSerialized]
        private bool outIndexMutationBudgetFallbackPending;

        [NonSerialized]
        private bool incrementalOutIndexMutationBudgetIsFixed;

        private static readonly string[] emptyMetaQueryKeys = new string[] { "" };

        [NonSerialized]
        private string[] metaQueryKeys;

        private Dictionary<string, EdgeBucket>
            outEdgesByQueryMeta;

        [NonSerialized]
        private bool
            outEdgesHaveExplicitQueryValueTargets;

        [NonSerialized]
        private bool
            inEdgesHaveExplicitQueryValueSources;

        protected bool CanEmitGraphChangeEvents = true;

        protected EdgeDictionaries edgeDictionaries;

        public object _Identifier;
        
        public override object Identifier { get { return _Identifier; }}


        protected object _Value;

        public override object Value {
            get{
                return _Value;
            }
            set{
                object oldValue = _Value;

                if (value == null)
                    return;                

                _Value = value;

                ValueChanged();

                if (GeneralUtil.CompareStrings(
                        oldValue,
                        "$NoInherit") !=
                    GeneralUtil.CompareStrings(
                        _Value,
                        "$NoInherit"))
                    NotifyNoInheritMarkerMetaValueChanged();

                //FireChange(new VertexChangeEventArgs(VertexChangeType.ValueChanged, null));

                if (_Value.ToString() == "piesek")
                {
                    int x = 0;
                }

                if (CanEmitGraphChangeEvents)
                    ExecutionFlowHelper.AddTransactionAtom(new GraphChangeTransactionAtom(
                        this,
                        AtomGraphChangeTypeEnum.ValueChange,
                        oldValue,
                        _Value,
                        null));

                GraphUtil.Debug(this, DebugOperationEnum.Value);
            }
        }

        protected void ValueChanged()
        {
            if (InEdgesRaw.Count == 1)
            {
                IVertex sourceVertex = InEdgesRaw[0].From;

                if (sourceVertex != null)
                    InvalidateOutValueIndexes(sourceVertex);
            }
            else if (InEdgesRaw.Count > 1)
            {
                HashSet<IVertex> affectedSourceVertices = new HashSet<IVertex>();

                foreach (IEdge e in InEdgesRaw)
                    if (e.From != null) // there could be artificial edge, with From==null
                        affectedSourceVertices.Add(e.From);

                foreach (IVertex sourceVertex in affectedSourceVertices)
                    InvalidateOutValueIndexes(sourceVertex);
            }

            foreach (IEdge e in OutEdgesRaw)
                e.To.InEdgesDictionariesNeedsRebuild = true;

            metaQueryKeys = null;

            if (MetaInEdgesRaw.Count > 0 ||
                InheritsInEdgeCount > 0)
                InvalidateMetaQueryIndexesForThisAndInheritChildren(true);
        }

        private static void InvalidateOutValueIndexes(IVertex sourceVertex)
        {
            MarkOutValueIndexesNeedRebuild(sourceVertex);

            if (sourceVertex is EasyVertex easySourceVertex)
            {
                easySourceVertex
                    .IncrementOutValueGeneration();
                return;
            }

            HashSet<IVertex> inheritChildren = VertexHelper.GetInheritChilds(sourceVertex);

            foreach (IVertex inheritChild in inheritChildren)
                MarkOutValueIndexesNeedRebuild(inheritChild);
        }

        private void IncrementOutValueGeneration()
        {
            if (InheritsInEdgeCount == 0)
                return;

            Interlocked.Increment(
                ref outValueGeneration);
            IncrementInheritanceDependencyEpoch();
        }

        private static void MarkOutValueIndexesNeedRebuild(IVertex vertex)
        {
            if (vertex is EasyVertex easyVertex)
            {
                easyVertex.OutEdgesDictionariesNeedsRebuild_Value = true;
                easyVertex.OutEdgesDictionariesNeedsRebuild_MetaAndValue = true;
                return;
            }

            vertex.OutEdgesDictionariesNeedsRebuild = true;
        }

        public bool HasInheritance { get; set; }

        public bool AllowInheritance = true;

        // InEdgesRaw
        // from == who inherits from me
        // meta == $Inherits
        // to == this

        private IList<IEdge> inheritsInEdges;

        public IList<IEdge> InheritsInEdges
        {
            get
            {
                return inheritsInEdges ??=
                    new List<IEdge>();
            }
            set
            {
                inheritsInEdges = value;
            }
        }

        private int InheritsInEdgeCount =>
            inheritsInEdges?.Count ?? 0;

        // OutEdgesRaw
        // from == this
        // meta == $Inherits
        // to == who I inherit from

        private IList<IEdge> inheritsOutEdges;

        public IList<IEdge> InheritsOutEdges
        {
            get
            {
                return inheritsOutEdges ??=
                    new List<IEdge>();
            }
            set
            {
                inheritsOutEdges = value;
            }
        }

        private int InheritsOutEdgeCount =>
            inheritsOutEdges?.Count ?? 0;

        public override IList<IEdge> InEdgesRaw { get { return edgeDictionaries.In; } }

        public override IList<IEdge> OutEdgesRaw { get { return edgeDictionaries.Out; } }

        protected IList<IEdge> _OutEdges;

        private static long CurrentInheritanceDependencyEpoch
        {
            get
            {
                return Volatile.Read(
                    ref inheritanceDependencyEpoch);
            }
        }

        private static void IncrementInheritanceDependencyEpoch()
        {
            Interlocked.Increment(
                ref inheritanceDependencyEpoch);
        }

        private InheritedOutDependencyStamp
            CaptureInheritedOutDependencyStamp(
                HashSet<IVertex> parents)
        {
            if (!HasInheritance || !AllowInheritance)
                return null;

            int easyParentCount = 0;

            foreach (IVertex parent in parents)
                if (parent is EasyVertex)
                    easyParentCount++;

            if (easyParentCount == 0)
                return new InheritedOutDependencyStamp();

            ParentOutDependencyVersion firstParent =
                default;
            ParentOutDependencyVersion[]
                additionalParents =
                    easyParentCount > 1
                        ? new ParentOutDependencyVersion[
                            easyParentCount - 1]
                        : null;
            int index = -1;

            foreach (IVertex parent in parents)
                if (parent is EasyVertex easyParent)
                {
                    ParentOutDependencyVersion version =
                        new ParentOutDependencyVersion(
                            easyParent);

                    if (index < 0)
                        firstParent = version;
                    else
                        additionalParents[index] =
                            version;

                    index++;
                }

            return additionalParents == null
                ? new InheritedOutDependencyStamp(
                    firstParent)
                : new InheritedOutDependencyStamp(
                    firstParent,
                    additionalParents);
        }

        private InheritedOutDependencyStamp
            CaptureCurrentInheritedOutDependencyStamp()
        {
            if (!HasInheritance || !AllowInheritance)
                return null;

            if (outEdgesDependencyStamp == null)
                return CaptureInheritedOutDependencyStamp(
                    VertexHelper.GetInheritParents(this));

            if (outEdgesDependencyStamp.ParentCount == 0)
                return new InheritedOutDependencyStamp();

            ParentOutDependencyVersion firstParent =
                new ParentOutDependencyVersion(
                    outEdgesDependencyStamp
                        .FirstParent
                        .Vertex);
            ParentOutDependencyVersion[]
                previousAdditionalParents =
                    outEdgesDependencyStamp
                        .AdditionalParents;

            if (previousAdditionalParents == null)
                return new InheritedOutDependencyStamp(
                    firstParent);

            ParentOutDependencyVersion[]
                currentAdditionalParents =
                    new ParentOutDependencyVersion[
                        previousAdditionalParents.Length];

            for (int index = 0;
                index < currentAdditionalParents.Length;
                index++)
            {
                currentAdditionalParents[index] =
                    new ParentOutDependencyVersion(
                        previousAdditionalParents[index]
                            .Vertex);
            }

            return new InheritedOutDependencyStamp(
                firstParent,
                currentAdditionalParents);
        }

        protected void CompleteLogicalOutEdgesRebuild(
            HashSet<IVertex> parents)
        {
            outEdgesDependencyStamp =
                CaptureInheritedOutDependencyStamp(
                    parents);
            Volatile.Write(
                ref outEdgesDependencyCheckedEpoch,
                CurrentInheritanceDependencyEpoch);
            currentOutIndexMask |=
                LogicalOutIndexMask;
        }

        private void CompleteInheritedOutIndexRebuild(
            InheritedOutIndexKind kind,
            InheritedOutDependencyStamp dependencyStamp =
                null)
        {
            dependencyStamp ??=
                CaptureCurrentInheritedOutDependencyStamp();
            long currentEpoch =
                CurrentInheritanceDependencyEpoch;

            switch (kind)
            {
                case InheritedOutIndexKind.DirectMeta:
                    directMetaDependencyStamp =
                        dependencyStamp;
                    Volatile.Write(
                        ref directMetaDependencyCheckedEpoch,
                        currentEpoch);
                    currentOutIndexMask |=
                        DirectMetaOutIndexMask;
                    break;
                case InheritedOutIndexKind.QueryMeta:
                    queryMetaDependencyStamp =
                        dependencyStamp;
                    Volatile.Write(
                        ref queryMetaDependencyCheckedEpoch,
                        currentEpoch);
                    currentOutIndexMask |=
                        QueryMetaOutIndexMask;
                    break;
                case InheritedOutIndexKind.Value:
                    valueDependencyStamp =
                        dependencyStamp;
                    Volatile.Write(
                        ref valueDependencyCheckedEpoch,
                        currentEpoch);
                    currentOutIndexMask |=
                        ValueOutIndexMask;
                    break;
                case InheritedOutIndexKind.QueryMetaAndValue:
                    metaAndValueDependencyStamp =
                        dependencyStamp;
                    Volatile.Write(
                        ref metaAndValueDependencyCheckedEpoch,
                        currentEpoch);
                    currentOutIndexMask |=
                        MetaAndValueOutIndexMask;
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnsureInheritedLogicalOutEdgesCurrent()
        {
            if (OutEdgesDictionariesNeedsRebuild_Edges)
                return;

            long currentEpoch =
                CurrentInheritanceDependencyEpoch;

            if (Volatile.Read(
                    ref outEdgesDependencyCheckedEpoch) ==
                currentEpoch)
                return;

            EnsureInheritedOutIndexCurrentSlow(
                InheritedOutIndexKind.LogicalEdges,
                outEdgesDependencyStamp,
                ref outEdgesDependencyCheckedEpoch,
                currentEpoch);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void EnsureInheritedOutIndexCurrentSlow(
            InheritedOutIndexKind kind,
            InheritedOutDependencyStamp dependencyStamp,
            ref long checkedEpoch,
            long currentEpoch)
        {
            if (!HasInheritance || !AllowInheritance)
            {
                Volatile.Write(
                    ref checkedEpoch,
                    currentEpoch);
                return;
            }

            if (dependencyStamp == null)
            {
                MarkInheritedOutIndexNeedRebuild(kind);
                return;
            }

            bool structureChanged = false;
            bool indexDependencyChanged = false;

            if (dependencyStamp.ParentCount > 0)
            {
                CheckParentOutDependency(
                    dependencyStamp.FirstParent,
                    kind,
                    ref structureChanged,
                    ref indexDependencyChanged);

                if (!structureChanged &&
                    !indexDependencyChanged &&
                    dependencyStamp.AdditionalParents !=
                        null)
                {
                    foreach (
                        ParentOutDependencyVersion parent
                        in dependencyStamp
                            .AdditionalParents)
                    {
                        CheckParentOutDependency(
                            parent,
                            kind,
                            ref structureChanged,
                            ref indexDependencyChanged);

                        if (structureChanged ||
                            indexDependencyChanged)
                            break;
                    }
                }
            }

            if (structureChanged)
            {
                OutEdgesDictionariesNeedsRebuild = true;
                return;
            }

            if (indexDependencyChanged)
            {
                Volatile.Write(
                    ref outEdgesDependencyCheckedEpoch,
                    currentEpoch);
                MarkInheritedOutIndexNeedRebuild(kind);
                return;
            }

            Volatile.Write(
                ref checkedEpoch,
                currentEpoch);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckParentOutDependency(
            ParentOutDependencyVersion parent,
            InheritedOutIndexKind kind,
            ref bool structureChanged,
            ref bool indexDependencyChanged)
        {
            EasyVertex parentVertex = parent.Vertex;

            if (parent.StructureGeneration !=
                Volatile.Read(
                    ref parentVertex
                        .outStructureGeneration))
            {
                structureChanged = true;
                return;
            }

            switch (kind)
            {
                case InheritedOutIndexKind.DirectMeta:
                    indexDependencyChanged =
                        parent.DirectMetaGeneration !=
                        Volatile.Read(
                            ref parentVertex
                                .outDirectMetaGeneration);
                    break;
                case InheritedOutIndexKind.QueryMeta:
                    indexDependencyChanged =
                        parent.QueryMetaGeneration !=
                        Volatile.Read(
                            ref parentVertex
                                .outQueryMetaGeneration);
                    break;
                case InheritedOutIndexKind.Value:
                    indexDependencyChanged =
                        parent.ValueGeneration !=
                        Volatile.Read(
                            ref parentVertex
                                .outValueGeneration);
                    break;
                case InheritedOutIndexKind.QueryMetaAndValue:
                    indexDependencyChanged =
                        parent.QueryMetaGeneration !=
                        Volatile.Read(
                            ref parentVertex
                                .outQueryMetaGeneration) ||
                        parent.ValueGeneration !=
                        Volatile.Read(
                            ref parentVertex
                                .outValueGeneration);
                    break;
            }
        }

        private void MarkInheritedOutIndexNeedRebuild(
            InheritedOutIndexKind kind)
        {
            switch (kind)
            {
                case InheritedOutIndexKind.LogicalEdges:
                    OutEdgesDictionariesNeedsRebuild = true;
                    break;
                case InheritedOutIndexKind.DirectMeta:
                    OutEdgesDictionariesNeedsRebuild_Meta =
                        true;
                    break;
                case InheritedOutIndexKind.QueryMeta:
                    OutEdgesDictionariesNeedsRebuild_QueryMeta =
                        true;
                    break;
                case InheritedOutIndexKind.Value:
                    OutEdgesDictionariesNeedsRebuild_Value =
                        true;
                    break;
                case InheritedOutIndexKind.QueryMetaAndValue:
                    OutEdgesDictionariesNeedsRebuild_MetaAndValue =
                        true;
                    break;
            }
        }

        public override IList<IEdge> OutEdges
        {
            get
            {
                CompleteIncrementalOutIndexMutationBurst();

                if (!OutEdgesDictionariesNeedsRebuild_Edges)
                {
                    long currentEpoch =
                        CurrentInheritanceDependencyEpoch;

                    if (Volatile.Read(
                            ref outEdgesDependencyCheckedEpoch) !=
                        currentEpoch)
                    {
                        EnsureInheritedOutIndexCurrentSlow(
                            InheritedOutIndexKind.LogicalEdges,
                            outEdgesDependencyStamp,
                            ref outEdgesDependencyCheckedEpoch,
                            currentEpoch);
                    }
                }

                if (OutEdgesDictionariesNeedsRebuild_Edges)
                {
                    OutEdgesDictionariesRebuild_Edges();                    
                    return _OutEdges;
                }
                else
                    return _OutEdges;                
            }
        }

        public override IList<IEdge> MetaInEdgesRaw { get { return edgeDictionaries.MetaIn; } }

        protected virtual void OutEdgesDictionariesRebuild_Edges()
        {
            HashSet<IVertex> parents = null;

            if (HasInheritance && AllowInheritance)
            {
                List<IEdge> FullEdges = OutEdgesRaw.ToList();

                parents = VertexHelper.GetInheritParents(this);

                foreach (IVertex v in parents)
                    GraphUtil.AddRange_NoNoInherit(FullEdges, v.OutEdgesRaw);                    

                _OutEdges = FullEdges;
            }
            else
                _OutEdges = OutEdgesRaw;

            OutEdgesDictionariesNeedsRebuild_Edges = false;
            CompleteLogicalOutEdgesRebuild(parents);
        }

        protected virtual bool CanShareOutEdgesRebuild()
        {
            return OutEdgesDictionariesNeedsRebuild_Edges &&
                HasInheritance &&
                AllowInheritance;
        }

        private HashSet<IVertex> GetSharedRebuildParents(
            out int capacity)
        {
            HashSet<IVertex> parents =
                VertexHelper.GetInheritParents(this);
            capacity = OutEdgesRaw.Count;

            foreach (IVertex parent in parents)
                capacity += parent.OutEdgesRaw.Count;

            return parents;
        }

        private void CompleteSharedOutEdgesRebuild(
            List<IEdge> fullEdges,
            HashSet<IVertex> parents,
            OutEdgesRebuildKind indexKind)
        {
            _OutEdges = fullEdges;
            OutEdgesDictionariesNeedsRebuild_Edges = false;
            CompleteLogicalOutEdgesRebuild(parents);
            CompleteInheritedOutIndexRebuild(
                GetInheritedOutIndexKind(indexKind),
                outEdgesDependencyStamp);
        }

        private static InheritedOutIndexKind
            GetInheritedOutIndexKind(
                OutEdgesRebuildKind rebuildKind)
        {
            return rebuildKind switch
            {
                OutEdgesRebuildKind.DirectMeta =>
                    InheritedOutIndexKind.DirectMeta,
                OutEdgesRebuildKind.QueryMeta =>
                    InheritedOutIndexKind.QueryMeta,
                OutEdgesRebuildKind.Value =>
                    InheritedOutIndexKind.Value,
                OutEdgesRebuildKind.QueryMetaAndValue =>
                    InheritedOutIndexKind.QueryMetaAndValue,
                _ => InheritedOutIndexKind.LogicalEdges
            };
        }

        private bool TryRebuildOutEdgesWithDirectMetaIndex()
        {
            if (!CanShareOutEdgesRebuild())
                return false;

            HashSet<IVertex> parents =
                GetSharedRebuildParents(out int capacity);
            List<IEdge> fullEdges =
                new List<IEdge>(capacity);
            Dictionary<object, object> edgesByMeta =
                new Dictionary<object, object>(capacity);

            foreach (IEdge edge in OutEdgesRaw)
            {
                fullEdges.Add(edge);
                AddEdgeToDictionary(
                    edgesByMeta,
                    GetQueryDictionaryKey(
                        edge.Meta?.Value),
                    edge);
            }

            IVertex previousMeta = null;
            bool previousMetaHasNoInherit = false;
            bool hasPreviousMeta = false;

            foreach (IVertex parent in parents)
                foreach (IEdge edge in parent.OutEdgesRaw)
                {

                    if (!hasPreviousMeta ||
                        !ReferenceEquals(
                            previousMeta,
                            edge.Meta))
                    {
                        previousMeta = edge.Meta;
                        previousMetaHasNoInherit =
                            GraphUtil.ExistQueryOut(
                                edge.Meta,
                                "$NoInherit",
                                null);
                        hasPreviousMeta = true;
                    }

                    if (previousMetaHasNoInherit)
                        continue;

                    fullEdges.Add(edge);
                    AddEdgeToDictionary(
                        edgesByMeta,
                        GetQueryDictionaryKey(
                            edge.Meta?.Value),
                        edge);
                }

            _OutEdgesByMeta = edgesByMeta;
            OutEdgesDictionariesNeedsRebuild_Meta = false;
            CompleteSharedOutEdgesRebuild(
                fullEdges,
                parents,
                OutEdgesRebuildKind.DirectMeta);
            return true;
        }

        private bool TryRebuildOutEdgesWithQueryMetaIndex()
        {
            if (!CanShareOutEdgesRebuild())
                return false;

            HashSet<IVertex> parents =
                GetSharedRebuildParents(out int capacity);
            List<IEdge> fullEdges =
                new List<IEdge>(capacity);
            Dictionary<string, EdgeBucket>
                edgesByMeta =
                    new Dictionary<
                        string,
                        EdgeBucket>(capacity);

            foreach (IEdge edge in OutEdgesRaw)
            {
                fullEdges.Add(edge);

                foreach (string queryMetaKey in
                    GetMetaQueryKeys(edge.Meta))
                    AddEdgeToDictionary(
                        edgesByMeta,
                        queryMetaKey,
                        edge);
            }

            if (capacity <= 4)
            {
                foreach (IVertex parent in parents)
                    foreach (IEdge edge in parent.OutEdgesRaw)
                    {

                        if (GraphUtil.ExistQueryOut(
                            edge.Meta,
                            "$NoInherit",
                            null))
                            continue;

                        fullEdges.Add(edge);

                        foreach (string queryMetaKey in
                            GetMetaQueryKeys(edge.Meta))
                            AddEdgeToDictionary(
                                edgesByMeta,
                                queryMetaKey,
                                edge);
                    }
            }
            else
            {
                IVertex previousMeta = null;
                bool previousMetaHasNoInherit = false;
                bool hasPreviousMeta = false;

                foreach (IVertex parent in parents)
                    foreach (IEdge edge in parent.OutEdgesRaw)
                    {

                        if (!hasPreviousMeta ||
                            !ReferenceEquals(
                                previousMeta,
                                edge.Meta))
                        {
                            previousMeta = edge.Meta;
                            previousMetaHasNoInherit =
                                GraphUtil.ExistQueryOut(
                                    edge.Meta,
                                    "$NoInherit",
                                    null);
                            hasPreviousMeta = true;
                        }

                        if (previousMetaHasNoInherit)
                            continue;

                        fullEdges.Add(edge);

                        foreach (string queryMetaKey in
                            GetMetaQueryKeys(edge.Meta))
                            AddEdgeToDictionary(
                                edgesByMeta,
                                queryMetaKey,
                                edge);
                    }
            }

            outEdgesByQueryMeta = edgesByMeta;
            OutEdgesDictionariesNeedsRebuild_QueryMeta = false;
            CompleteSharedOutEdgesRebuild(
                fullEdges,
                parents,
                OutEdgesRebuildKind.QueryMeta);
            return true;
        }

        private bool TryRebuildOutEdgesWithValueIndex()
        {
            if (!CanShareOutEdgesRebuild())
                return false;

            HashSet<IVertex> parents =
                GetSharedRebuildParents(out int capacity);
            List<IEdge> fullEdges =
                new List<IEdge>(capacity);
            Dictionary<string, object> edgesByValue =
                new Dictionary<string, object>(capacity);
            bool hasExplicitQueryValueTargets =
                false;

            foreach (IEdge edge in OutEdgesRaw)
            {
                fullEdges.Add(edge);

                if (TryGetIndexableQueryValue(
                    edge.To,
                    out object queryValue))
                    AddEdgeToDictionary(
                        edgesByValue,
                        GetQueryDictionaryKey(
                            queryValue),
                        edge);
                else
                    hasExplicitQueryValueTargets =
                        true;
            }

            IVertex previousMeta = null;
            bool previousMetaHasNoInherit = false;
            bool hasPreviousMeta = false;

            foreach (IVertex parent in parents)
                foreach (IEdge edge in parent.OutEdgesRaw)
                {

                    if (!hasPreviousMeta ||
                        !ReferenceEquals(
                            previousMeta,
                            edge.Meta))
                    {
                        previousMeta = edge.Meta;
                        previousMetaHasNoInherit =
                            GraphUtil.ExistQueryOut(
                                edge.Meta,
                                "$NoInherit",
                                null);
                        hasPreviousMeta = true;
                    }

                    if (previousMetaHasNoInherit)
                        continue;

                    fullEdges.Add(edge);

                    if (TryGetIndexableQueryValue(
                        edge.To,
                        out object queryValue))
                        AddEdgeToDictionary(
                            edgesByValue,
                            GetQueryDictionaryKey(
                                queryValue),
                            edge);
                    else
                        hasExplicitQueryValueTargets =
                            true;
                }

            _OutEdgesByValue = edgesByValue;
            outEdgesHaveExplicitQueryValueTargets =
                hasExplicitQueryValueTargets;
            OutEdgesDictionariesNeedsRebuild_Value = false;
            CompleteSharedOutEdgesRebuild(
                fullEdges,
                parents,
                OutEdgesRebuildKind.Value);
            return true;
        }

        private bool TryRebuildOutEdgesWithMetaAndValueIndex()
        {
            if (!CanShareOutEdgesRebuild())
                return false;

            HashSet<IVertex> parents =
                GetSharedRebuildParents(out int capacity);
            List<IEdge> fullEdges =
                new List<IEdge>(capacity);
            Dictionary<GraphUtil.MetaAndValueKey, object>
                edgesByMetaAndValue =
                    new Dictionary<
                        GraphUtil.MetaAndValueKey,
                        object>(capacity);
            bool hasExplicitQueryValueTargets =
                false;

            foreach (IEdge edge in OutEdgesRaw)
            {
                fullEdges.Add(edge);

                if (TryGetIndexableQueryValue(
                    edge.To,
                    out object queryValue))
                    foreach (string queryMetaKey in
                        GetMetaQueryKeys(edge.Meta))
                        AddEdgeToDictionary(
                            edgesByMetaAndValue,
                            new GraphUtil.MetaAndValueKey(
                                queryMetaKey,
                                queryValue),
                            edge);
                else
                    hasExplicitQueryValueTargets =
                        true;
            }

            IVertex previousMeta = null;
            bool previousMetaHasNoInherit = false;
            bool hasPreviousMeta = false;

            foreach (IVertex parent in parents)
                foreach (IEdge edge in parent.OutEdgesRaw)
                {

                    if (!hasPreviousMeta ||
                        !ReferenceEquals(
                            previousMeta,
                            edge.Meta))
                    {
                        previousMeta = edge.Meta;
                        previousMetaHasNoInherit =
                            GraphUtil.ExistQueryOut(
                                edge.Meta,
                                "$NoInherit",
                                null);
                        hasPreviousMeta = true;
                    }

                    if (previousMetaHasNoInherit)
                        continue;

                    fullEdges.Add(edge);

                    if (TryGetIndexableQueryValue(
                        edge.To,
                        out object queryValue))
                        foreach (string queryMetaKey in
                            GetMetaQueryKeys(edge.Meta))
                            AddEdgeToDictionary(
                                edgesByMetaAndValue,
                                new GraphUtil.MetaAndValueKey(
                                    queryMetaKey,
                                    queryValue),
                                edge);
                    else
                        hasExplicitQueryValueTargets =
                            true;
                }

            _OutEdgesByMetaAndValue = edgesByMetaAndValue;
            outEdgesHaveExplicitQueryValueTargets =
                hasExplicitQueryValueTargets;
            OutEdgesDictionariesNeedsRebuild_MetaAndValue = false;
            CompleteSharedOutEdgesRebuild(
                fullEdges,
                parents,
                OutEdgesRebuildKind.QueryMetaAndValue);
            return true;
        }

        private void InEdgesDictionariesRebuild_Meta()
        {
            IList<IEdge> inEdges = InEdgesRaw;
            Dictionary<string, object> edgesByMeta =
                new Dictionary<string, object>(inEdges.Count);

            _InEdgesByMeta = edgesByMeta;

            foreach (IEdge edge in inEdges)
                AddEdgeToDictionary(
                    edgesByMeta,
                    GetQueryDictionaryKey(edge.Meta?.Value),
                    edge);

            InEdgesDictionariesNeedsRebuild_Meta = false;
        }

        private void OutEdgesDictionariesRebuild_Meta()
        {
            if (TryRebuildOutEdgesWithDirectMetaIndex())
                return;

            IList<IEdge> outEdges = OutEdges;
            Dictionary<object, object> directEdgesByMeta =
                new Dictionary<object, object>(outEdges.Count);

            _OutEdgesByMeta = directEdgesByMeta;

            foreach (IEdge edge in outEdges)
                AddEdgeToDictionary(
                    directEdgesByMeta,
                    GetQueryDictionaryKey(edge.Meta?.Value),
                    edge);

            OutEdgesDictionariesNeedsRebuild_Meta = false;
            CompleteInheritedOutIndexRebuild(
                InheritedOutIndexKind.DirectMeta);
        }

        private void OutEdgesDictionariesRebuild_QueryMeta()
        {
            if (TryRebuildOutEdgesWithQueryMetaIndex())
                return;

            IList<IEdge> outEdges = OutEdges;
            Dictionary<string, EdgeBucket>
                queryEdgesByMeta =
                    new Dictionary<
                        string,
                        EdgeBucket>(outEdges.Count);

            outEdgesByQueryMeta = queryEdgesByMeta;

            foreach (IEdge edge in outEdges)
                foreach (string queryMetaKey in GetMetaQueryKeys(edge.Meta))
                    AddEdgeToDictionary(queryEdgesByMeta, queryMetaKey, edge);

            OutEdgesDictionariesNeedsRebuild_QueryMeta = false;
            CompleteInheritedOutIndexRebuild(
                InheritedOutIndexKind.QueryMeta);
        }

        private void InEdgesDictionariesRebuild_Value()
        {
            IList<IEdge> inEdges = InEdgesRaw;
            Dictionary<string, object> edgesByValue =
                new Dictionary<string, object>(inEdges.Count);
            bool hasExplicitQueryValueSources =
                false;

            _InEdgesByValue = edgesByValue;

            foreach (IEdge edge in inEdges)
            {
                if (TryGetIndexableQueryValue(
                    edge.From,
                    out object queryValue))
                    AddEdgeToDictionary(
                        edgesByValue,
                        GetQueryDictionaryKey(
                            queryValue),
                        edge);
                else
                    hasExplicitQueryValueSources =
                        true;
            }

            inEdgesHaveExplicitQueryValueSources =
                hasExplicitQueryValueSources;
            InEdgesDictionariesNeedsRebuild_Value = false;
        }

        private void OutEdgesDictionariesRebuild_Value()
        {
            if (TryRebuildOutEdgesWithValueIndex())
                return;

            IList<IEdge> outEdges = OutEdges;
            Dictionary<string, object> edgesByValue =
                new Dictionary<string, object>(outEdges.Count);
            bool hasExplicitQueryValueTargets =
                false;

            _OutEdgesByValue = edgesByValue;

            foreach (IEdge edge in outEdges)
            {
                if (TryGetIndexableQueryValue(
                    edge.To,
                    out object queryValue))
                    AddEdgeToDictionary(
                        edgesByValue,
                        GetQueryDictionaryKey(
                            queryValue),
                        edge);
                else
                    hasExplicitQueryValueTargets =
                        true;
            }

            outEdgesHaveExplicitQueryValueTargets =
                hasExplicitQueryValueTargets;
            OutEdgesDictionariesNeedsRebuild_Value = false;
            CompleteInheritedOutIndexRebuild(
                InheritedOutIndexKind.Value);
        }

        private void InEdgesDictionariesRebuild_MetaAndValue()
        {
            IList<IEdge> inEdges = InEdgesRaw;
            Dictionary<GraphUtil.MetaAndValueKey, object> edgesByMetaAndValue =
                new Dictionary<GraphUtil.MetaAndValueKey, object>(inEdges.Count);
            bool hasExplicitQueryValueSources =
                false;

            _InEdgesByMetaAndValue = edgesByMetaAndValue;

            foreach (IEdge edge in inEdges)
            {
                if (TryGetIndexableQueryValue(
                    edge.From,
                    out object queryValue))
                    AddEdgeToDictionary(
                        edgesByMetaAndValue,
                        new GraphUtil.MetaAndValueKey(
                            edge.Meta?.Value,
                            queryValue),
                        edge);
                else
                    hasExplicitQueryValueSources =
                        true;
            }

            inEdgesHaveExplicitQueryValueSources =
                hasExplicitQueryValueSources;
            InEdgesDictionariesNeedsRebuild_MetaAndValue = false;
        }

        private void OutEdgesDictionariesRebuild_MetaAndValue()
        {
            if (TryRebuildOutEdgesWithMetaAndValueIndex())
                return;

            IList<IEdge> outEdges = OutEdges;
            Dictionary<GraphUtil.MetaAndValueKey, object> queryEdgesByMetaAndValue =
                new Dictionary<GraphUtil.MetaAndValueKey, object>(outEdges.Count);
            bool hasExplicitQueryValueTargets =
                false;

            _OutEdgesByMetaAndValue = queryEdgesByMetaAndValue;

            foreach (IEdge edge in outEdges)
            {
                if (TryGetIndexableQueryValue(
                    edge.To,
                    out object queryValue))
                    foreach (string queryMetaKey in
                        GetMetaQueryKeys(edge.Meta))
                        AddEdgeToDictionary(
                            queryEdgesByMetaAndValue,
                            new GraphUtil.MetaAndValueKey(
                                queryMetaKey,
                                queryValue),
                            edge);
                else
                    hasExplicitQueryValueTargets =
                        true;
            }

            outEdgesHaveExplicitQueryValueTargets =
                hasExplicitQueryValueTargets;
            OutEdgesDictionariesNeedsRebuild_MetaAndValue = false;
            CompleteInheritedOutIndexRebuild(
                InheritedOutIndexKind.QueryMetaAndValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void HandleLocalOutEdgeMutation(
            IEdge edge,
            bool added,
            bool allowIncremental)
        {
            byte indexMask = currentOutIndexMask;

            if (indexMask == 0)
            {
                if (outIndexMutationBudgetFallbackPending &&
                    outIndexMutationsSinceLastQuery < 256)
                    outIndexMutationsSinceLastQuery++;

                return;
            }

            if (outIndexMutationsSinceLastQuery < 256)
                outIndexMutationsSinceLastQuery++;

            if (consecutiveIncrementalOutIndexMutations >=
                incrementalOutIndexMutationBudget)
            {
                consecutiveIncrementalOutIndexMutations = 0;
                outIndexMutationBudgetFallbackPending =
                    true;
                currentOutIndexMask = 0;
                OutEdgesDictionariesNeedsRebuild = true;
                return;
            }

            if (indexMask == LogicalOutIndexMask &&
                allowIncremental &&
                (!HasInheritance || !AllowInheritance) &&
                ReferenceEquals(_OutEdges, OutEdgesRaw))
                return;

            HandleLocalOutEdgeMutationSlow(
                edge,
                added,
                allowIncremental);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void HandleLocalOutEdgeMutationSlow(
            IEdge edge,
            bool added,
            bool allowIncremental)
        {
            if (!allowIncremental ||
                !CanIncrementallyUpdateLocalOutIndexes(edge))
            {
                consecutiveIncrementalOutIndexMutations = 0;
                currentOutIndexMask = 0;
                OutEdgesDictionariesNeedsRebuild = true;
                return;
            }

            bool patchedAny =
                !OutEdgesDictionariesNeedsRebuild_Edges;
            bool patchSucceeded = true;
            string[] queryMetaKeys = null;

            if (!OutEdgesDictionariesNeedsRebuild_Meta)
            {
                if (_OutEdgesByMeta is
                    Dictionary<object, object>
                        directMetaIndex)
                {
                    patchSucceeded &=
                        ApplyIncrementalDictionaryMutation(
                            directMetaIndex,
                            GetQueryDictionaryKey(
                                edge.Meta.Value),
                            edge,
                            added);
                    patchedAny = true;
                }
                else
                    patchSucceeded = false;
            }

            if (!OutEdgesDictionariesNeedsRebuild_QueryMeta)
            {
                if (outEdgesByQueryMeta != null)
                {
                    queryMetaKeys ??=
                        GetMetaQueryKeys(edge.Meta);

                    foreach (string queryMetaKey
                        in queryMetaKeys)
                    {
                        patchSucceeded &=
                            ApplyIncrementalDictionaryMutation(
                                outEdgesByQueryMeta,
                                queryMetaKey,
                                edge,
                                added);
                    }

                    patchedAny = true;
                }
                else
                    patchSucceeded = false;
            }

            if (!OutEdgesDictionariesNeedsRebuild_Value)
            {
                if (_OutEdgesByValue != null)
                {
                    if (TryGetIndexableQueryValue(
                        edge.To,
                        out object queryValue))
                        patchSucceeded &=
                            ApplyIncrementalDictionaryMutation(
                                _OutEdgesByValue,
                                GetQueryDictionaryKey(
                                    queryValue),
                                edge,
                                added);
                    else if (added)
                        outEdgesHaveExplicitQueryValueTargets =
                            true;

                    patchedAny = true;
                }
                else
                    patchSucceeded = false;
            }

            if (!OutEdgesDictionariesNeedsRebuild_MetaAndValue)
            {
                if (_OutEdgesByMetaAndValue != null)
                {
                    if (TryGetIndexableQueryValue(
                        edge.To,
                        out object queryValue))
                    {
                        queryMetaKeys ??=
                            GetMetaQueryKeys(edge.Meta);

                        foreach (string queryMetaKey
                            in queryMetaKeys)
                        {
                            patchSucceeded &=
                                ApplyIncrementalDictionaryMutation(
                                    _OutEdgesByMetaAndValue,
                                    new GraphUtil.MetaAndValueKey(
                                        queryMetaKey,
                                        queryValue),
                                    edge,
                                    added);
                        }
                    }
                    else if (added)
                        outEdgesHaveExplicitQueryValueTargets =
                            true;

                    patchedAny = true;
                }
                else
                    patchSucceeded = false;
            }

            if (!patchSucceeded)
            {
                consecutiveIncrementalOutIndexMutations = 0;
                currentOutIndexMask = 0;
                OutEdgesDictionariesNeedsRebuild = true;
                return;
            }

            if (patchedAny)
            {
                consecutiveIncrementalOutIndexMutations++;
            }
        }

        protected virtual bool
            CanIncrementallyUpdateLocalOutIndexes(
                IEdge edge)
        {
            if (edge?.Meta == null ||
                edge.To == null ||
                Store.DetachState !=
                    DetachStateEnum.Attached)
                return false;

            if (HasInheritance && AllowInheritance)
                return false;

            if (InheritsInEdgeCount > 0)
                return false;

            if (GeneralUtil.CompareStrings(
                edge.Meta.Value,
                "$Inherits"))
                return false;

            return OutEdgesDictionariesNeedsRebuild_Edges ||
                ReferenceEquals(_OutEdges, OutEdgesRaw);
        }

        internal void SetIncrementalOutIndexMutationBudget(
            int mutationBudget)
        {
            if (mutationBudget < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(mutationBudget));

            incrementalOutIndexMutationBudget =
                mutationBudget;
            incrementalOutIndexMutationBudgetIsFixed =
                true;
            consecutiveIncrementalOutIndexMutations = 0;
            outIndexMutationsSinceLastQuery = 0;
            outIndexMutationBudgetFallbackPending = false;
        }

        internal void
            InvalidateOutIndexesAfterBatchMutation()
        {
            consecutiveIncrementalOutIndexMutations = 0;
            outIndexMutationsSinceLastQuery = 0;
            outIndexMutationBudgetFallbackPending = false;
            currentOutIndexMask = 0;
            OutEdgesDictionariesNeedsRebuild = true;
        }

        private void CompleteIncrementalOutIndexMutationBurst()
        {
            if (!incrementalOutIndexMutationBudgetIsFixed &&
                outIndexMutationBudgetFallbackPending &&
                outIndexMutationsSinceLastQuery >
                    incrementalOutIndexMutationBudget)
            {
                incrementalOutIndexMutationBudget =
                    outIndexMutationsSinceLastQuery >= 128
                    ? 256
                    : Math.Max(
                        outIndexMutationsSinceLastQuery * 2,
                        8);
            }

            consecutiveIncrementalOutIndexMutations = 0;
            outIndexMutationsSinceLastQuery = 0;
            outIndexMutationBudgetFallbackPending = false;
        }

        private static bool
            ApplyIncrementalDictionaryMutation<TKey>(
                Dictionary<TKey, object> dictionary,
                TKey key,
                IEdge edge,
                bool added)
            where TKey : notnull
        {
            if (added)
            {
                AddEdgeToIncrementalDictionary(
                    dictionary,
                    key,
                    edge);
                return true;
            }

            return RemoveEdgeFromIncrementalDictionary(
                dictionary,
                key,
                edge);
        }

        private static bool
            ApplyIncrementalDictionaryMutation<TKey>(
                Dictionary<TKey, EdgeBucket> dictionary,
                TKey key,
                IEdge edge,
                bool added)
            where TKey : notnull
        {
            if (added)
            {
                bool exists;
                ref EdgeBucket bucket =
                    ref CollectionsMarshal
                        .GetValueRefOrAddDefault(
                            dictionary,
                            key,
                            out exists);
                bucket.Add(
                    edge,
                    out _);

                return true;
            }

            ref EdgeBucket existingBucket =
                ref CollectionsMarshal
                    .GetValueRefOrNullRef(
                        dictionary,
                        key);

            if (Unsafe.IsNullRef(
                ref existingBucket))
                return false;

            EdgeBucketRemovalResult removalResult =
                existingBucket.Remove(edge);

            if (removalResult ==
                EdgeBucketRemovalResult.NotFound)
                return false;

            if (removalResult ==
                EdgeBucketRemovalResult.Emptied)
                dictionary.Remove(key);

            return true;
        }

        private static void
            AddEdgeToIncrementalDictionary<TKey>(
                Dictionary<TKey, object> dictionary,
                TKey key,
                IEdge edge)
            where TKey : notnull
        {
            bool exists;
            ref object dictionaryValue =
                ref CollectionsMarshal
                    .GetValueRefOrAddDefault(
                        dictionary,
                        key,
                        out exists);

            if (!exists)
            {
                dictionaryValue = edge;
                return;
            }

            if (dictionaryValue is
                List_VertexBase list)
            {
                list.Add(edge);
                return;
            }

            dictionaryValue = new List_VertexBase
            {
                (IEdge)dictionaryValue,
                edge
            };
        }

        private static bool
            RemoveEdgeFromIncrementalDictionary<TKey>(
                Dictionary<TKey, object> dictionary,
                TKey key,
                IEdge edge)
            where TKey : notnull
        {
            if (!dictionary.TryGetValue(
                key,
                out object dictionaryValue))
                return false;

            if (dictionaryValue is
                List_VertexBase list)
            {
                int removeIndex = -1;

                if (list.Count > 0 &&
                    ReferenceEquals(
                        list[list.Count - 1],
                        edge))
                    removeIndex = list.Count - 1;
                else
                    for (int index = 0;
                        index < list.Count;
                        index++)
                        if (ReferenceEquals(
                            list[index],
                            edge))
                        {
                            removeIndex = index;
                            break;
                        }

                if (removeIndex < 0)
                    return false;

                list.RemoveAt(removeIndex);

                if (list.Count == 1)
                {
                    dictionary[key] = list[0];
                }
                else if (list.Count == 0)
                {
                    dictionary.Remove(key);
                }

                return true;
            }

            if (!ReferenceEquals(
                dictionaryValue,
                edge))
                return false;

            dictionary.Remove(key);
            return true;
        }

        private static string GetQueryDictionaryKey(object value)
        {
            return value as string ?? value?.ToString() ?? "";
        }

        private static bool TryGetIndexableQueryValue(
            IVertex vertex,
            out object value)
        {
            if (RequiresExplicitQueryValueEvaluation(
                vertex))
            {
                value = null;
                return false;
            }

            value = vertex?.Value;
            return true;
        }

        private static bool
            RequiresExplicitQueryValueEvaluation(
                IVertex vertex)
        {
            return vertex is
                    IExplicitQueryValueVertex explicitVertex &&
                explicitVertex
                    .RequiresExplicitQueryValueEvaluation;
        }

        private static void AddEdgeToDictionary<TKey>(
            Dictionary<TKey, object> dictionary,
            TKey key,
            IEdge edge) where TKey : notnull
        {
            bool exists;
            ref object dictionaryValue = ref CollectionsMarshal.GetValueRefOrAddDefault(
                dictionary,
                key,
                out exists);

            if (!exists)
            {
                dictionaryValue = edge;
                return;
            }

            if (dictionaryValue is List_VertexBase list)
            {
                list.Add(edge);
                return;
            }

            List_VertexBase newList = new List_VertexBase
            {
                (IEdge)dictionaryValue,
                edge
            };

            dictionaryValue = newList;
        }

        private static void AddEdgeToDictionary<TKey>(
            Dictionary<TKey, EdgeBucket> dictionary,
            TKey key,
            IEdge edge)
            where TKey : notnull
        {
            bool exists;
            ref EdgeBucket bucket =
                ref CollectionsMarshal
                    .GetValueRefOrAddDefault(
                        dictionary,
                        key,
                        out exists);
            bucket.Add(
                edge,
                out _);
        }

        private static string[] GetMetaQueryKeys(IVertex metaVertex)
        {
            if (metaVertex == null)
                return emptyMetaQueryKeys;

            if (metaVertex is EasyVertex easyMetaVertex)
            {
                if (easyMetaVertex.metaQueryKeys == null)
                    easyMetaVertex.metaQueryKeys = CreateMetaQueryKeys(metaVertex);

                return easyMetaVertex.metaQueryKeys;
            }

            return CreateMetaQueryKeys(metaVertex);
        }

        private static string[] CreateMetaQueryKeys(IVertex metaVertex)
        {
            HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
            keys.Add(GetQueryDictionaryKey(metaVertex.Value));

            foreach (IVertex parent in VertexHelper.GetInheritParents(metaVertex))
                keys.Add(GetQueryDictionaryKey(parent.Value));

            return keys.ToArray();
        }

        private void InvalidateMetaQueryIndexesForThisAndInheritChildren(
            bool directMetaValueChanged)
        {
            HashSet<IVertex> affectedMetaVertices = VertexHelper.GetInheritChilds(this);
            affectedMetaVertices.Add(this);

            HashSet<IVertex> affectedSourceVertices = new HashSet<IVertex>();
            HashSet<IVertex> directMetaSourceVertices = directMetaValueChanged
                ? new HashSet<IVertex>()
                : null;

            foreach (IVertex metaVertex in affectedMetaVertices)
            {
                if (metaVertex is EasyVertex easyMetaVertex)
                    easyMetaVertex.metaQueryKeys = null;

                foreach (IEdge metaInEdge in metaVertex.MetaInEdgesRaw)
                    if (metaInEdge.From != null)
                    {
                        affectedSourceVertices.Add(metaInEdge.From);

                        if (directMetaValueChanged && metaVertex == this)
                        {
                            directMetaSourceVertices.Add(metaInEdge.From);

                            if (metaInEdge.To != null)
                                metaInEdge.To.InEdgesDictionariesNeedsRebuild = true;
                        }
                    }
            }

            foreach (IVertex sourceVertex in affectedSourceVertices)
                if (sourceVertex is EasyVertex easySourceVertex)
                    easySourceVertex
                        .MarkMetaQueryIndexesNeedRebuild(
                            directMetaSourceVertices != null &&
                            directMetaSourceVertices.Contains(
                                sourceVertex));
                else
                    sourceVertex.OutEdgesDictionariesNeedsRebuild = true;
        }

        private void MarkMetaQueryIndexesNeedRebuild(bool directMeta)
        {
            OutEdgesDictionariesNeedsRebuild_QueryMeta = true;
            OutEdgesDictionariesNeedsRebuild_MetaAndValue = true;

            if (directMeta)
                OutEdgesDictionariesNeedsRebuild_Meta = true;

            if (InheritsInEdgeCount == 0)
                return;

            Interlocked.Increment(
                ref outQueryMetaGeneration);

            if (directMeta)
                Interlocked.Increment(
                    ref outDirectMetaGeneration);

            IncrementInheritanceDependencyEpoch();
        }

        // edge = new Edge in Attached state
        // OutEdgesRaw.OnAdd
        //    edge.Meta.MetaInEdgesRaw.Add(edge);                                
        //    edge.To.InEdgesRaw.Add(edge);
        // this.AttachEdge(edge)
        // edge.To.AttachInEdge(edge)

        public override IEdge AddEdge(IVertex metaVertex, IVertex destVertex)
        {
           /* if (
                metaVertex != null && GeneralUtil.CompareStrings(metaVertex, "If") 
                //&& (String)destVertex.Value == "Arrow"
                )
            {
                int x = 0;
            }*/

            if (DisposedState == DisposeStateEnum.Disposed)
                throw new Exception("Vertex not live");

            if (destVertex == null)
                destVertex = MinusZero.Instance.Empty; // can be    

            if (destVertex.DisposedState == DisposeStateEnum.Disposed)
                throw new Exception("Vertex not live");

            ValidateInheritanceEdge(metaVertex, destVertex);

            EdgeBase ne = new EasyEdge(this, metaVertex, destVertex);

            OutEdgesRaw.Add(ne);

            AttachEdge(ne);
            destVertex.AttachInEdge(ne);

            if (CanEmitGraphChangeEvents)
                ExecutionFlowHelper.AddTransactionAtom(new GraphChangeTransactionAtom(
                    this,
                    AtomGraphChangeTypeEnum.EdgeAdded,
                    null,
                    null,
                    ne));

            return ne;
        }

        private void ValidateInheritanceEdge(IVertex metaVertex, IVertex parentVertex)
        {
            if (metaVertex == null ||
                !GeneralUtil.CompareStrings(metaVertex.Value, "$Inherits"))
                return;

            if (ReferenceEquals(this, parentVertex))
                throw new InvalidOperationException(
                    "A vertex cannot inherit from itself.");

            if (VertexHelper.GetInheritParents(parentVertex).Contains(this))
                throw new InvalidOperationException(
                    "The $Inherits edge would create an inheritance cycle.");
        }

        public override void AttachInEdge(IEdge edge)
        {
            if (GeneralUtil.CompareStrings(edge.Meta.Value, "$Inherits"))
                InheritsInEdges.Add(edge);
        }

        public override void AttachEdge(IEdge edge)
        {            
            if (GeneralUtil.CompareStrings(edge.Meta.Value, "$Inherits"))
            {
                InheritsOutEdges.Add(edge);                

                HasInheritance = true;

                InvalidateMetaQueryIndexesForThisAndInheritChildren(false);
            }

            if (GeneralUtil.CompareStrings(
                edge.Meta.Value,
                "$NoInherit"))
                NotifyNoInheritConsumersChanged();

            if (GeneralUtil.CompareStrings(edge.Meta.Value, "$GraphChangeTrigger"))
            {
                HasOnlyNonTransactedRootVertexEventsEdgeNeedsRebuild = true;
                GraphChangeTriggerWatcher.AddGraphChangeTrigger(edge);
            }

            if (GeneralUtil.CompareStrings(edge.To.Value, "OnlyNonTransactedRootVertexEvents") ||
                GeneralUtil.CompareStrings(edge.Meta.Value, "Listener"))
                OnlyNonTransactedRootVertexEvents_Listener_AddedRemoved();
        }

        public override void DetachInEdge(IEdge edge)
        {
            if (GeneralUtil.CompareStrings(edge.Meta.Value, "$Inherits"))
                InheritsInEdges.Remove(edge);
        }

        public override void DetachEdge(IEdge edge)
        {
            if (edge.Meta != null)
            {
                if (GeneralUtil.CompareStrings(edge.Meta.Value, "$Inherits"))
                {
                    InheritsOutEdges.Remove(edge);                    

                    if (InheritsOutEdgeCount == 0)
                        HasInheritance = false;

                    InvalidateMetaQueryIndexesForThisAndInheritChildren(false);
                }

                if (GeneralUtil.CompareStrings(
                    edge.Meta.Value,
                    "$NoInherit"))
                    NotifyNoInheritConsumersChanged();

                if (GeneralUtil.CompareStrings(edge.Meta.Value, "$GraphChangeTrigger")) {
                    HasOnlyNonTransactedRootVertexEventsEdgeNeedsRebuild = true;
                    GraphChangeTriggerWatcher.RemoveGraphChangeTrigger(edge);
                }

                if (GeneralUtil.CompareStrings(edge.To.Value, "OnlyNonTransactedRootVertexEvents") ||
                    GeneralUtil.CompareStrings(edge.Meta.Value, "Listener"))
                    OnlyNonTransactedRootVertexEvents_Listener_AddedRemoved();
            }
        }

        protected void OnlyNonTransactedRootVertexEvents_Listener_AddedRemoved()
        {
            foreach (IEdge e in GraphUtil.GetQueryIn(this, "$GraphChangeTrigger", null))
                if (e.From is EasyVertex)
                {
                    EasyVertex ev = (EasyVertex)e.From;

                    ev.HasOnlyNonTransactedRootVertexEventsEdgeNeedsRebuild = true;
                }
        }

        public override void AddEdgesList(IEnumerable<IEdge> edges)
        {            
            foreach (IEdge e in edges) // possibly not optimal implementation
                AddEdge(e.Meta, e.To);
        }

        // OutEdgesRaw.OnRemove
        //      edge.Meta.MetaInEdgesRaw.Remove(edge);
        //      edge.To.InEdgesRaw.Remove(edge);
        //          edge.To.CheckIfShouldDispose();
        //      edge.From.DetachEdge(item);
        public override void DeleteEdge(IEdge _edge)
        {
            if (_edge == null)
                return;

            if (DisposedState == DisposeStateEnum.Disposed)
                throw new Exception("Vertex not live");            

            IEdge edge = edgeDictionaries.Out.Get(_edge);

            if (edge == null)
                foreach (IEdge e in OutEdges)
                    if (e.From == _edge.From && e.Meta == _edge.Meta && e.To == _edge.To)
                        edge = e;

            if (edge != null)
            {                
                OutEdgesRaw.Remove(edge);

                if(CanEmitGraphChangeEvents)
                    ExecutionFlowHelper.AddTransactionAtom(new GraphChangeTransactionAtom(
                        this,
                        AtomGraphChangeTypeEnum.EdgeRemoved,
                        null,
                        null,
                        edge));
            }
        }

        public override void DeleteEdgesList(IEnumerable<IEdge> edges)
        {            
            foreach (IEdge e in edges) // possibly not optimal implementation
                DeleteEdge(e); // Meta/To check to be performed
        }       

        private const int DefaultQueryParseCacheCapacity = 512;
        private const int DefaultMetaQueryParseCacheCapacity = 128;

        private static readonly object queryParseCacheSynchronizationRoot =
            new object();
        private static readonly BoundedQueryParseCache queryParseCache =
            new BoundedQueryParseCache(
                DefaultQueryParseCacheCapacity,
                queryParseCacheSynchronizationRoot);
        private static readonly BoundedQueryParseCache metaQueryParseCache =
            new BoundedQueryParseCache(
                DefaultMetaQueryParseCacheCapacity,
                queryParseCacheSynchronizationRoot);

        public static int QueryParseCacheCapacity
        {
            get { return queryParseCache.Capacity; }
            set { queryParseCache.Capacity = value; }
        }

        public static int MetaQueryParseCacheCapacity
        {
            get { return metaQueryParseCache.Capacity; }
            set { metaQueryParseCache.Capacity = value; }
        }

        public static int QueryParseCacheEntryCount
        {
            get { return queryParseCache.State.Count; }
        }

        public static int MetaQueryParseCacheEntryCount
        {
            get { return metaQueryParseCache.State.Count; }
        }

        public static void ResetQueryParseCaches()
        {
            lock (queryParseCacheSynchronizationRoot)
            {
                queryParseCache.Clear();
                metaQueryParseCache.Clear();
            }
        }

        public override void Dispose()
        {
            if (DisposedState != DisposeStateEnum.Live)
                return;

            DisposedState = DisposeStateEnum.Disposing;

            GraphUtil.Debug(this, DebugOperationEnum.Dispose);

            DeleteAllInEdges();
            DeleteAllMetaInEdges();
            DeleteAllEdges();

            Store.RemoveVertexIdentifier(this);

            DisposedState = DisposeStateEnum.Disposed;
        }

        public void DeleteAllInEdges()
        {
            if (DisposedState == DisposeStateEnum.Disposed)
                throw new Exception("Vertex not live");

            foreach (IEdge edge in InEdgesRaw.ToList())
            {
                InEdgesRaw.Remove(edge);                

                if (CanEmitGraphChangeEvents)
                    ExecutionFlowHelper.AddTransactionAtom(new GraphChangeTransactionAtom(
                        edge.From,
                        AtomGraphChangeTypeEnum.OutputEdgeDisposed,
                        null,
                        null,
                        edge));
            }
        }

        public void DeleteAllMetaInEdges()
        {
            if (DisposedState == DisposeStateEnum.Disposed)
                throw new Exception("Vertex not live");

            foreach (IEdge edge in MetaInEdgesRaw.ToList())
                MetaInEdgesRaw.Remove(edge);
        }

        public void DeleteAllEdges()
        {
            if (DisposedState == DisposeStateEnum.Disposed)
                throw new Exception("Vertex not live");

            foreach (IEdge edge in OutEdgesRaw.ToList()) {             
                OutEdgesRaw.Remove(edge);                

                //FireChange(new VertexChangeEventArgs(VertexChangeType.EdgeRemoved, edge)); // moved from before edge.Meta.DeleteMetaInEdge(edge); XXX !!!
            }            
        }

        public void NotifyOutEdgesChanged()
        {
            if (InheritsInEdgeCount == 0)
                return;

            Interlocked.Increment(
                ref outStructureGeneration);
            IncrementInheritanceDependencyEpoch();
        }

        private void NotifyNoInheritConsumersChanged()
        {
            NotifyNoInheritConsumersChanged(this);
        }

        private static void NotifyNoInheritConsumersChanged(
            IVertex noInheritMarkedMeta)
        {
            if (noInheritMarkedMeta.MetaInEdgesRaw.Count == 0)
            {
                return;
            }

            HashSet<IVertex> affectedSources =
                new HashSet<IVertex>();

            foreach (IEdge edge in
                noInheritMarkedMeta.MetaInEdgesRaw)
                if (edge.From != null)
                    affectedSources.Add(edge.From);


            foreach (IVertex source in affectedSources)
            {
                if (source is EasyVertex easySource)
                {
                    easySource.NotifyOutEdgesChanged();
                    continue;
                }

                HashSet<IVertex> children =
                    VertexHelper.GetInheritChilds(
                        source);

                foreach (IVertex child in children)
                    child.OutEdgesDictionariesNeedsRebuild =
                        true;
            }
        }

        private void NotifyNoInheritMarkerMetaValueChanged()
        {
            HashSet<IVertex> affectedMarkerMetas =
                VertexHelper.GetInheritChilds(this);
            affectedMarkerMetas.Add(this);

            HashSet<IVertex> markedMetaVertices =
                new HashSet<IVertex>();

            foreach (IVertex affectedMarkerMeta in
                affectedMarkerMetas)
                foreach (IEdge markerEdge in
                    affectedMarkerMeta.MetaInEdgesRaw)
                    if (markerEdge.From != null)
                        markedMetaVertices.Add(
                            markerEdge.From);

            foreach (IVertex markedMetaVertex in
                markedMetaVertices)
                NotifyNoInheritConsumersChanged(
                    markedMetaVertex);
        }

        public IDictionary<object, object> GetOutOdgesByMeta()
        {
            CompleteIncrementalOutIndexMutationBurst();

            if (!OutEdgesDictionariesNeedsRebuild_Meta)
            {
                long currentEpoch =
                    CurrentInheritanceDependencyEpoch;

                if (Volatile.Read(
                        ref directMetaDependencyCheckedEpoch) !=
                    currentEpoch)
                {
                    EnsureInheritedOutIndexCurrentSlow(
                        InheritedOutIndexKind.DirectMeta,
                        directMetaDependencyStamp,
                        ref directMetaDependencyCheckedEpoch,
                        currentEpoch);
                }
            }

            if (OutEdgesDictionariesNeedsRebuild_Meta)
                OutEdgesDictionariesRebuild_Meta();

            return OutEdgesByMeta;
        }

        private void QueryOutEdgesWithExplicitQueryValues(
            object meta,
            object to,
            out IEdge result,
            out IList<IEdge> results)
        {
            result = null;
            results = null;
            string metaKey =
                meta == null
                    ? null
                    : GetQueryDictionaryKey(meta);
            string toKey =
                GetQueryDictionaryKey(to);

            foreach (IEdge edge in OutEdges)
            {
                if (metaKey != null &&
                    !DoesOutEdgeMatchQueryMeta(
                        edge,
                        metaKey))
                    continue;

                if (!StringComparer.Ordinal.Equals(
                    GetQueryDictionaryKey(
                        edge.To?.Value),
                    toKey))
                    continue;

                AddQueryResult(
                    edge,
                    ref result,
                    ref results);
            }
        }

        private void QueryInEdgesWithExplicitQueryValues(
            object meta,
            object from,
            out IEdge result,
            out IList<IEdge> results)
        {
            result = null;
            results = null;
            string metaKey =
                meta == null
                    ? null
                    : GetQueryDictionaryKey(meta);
            string fromKey =
                GetQueryDictionaryKey(from);

            foreach (IEdge edge in InEdgesRaw)
            {
                if (metaKey != null &&
                    !StringComparer.Ordinal.Equals(
                        GetQueryDictionaryKey(
                            edge.Meta?.Value),
                        metaKey))
                    continue;

                if (!StringComparer.Ordinal.Equals(
                    GetQueryDictionaryKey(
                        edge.From?.Value),
                    fromKey))
                    continue;

                AddQueryResult(
                    edge,
                    ref result,
                    ref results);
            }
        }

        private static bool DoesOutEdgeMatchQueryMeta(
            IEdge edge,
            string metaKey)
        {
            foreach (string queryMetaKey in
                GetMetaQueryKeys(edge.Meta))
                if (StringComparer.Ordinal.Equals(
                    queryMetaKey,
                    metaKey))
                    return true;

            return false;
        }

        private static void AddQueryResult(
            IEdge edge,
            ref IEdge result,
            ref IList<IEdge> results)
        {
            if (result == null &&
                results == null)
            {
                result = edge;
                return;
            }

            if (results == null)
            {
                results =
                    new List_VertexBase
                    {
                        result,
                        edge
                    };
                result = null;
                return;
            }

            results.Add(edge);
        }

        public override void QueryOutEdges(object meta, object to, out IEdge result, out IList<IEdge> results)
         {
            CompleteIncrementalOutIndexMutationBurst();
            result = null;
            results = null;

            if (meta!=null && to == null)
            {
                if (!OutEdgesDictionariesNeedsRebuild_QueryMeta)
                {
                    long currentEpoch =
                        CurrentInheritanceDependencyEpoch;

                    if (Volatile.Read(
                            ref queryMetaDependencyCheckedEpoch) !=
                        currentEpoch)
                    {
                        EnsureInheritedOutIndexCurrentSlow(
                            InheritedOutIndexKind.QueryMeta,
                            queryMetaDependencyStamp,
                            ref queryMetaDependencyCheckedEpoch,
                            currentEpoch);
                    }
                }

                if (OutEdgesDictionariesNeedsRebuild_QueryMeta || outEdgesByQueryMeta == null)
                    OutEdgesDictionariesRebuild_QueryMeta();

                string metaKey = GetQueryDictionaryKey(meta);
                ref EdgeBucket bucket =
                    ref CollectionsMarshal
                        .GetValueRefOrNullRef(
                            outEdgesByQueryMeta,
                            metaKey);

                if (Unsafe.IsNullRef(ref bucket))
                    return;

                bucket.GetQueryResult(
                    out result,
                    out results);

                return;
            }

            if (meta == null && to != null)
            {
                if (!OutEdgesDictionariesNeedsRebuild_Value)
                {
                    long currentEpoch =
                        CurrentInheritanceDependencyEpoch;

                    if (Volatile.Read(
                            ref valueDependencyCheckedEpoch) !=
                        currentEpoch)
                    {
                        EnsureInheritedOutIndexCurrentSlow(
                            InheritedOutIndexKind.Value,
                            valueDependencyStamp,
                            ref valueDependencyCheckedEpoch,
                            currentEpoch);
                    }
                }

                if (OutEdgesDictionariesNeedsRebuild_Value)
                    OutEdgesDictionariesRebuild_Value();

                if (outEdgesHaveExplicitQueryValueTargets)
                {
                    QueryOutEdgesWithExplicitQueryValues(
                        null,
                        to,
                        out result,
                        out results);
                    return;
                }

                string toKey = GetQueryDictionaryKey(to);
                object val;

                if (!OutEdgesByValue.TryGetValue(
                    toKey,
                    out val))
                    return;

                if (val is List_VertexBase list)
                    results = list;
                else
                    result = (IEdge)val;

                return;
            }

            if (meta != null && to != null)
            {
                if (!OutEdgesDictionariesNeedsRebuild_MetaAndValue)
                {
                    long currentEpoch =
                        CurrentInheritanceDependencyEpoch;

                    if (Volatile.Read(
                            ref metaAndValueDependencyCheckedEpoch) !=
                        currentEpoch)
                    {
                        EnsureInheritedOutIndexCurrentSlow(
                            InheritedOutIndexKind.QueryMetaAndValue,
                            metaAndValueDependencyStamp,
                            ref metaAndValueDependencyCheckedEpoch,
                            currentEpoch);
                    }
                }

                if (OutEdgesDictionariesNeedsRebuild_MetaAndValue)
                    OutEdgesDictionariesRebuild_MetaAndValue();

                if (outEdgesHaveExplicitQueryValueTargets)
                {
                    QueryOutEdgesWithExplicitQueryValues(
                        meta,
                        to,
                        out result,
                        out results);
                    return;
                }

                GraphUtil.MetaAndValueKey searchKey =
                    new GraphUtil.MetaAndValueKey(meta, to);
                object val;

                if (!OutEdgesByMetaAndValue.TryGetValue(
                    searchKey,
                    out val))
                    return;

                if (val is List_VertexBase list)
                    results = list;
                else
                    result = (IEdge)val;

                return;
            }

            results = OutEdges.ToList();
        }

        public override void QueryInEdges(object meta, object from, out IEdge result, out IList<IEdge> results)
        {
            result = null;
            results = null;

            if (meta != null && from == null)
            {
                if (InEdgesDictionariesNeedsRebuild_Meta)
                    InEdgesDictionariesRebuild_Meta();

                string metaKey = GetQueryDictionaryKey(meta);
                object val;

                if (!InEdgesByMeta.TryGetValue(metaKey, out val))
                    return;

                if (val is List_VertexBase list)
                    results = list;
                else
                    result = (IEdge)val;

                return;
            }

            if (meta == null && from != null)
            {
                if (InEdgesDictionariesNeedsRebuild_Value)
                    InEdgesDictionariesRebuild_Value();

                if (inEdgesHaveExplicitQueryValueSources)
                {
                    QueryInEdgesWithExplicitQueryValues(
                        null,
                        from,
                        out result,
                        out results);
                    return;
                }

                string fromKey = GetQueryDictionaryKey(from);
                object val;

                if (!InEdgesByValue.TryGetValue(
                    fromKey,
                    out val))
                    return;

                if (val is List_VertexBase list)
                    results = list;
                else
                    result = (IEdge)val;

                return;
            }

            if (meta != null && from != null)
            {
                if (InEdgesDictionariesNeedsRebuild_MetaAndValue)
                    InEdgesDictionariesRebuild_MetaAndValue();

                if (inEdgesHaveExplicitQueryValueSources)
                {
                    QueryInEdgesWithExplicitQueryValues(
                        meta,
                        from,
                        out result,
                        out results);
                    return;
                }

                GraphUtil.MetaAndValueKey searchKey =
                    new GraphUtil.MetaAndValueKey(meta, from);
                object val;

                if (!InEdgesByMetaAndValue.TryGetValue(
                    searchKey,
                    out val))
                    return;

                if (val is List_VertexBase list)
                    results = list;
                else
                    result = (IEdge)val;

                return;
            }

            results = InEdgesRaw.ToList();
        }

        public override IVertex Get(bool metaMode, string query)
        {
            QueryParseCacheLease queryLease =
                GetParsedQuery(
                    metaMode,
                    query,
                    out bool parseFailed);

            try
            {
                if (parseFailed)
                    return null;

                return MinusZero.Instance.DefaultExecuter.Get(
                    metaMode,
                    this,
                    queryLease.Vertex);
            }
            finally
            {
                queryLease.Dispose();
            }
        }

        public override IVertex GetAll(bool metaMode, string query)
        {
            QueryParseCacheLease queryLease =
                GetParsedQuery(
                    metaMode,
                    query,
                    out bool parseFailed);

            try
            {
                if (parseFailed)
                    return null;

                return MinusZero.Instance.DefaultExecuter.GetAll(
                    metaMode,
                    this,
                    queryLease.Vertex);
            }
            finally
            {
                queryLease.Dispose();
            }
        }

        private static QueryParseCacheLease GetParsedQuery(
            bool metaMode,
            string query,
            out bool parseFailed)
        {
            BoundedQueryParseCache cache = metaMode
                ? metaQueryParseCache
                : queryParseCache;
            bool factoryParseFailed = false;

            QueryParseCacheLease queryLease =
                cache.GetOrCreate(
                    query,
                    () =>
                    {
                        IVertex queryVertex =
                            MinusZero.Instance.CreateTempVertex();

                        try
                        {
                            IEdge baseEdge;
                            IVertex parseError =
                                MinusZero.Instance.DefaultFormalTextParser.Parse(
                                    new EdgeBase(
                                        null,
                                        null,
                                        queryVertex),
                                    query,
                                    CodeRepresentationEnum.VertexAndManyLines,
                                    out baseEdge);

                            factoryParseFailed =
                                parseError != null &&
                                parseError.Count() > 0;

                            return new QueryParseCacheValue(
                                queryVertex,
                                !factoryParseFailed);
                        }
                        catch
                        {
                            queryVertex.Dispose();
                            throw;
                        }
                    });


            parseFailed = factoryParseFailed;
            return queryLease;
        }
        
        public override IVertex Get(bool metaMode, IVertex expression)
        {
            return MinusZero.Instance.DefaultExecuter.Get(metaMode, this, expression);
        }

        public override IVertex GetAll(bool metaMode, IVertex expression)
        {
            return MinusZero.Instance.DefaultExecuter.GetAll(metaMode, this, expression);
        }

        public override INoInEdgeInOutVertexVertex Execute(IExecution exe)
        {
            return ExecutionFlowHelper.Execute(this, exe);
        }

        protected void VertexInit_First(
            bool useSpecializedStackStorage = false)
        {
            edgeDictionaries = new EdgeDictionaries(
                this,
                useSpecializedStackStorage);

            if (useSpecializedStackStorage)
            {
                inheritsInEdges = null;
                inheritsOutEdges = null;
            }
            else
            {
                InheritsInEdges = new List<IEdge>();
                InheritsOutEdges = new List<IEdge>();
            }

            InEdgesDictionariesNeedsRebuild = true;
            OutEdgesDictionariesNeedsRebuild = true;

            if (useSpecializedStackStorage)
            {
                _Value = "";
                return;
            }

            bool tempCanEmitGraphChangeEvents = CanEmitGraphChangeEvents;

            CanEmitGraphChangeEvents = false;

            Value = "";

            CanEmitGraphChangeEvents = tempCanEmitGraphChangeEvents;
        }

        static object lock_object = new object();        
        protected virtual void VertexInit()
        {
            VertexInit(
                VertexIdentifierRegistrationMode.Registered);
        }

        private protected void VertexInit(
            VertexIdentifierRegistrationMode registrationMode,
            bool useSpecializedStackStorage = false)
        {
            lock (lock_object)
            {
                VertexInit_First(
                    useSpecializedStackStorage);

                _Identifier = Store.VertexIdentifierCount++;

                //Store.VertexIdentifierCount += RND.Next(10) + 1;

                //_Identifier = Store.VertexIdentifierCount;

                GraphUtil.Debug(this, DebugOperationEnum.Init);

                if (registrationMode ==
                    VertexIdentifierRegistrationMode.Registered)
                {
                    Store.StoreVertexIdentifier(this);
                }
            }
        }

        public override void ExecuteSecondStageCommitAction()
        {
            if (ShouldDispose())
            {
                Dispose();
            }
        }

        public EasyVertex(IStore _store) : base(_store)
        {
            VertexInit();
        }

        private protected EasyVertex(
            IStore _store,
            VertexIdentifierRegistrationMode registrationMode,
            bool useSpecializedStackStorage = false)
            : base(_store)
        {
            VertexInit(
                registrationMode,
                useSpecializedStackStorage);
        }

        public EasyVertex(IStore _store, object toBeIdentifier) : base(_store)
        {
            DisposedState = DisposeStateEnum.Live;

            _Identifier = toBeIdentifier;

            VertexInit_First();            

            if (toBeIdentifier is int)
            {
                int val = (int)toBeIdentifier + 1;

                if (val > Store.VertexIdentifierCount)
                    Store.VertexIdentifierCount = val;
            }

            Store.StoreVertexIdentifier(this);
        }

        bool ShouldDispose()
        {
            if (DisposedState != DisposeStateEnum.Live)
                return false;

            int cumulativeEdgesCount = 0;

            cumulativeEdgesCount += edgeDictionaries.InCount;
            cumulativeEdgesCount += edgeDictionaries.MetaInCount;

            if (cumulativeEdgesCount == 0 && ExternalReferenceCount == 0
                && Store.DetachState == DetachStateEnum.Attached
                && !IsRoot)          
                return true;            

            return false;
        }

        public override void CheckIfShouldDispose()
        {
            if(ShouldDispose())
                ExecutionFlowHelper.AddSecondStageCommitAction(edgeDictionaries.Vertex);
        }

        public void ClearDictionaries()
        {
            this.InEdgesDictionariesNeedsRebuild = true;
            
            consecutiveIncrementalOutIndexMutations = 0;
            outIndexMutationsSinceLastQuery = 0;
            outIndexMutationBudgetFallbackPending = false;
            currentOutIndexMask = 0;
            this.OutEdgesDictionariesNeedsRebuild = true;
            
            //   InEdgesRaw.Clear();
            // OutEdgesRaw.Clear();
            //MetaInEdgesRaw.Clear();


            _OutEdgesByMeta = null;
            outEdgesByQueryMeta = null;
            metaQueryKeys = null;
            _OutEdgesByValue = null;
            _OutEdgesByMetaAndValue = null;
            _InEdgesByMeta = null;
            _InEdgesByValue = null;
            _InEdgesByMetaAndValue = null;
            outEdgesHaveExplicitQueryValueTargets =
                false;
            inEdgesHaveExplicitQueryValueSources =
                false;
        }
    }
}
