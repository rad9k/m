using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.Util;
using m0.ZeroCode.Helpers;
using Microsoft.AspNetCore.StaticAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace m0.ZeroTypes
{
    public class VertexOperations
    {
        static string[] NoCopy_MetaValue = {"$GraphChangeTrigger"};
        static string[] NoCopy_VertexIsValue = { "GraphChangeTrigger" };

        // Copy/Move-and-replace similarity tuning (static, tunable). Cutoff is the minimum similarity
        // score [0..1] for an old/new pair to be considered a match.
        public static double ReplaceSimilarityCutoff = 0.5;
        public static double Replace_ValueWeight = 0.6;          // weight of value (name) similarity in local score
        public static double Replace_IsWeight = 0.4;             // weight of $Is overlap in local score
        public static double Replace_StructureWeight = 0.6;      // blend: how much recursive child structure counts vs local
        public static double Replace_MetaMismatchPenalty = 0.85; // multiplier applied to a child match when its edge meta differs
        public static int Replace_MaxMatchDepth = 8;             // recursion cap for structural similarity

        public static void CopyEdgesSet(IEnumerable<IEdge> edgesToCopy, IVertex copyTo)
        {
            List<IEdge> roots = edgesToCopy.ToList();

            Dictionary<IVertex, IVertex> oldToNew = new Dictionary<IVertex, IVertex>();

            CopySubGraphIntoVertex(roots, copyTo, oldToNew);

            MinusZero.Instance.Log(1, "VertexOperations.CopyVertex",
                "roots=" + roots.Count + " scopeVertices=" + oldToNew.Count);
        }

        // MoveSet: same copy as CopyVertex (subgraph ending at links, meta remapped when inside the
        // copied scope, copies attached under moveTo), then a full relocation of the original fragment:
        //  - every external referrer of a copied vertex (in-edge or meta-in-edge) is repointed to the copy,
        //  - the original input edges are deleted from their From vertices,
        //  - the original subgraph loses its incoming references and disposes.
        public static void MoveEdgesSet(IEnumerable<IEdge> edgesToMove, IVertex moveTo)
        {
            List<IEdge> roots = edgesToMove.ToList();

            HashSet<IEdge> inputEdges = new HashSet<IEdge>(roots);

            Dictionary<IVertex, IVertex> oldToNew = new Dictionary<IVertex, IVertex>();

            CopySubGraphIntoVertex(roots, moveTo, oldToNew);

            // Repin external referrers onto the copies. These are real graph changes, so they are
            // intentionally NOT hidden from graph change watchers. Edges internal to the scope are
            // already recreated among the copies; the input edges are skipped here because they are
            // deleted below and replaced by the moveTo connectors.
            foreach (KeyValuePair<IVertex, IVertex> oldNew in oldToNew)
            {
                IVertex oldVertex = oldNew.Key;
                IVertex newVertex = oldNew.Value;

                foreach (IEdge inEdge in oldVertex.InEdgesRaw.ToList())
                {
                    if (oldToNew.ContainsKey(inEdge.From) || inputEdges.Contains(inEdge))
                        continue;

                    IVertex meta = oldToNew.ContainsKey(inEdge.Meta) ? oldToNew[inEdge.Meta] : inEdge.Meta;

                    inEdge.From.AddEdge(meta, newVertex);
                    inEdge.From.DeleteEdge(inEdge);
                }

                foreach (IEdge metaInEdge in oldVertex.MetaInEdgesRaw.ToList())
                {
                    if (oldToNew.ContainsKey(metaInEdge.From) || inputEdges.Contains(metaInEdge))
                        continue;

                    IVertex to = oldToNew.ContainsKey(metaInEdge.To) ? oldToNew[metaInEdge.To] : metaInEdge.To;

                    metaInEdge.From.AddEdge(newVertex, to);
                    metaInEdge.From.DeleteEdge(metaInEdge);
                }
            }

            // Delete the original input edges from their real From vertices. The original subgraph
            // then loses its incoming references and disposes.
            foreach (IEdge e in roots)
                if (e.From != null && e.From.DisposedState == DisposeStateEnum.Live)
                    e.From.DeleteEdge(e);

            MinusZero.Instance.Log(1, "VertexOperations.MoveVertex",
                "roots=" + roots.Count + " scopeVertices=" + oldToNew.Count);
        }

        // CopyAndReplaceSet: builds a new subgraph modeled on the source under replaceTo and reconciles
        // it with a "similar" subgraph already present there; the source is left intact (copied).
        public static void CopyAndReplaceEdgesSet(IEnumerable<IEdge> edgesToReplace, IVertex replaceTo)
        {
            CopyOrMoveAndReplace(edgesToReplace.ToList(), replaceTo, false);
        }

        // MoveAndReplaceSet: like CopyAndReplaceVertex, but the source is consumed (moved): the source's
        // external referrers are repinned onto the new copies and the input edges are deleted.
        public static void MoveAndReplaceEdgesSet(IEnumerable<IEdge> edgesToReplace, IVertex replaceTo)
        {
            CopyOrMoveAndReplace(edgesToReplace.ToList(), replaceTo, true);
        }

        // Shared core. Builds the new subgraph modeled on the source under replaceTo, then reconciles it
        // with the existing ("old") similar subgraph already present under replaceTo:
        //  - locate the old similar subgraph (matched from the input edges),
        //  - build the new subgraph from the source under replaceTo,
        //  - compute a 1:1 anchor-first mapping old -> new with a fuzzy similarity measure tolerant to
        //    renames, meta changes and inserted/removed levels,
        //  - repin external referrers of the matched old vertices onto the new ones (order preserved),
        //  - when moveSource is true, also relocate the source: repin the source's external referrers
        //    onto the new copies and delete the input edges (source consumed),
        //  - delete the old connectors; unmatched old (and, on move, source) vertices are cut off and
        //    disposed once nothing references them.
        // Repins are applied first (no disposal), then the old/source connectors are deleted, so every
        // repin still sees its target alive.
        static void CopyOrMoveAndReplace(List<IEdge> roots, IVertex replaceTo, bool moveSource)
        {
            Dictionary<IVertex, Dictionary<IVertex, double>> memo = new Dictionary<IVertex, Dictionary<IVertex, double>>();

            // 1. Find the existing (old) similar subgraph roots under replaceTo, BEFORE adding the new copies.
            List<IEdge> oldRootEdges = FindOldRootEdges(roots, replaceTo, memo);

            HashSet<IVertex> oldScope = new HashSet<IVertex>();
            foreach (IEdge oldRootEdge in oldRootEdges)
                CollectCopyScope(oldRootEdge.To, oldScope);

            // 2. Build the new subgraph from the source under replaceTo.
            Dictionary<IVertex, IVertex> sourceToNew = new Dictionary<IVertex, IVertex>();
            CopySubGraphIntoVertex(roots, replaceTo, sourceToNew);

            HashSet<IVertex> newScope = new HashSet<IVertex>(sourceToNew.Values);
            HashSet<IVertex> sourceScope = new HashSet<IVertex>(sourceToNew.Keys);

            // 3. Match old vs new (anchor-first, 1:1).
            Dictionary<IVertex, IVertex> oldToNew = MatchSubGraphs(oldScope, newScope, memo);

            // 4. Collect repins (order-preserving). The input and old-root edges are never repinned; edges
            //    internal to the old/new/source scopes are skipped too.
            HashSet<IEdge> excluded = new HashSet<IEdge>(oldRootEdges);
            foreach (IEdge inputEdge in roots)
                excluded.Add(inputEdge);

            Dictionary<IVertex, List<EdgeRewrite>> byFrom = new Dictionary<IVertex, List<EdgeRewrite>>();

            // Old-side: external referrers of matched old vertices -> new (meta remapped via oldToNew).
            foreach (KeyValuePair<IVertex, IVertex> match in oldToNew)
                CollectExternalReferrerRepins(match.Key, match.Value, oldToNew, byFrom,
                    oldScope, newScope, sourceScope, excluded);

            // Source-side (move only): external referrers of source vertices -> new (meta via sourceToNew).
            if (moveSource)
                foreach (KeyValuePair<IVertex, IVertex> pair in sourceToNew)
                    CollectExternalReferrerRepins(pair.Key, pair.Value, sourceToNew, byFrom,
                        sourceScope, newScope, oldScope, excluded);

            // 5. Apply repins. No disposal happens here (every RewriteFrom re-adds before deleting).
            foreach (KeyValuePair<IVertex, List<EdgeRewrite>> kv in byFrom)
                RewriteFrom(kv.Key, kv.Value);

            // 6. Delete the old connectors (old subgraph disposes). On move also delete the input edges
            //    (source disposes). Re-found by meta/to because step 5 may have rebuilt them.
            foreach (IEdge oldRootEdge in oldRootEdges)
                DeleteMatchingEdge(replaceTo, oldRootEdge.Meta, oldRootEdge.To);

            if (moveSource)
                foreach (IEdge inputEdge in roots)
                    if (inputEdge.From != null && inputEdge.From.DisposedState == DisposeStateEnum.Live)
                        DeleteMatchingEdge(inputEdge.From, inputEdge.Meta, inputEdge.To);

            MinusZero.Instance.Log(1, moveSource ? "VertexOperations.MoveAndReplaceVertex" : "VertexOperations.CopyAndReplaceVertex",
                "roots=" + roots.Count + " oldRoots=" + oldRootEdges.Count
                + " oldScope=" + oldScope.Count + " newScope=" + newScope.Count
                + " sourceScope=" + sourceScope.Count + " matched=" + oldToNew.Count);
        }

        // Queues repins of every external referrer (in-edge / meta-in-edge) of 'source' onto 'target'.
        // Referrers coming from ownScope/newScope/otherScope or that are excluded are left untouched.
        static void CollectExternalReferrerRepins(IVertex source, IVertex target, Dictionary<IVertex, IVertex> map,
            Dictionary<IVertex, List<EdgeRewrite>> byFrom, HashSet<IVertex> ownScope, HashSet<IVertex> newScope,
            HashSet<IVertex> otherScope, HashSet<IEdge> excluded)
        {
            foreach (IEdge inEdge in source.InEdgesRaw.ToList())
            {
                if (ownScope.Contains(inEdge.From) || newScope.Contains(inEdge.From)
                    || otherScope.Contains(inEdge.From) || excluded.Contains(inEdge))
                    continue;

                AddRewrite(byFrom, inEdge.From, new EdgeRewrite
                {
                    Target = inEdge,
                    NewMeta = Remap(map, inEdge.Meta),
                    NewTo = target
                });
            }

            foreach (IEdge metaInEdge in source.MetaInEdgesRaw.ToList())
            {
                if (ownScope.Contains(metaInEdge.From) || newScope.Contains(metaInEdge.From)
                    || otherScope.Contains(metaInEdge.From) || excluded.Contains(metaInEdge))
                    continue;

                AddRewrite(byFrom, metaInEdge.From, new EdgeRewrite
                {
                    Target = metaInEdge,
                    NewMeta = target,
                    NewTo = Remap(map, metaInEdge.To)
                });
            }
        }

        static void DeleteMatchingEdge(IVertex from, IVertex meta, IVertex to)
        {
            foreach (IEdge e in from.OutEdgesRaw.ToList())
                if (ReferenceEquals(e.Meta, meta) && ReferenceEquals(e.To, to))
                {
                    from.DeleteEdge(e);
                    return;
                }
        }

        class EdgeRewrite
        {
            public IEdge Target;
            public IVertex NewMeta;
            public IVertex NewTo;
        }

        class MatchCandidate
        {
            public double Score;
            public IVertex Old;
            public IVertex New;
        }

        static IVertex Remap(Dictionary<IVertex, IVertex> map, IVertex v)
        {
            if (v != null && map.ContainsKey(v))
                return map[v];

            return v;
        }

        static void AddRewrite(Dictionary<IVertex, List<EdgeRewrite>> byFrom, IVertex from, EdgeRewrite rewrite)
        {
            List<EdgeRewrite> list;

            if (!byFrom.TryGetValue(from, out list))
            {
                list = new List<EdgeRewrite>();
                byFrom[from] = list;
            }

            list.Add(rewrite);
        }

        // Rebuilds the whole out-edge list of from, applying the repins in place. This preserves the
        // original edge order (plain add+delete would move repinned edges to the end).
        static void RewriteFrom(IVertex from, List<EdgeRewrite> rewrites)
        {
            List<IEdge> snapshot = from.OutEdgesRaw.ToList();

            foreach (IEdge e in snapshot)
            {
                EdgeRewrite rewrite = null;

                foreach (EdgeRewrite candidate in rewrites)
                    if (ReferenceEquals(candidate.Target, e))
                    {
                        rewrite = candidate;
                        break;
                    }

                if (rewrite != null)
                    from.AddEdge(rewrite.NewMeta, rewrite.NewTo);
                else
                    from.AddEdge(e.Meta, e.To);
            }

            from.DeleteEdgesList(snapshot);
        }

        // Finds, for each input root edge, the best-matching existing edge under replaceTo (same meta,
        // similar To vertex). Each existing edge is used at most once (1:1 at the root level too).
        static List<IEdge> FindOldRootEdges(List<IEdge> roots, IVertex replaceTo, Dictionary<IVertex, Dictionary<IVertex, double>> memo)
        {
            List<IEdge> result = new List<IEdge>();
            HashSet<IEdge> used = new HashSet<IEdge>();

            foreach (IEdge rootEdge in roots)
            {
                IVertex sourceRoot = rootEdge.To;

                IEdge best = null;
                double bestScore = -1;

                foreach (IEdge candidate in replaceTo.OutEdgesRaw)
                {
                    if (used.Contains(candidate) || VertexOperations.IsLink(candidate))
                        continue;

                    if (!GeneralUtil.CompareStrings(candidate.Meta, rootEdge.Meta))
                        continue;

                    double score = Similarity(sourceRoot, candidate.To, 0, memo);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = candidate;
                    }
                }

                if (best != null && bestScore >= ReplaceSimilarityCutoff)
                {
                    result.Add(best);
                    used.Add(best);
                }
            }

            return result;
        }

        // Anchor-first, 1:1 matching: score all old/new pairs by (content + structure) similarity, keep
        // those at or above the cutoff, then assign greedily best-first so each old and each new vertex
        // is used at most once.
        static Dictionary<IVertex, IVertex> MatchSubGraphs(HashSet<IVertex> oldScope, HashSet<IVertex> newScope,
            Dictionary<IVertex, Dictionary<IVertex, double>> memo)
        {
            List<MatchCandidate> candidates = new List<MatchCandidate>();

            foreach (IVertex oldVertex in oldScope)
                foreach (IVertex newVertex in newScope)
                {
                    double score = Similarity(oldVertex, newVertex, 0, memo);

                    if (score >= ReplaceSimilarityCutoff)
                        candidates.Add(new MatchCandidate { Score = score, Old = oldVertex, New = newVertex });
                }

            candidates.Sort((x, y) =>
            {
                int c = y.Score.CompareTo(x.Score);
                if (c != 0) return c;

                c = string.CompareOrdinal(GraphUtil.GetStringValue(x.Old), GraphUtil.GetStringValue(y.Old));
                if (c != 0) return c;

                return string.CompareOrdinal(GraphUtil.GetStringValue(x.New), GraphUtil.GetStringValue(y.New));
            });

            HashSet<IVertex> usedOld = new HashSet<IVertex>();
            HashSet<IVertex> usedNew = new HashSet<IVertex>();
            Dictionary<IVertex, IVertex> map = new Dictionary<IVertex, IVertex>();

            foreach (MatchCandidate candidate in candidates)
            {
                if (usedOld.Contains(candidate.Old) || usedNew.Contains(candidate.New))
                    continue;

                map[candidate.Old] = candidate.New;
                usedOld.Add(candidate.Old);
                usedNew.Add(candidate.New);
            }

            return map;
        }

        // Recursive content+structure similarity in [0..1], memoized. A provisional value is stored
        // before recursing so cyclic structures terminate.
        static double Similarity(IVertex a, IVertex b, int depth, Dictionary<IVertex, Dictionary<IVertex, double>> memo)
        {
            Dictionary<IVertex, double> inner;

            if (!memo.TryGetValue(a, out inner))
            {
                inner = new Dictionary<IVertex, double>();
                memo[a] = inner;
            }

            double cached;
            if (inner.TryGetValue(b, out cached))
                return cached;

            double local = LocalSimilarity(a, b);

            inner[b] = local; // provisional, breaks cycles

            double result = local;

            if (depth < Replace_MaxMatchDepth)
            {
                double childAlignment = ChildAlignment(a, b, depth, memo);

                if (childAlignment >= 0)
                    result = Replace_StructureWeight * childAlignment + (1 - Replace_StructureWeight) * local;
            }

            inner[b] = result;

            return result;
        }

        // Greedy best alignment of the copyable, non-link children of a and b, normalized by the larger
        // child count (so missing/extra children reduce the score). Returns -1 when neither has children.
        static double ChildAlignment(IVertex a, IVertex b, int depth, Dictionary<IVertex, Dictionary<IVertex, double>> memo)
        {
            List<IEdge> childrenA = CopyableChildEdges(a);
            List<IEdge> childrenB = CopyableChildEdges(b);

            if (childrenA.Count == 0 && childrenB.Count == 0)
                return -1;

            if (childrenA.Count == 0 || childrenB.Count == 0)
                return 0;

            bool[] usedB = new bool[childrenB.Count];
            double total = 0;

            foreach (IEdge edgeA in childrenA)
            {
                double best = 0;
                int bestIndex = -1;

                for (int i = 0; i < childrenB.Count; i++)
                {
                    if (usedB[i])
                        continue;

                    double score = Similarity(edgeA.To, childrenB[i].To, depth + 1, memo);

                    if (!GeneralUtil.CompareStrings(edgeA.Meta, childrenB[i].Meta))
                        score *= Replace_MetaMismatchPenalty;

                    if (score > best)
                    {
                        best = score;
                        bestIndex = i;
                    }
                }

                if (bestIndex >= 0)
                {
                    usedB[bestIndex] = true;
                    total += best;
                }
            }

            return total / Math.Max(childrenA.Count, childrenB.Count);
        }

        static double LocalSimilarity(IVertex a, IVertex b)
        {
            double valueSimilarity = ValueSimilarity(GraphUtil.GetStringValue(a), GraphUtil.GetStringValue(b));

            List<string> isA = IsValues(a);
            List<string> isB = IsValues(b);

            if (isA.Count == 0 && isB.Count == 0)
                return valueSimilarity;

            double isSimilarity = DiceOverlap(isA, isB);

            return (Replace_ValueWeight * valueSimilarity + Replace_IsWeight * isSimilarity)
                / (Replace_ValueWeight + Replace_IsWeight);
        }

        static List<IEdge> CopyableChildEdges(IVertex v)
        {
            List<IEdge> result = new List<IEdge>();

            foreach (IEdge e in v.OutEdgesRaw)
                if (CanCopy_ByEdge(e) && !VertexOperations.IsLink(e))
                    result.Add(e);

            return result;
        }

        static List<string> IsValues(IVertex v)
        {
            List<string> result = new List<string>();

            foreach (IEdge e in GraphUtil.GetQueryOut(v, "$Is", null))
                result.Add(GraphUtil.GetStringValue(e.To));

            return result;
        }

        static double DiceOverlap(List<string> a, List<string> b)
        {
            if (a.Count == 0 && b.Count == 0)
                return 1.0;

            if (a.Count == 0 || b.Count == 0)
                return 0.0;

            HashSet<string> setA = new HashSet<string>(a);
            HashSet<string> setB = new HashSet<string>(b);

            int intersection = 0;
            foreach (string s in setA)
                if (setB.Contains(s))
                    intersection++;

            return (2.0 * intersection) / (setA.Count + setB.Count);
        }

        static double ValueSimilarity(string a, string b)
        {
            if (a == b)
                return 1.0;

            int max = Math.Max(a.Length, b.Length);

            if (max == 0)
                return 1.0;

            return 1.0 - (double)LevenshteinDistance(a, b) / max;
        }

        static int LevenshteinDistance(string a, string b)
        {
            int[] previous = new int[b.Length + 1];
            int[] current = new int[b.Length + 1];

            for (int j = 0; j <= b.Length; j++)
                previous[j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                current[0] = i;

                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;

                    current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1), previous[j - 1] + cost);
                }

                int[] swap = previous;
                previous = current;
                current = swap;
            }

            return previous[b.Length];
        }

        // Builds a copy of the non-link subgraph(s) reachable from each root's To vertex into copyTo,
        // filling oldToNew with the source-vertex -> copy-vertex mapping. Meta and To are remapped to
        // the copy whenever they belong to the copied scope, so a meta vertex that is part of the
        // copied set is referenced by its copy in the destination, not by the original meta vertex.
        static void CopySubGraphIntoVertex(List<IEdge> roots, IVertex copyTo, Dictionary<IVertex, IVertex> oldToNew)
        {
            // Copy scope: union of subgraphs reachable from each root's To vertex through non-link
            // OutEdgesRaw. Traversal stops at links (VertexOperations.IsLink), so linked targets stay
            // as references to the original vertices instead of being copied.
            HashSet<IVertex> scope = new HashSet<IVertex>();

            foreach (IEdge edgeToCopy in roots)
                CollectCopyScope(edgeToCopy.To, scope);

            // Create one copy per scope vertex. New vertices must live in copyTo's store, so they
            // are created via copyTo and kept alive by a temporary holder edge until the real edges
            // are wired. The holder churn is hidden from graph change watchers.
            List<IEdge> tempEdges = new List<IEdge>();

            bool previousWatch = TurnGraphChangeWatchOff();
            try
            {
                foreach (IVertex source in scope)
                {
                    IEdge holder = copyTo.AddVertexAndReturnEdge(null, source.Value);

                    oldToNew[source] = holder.To;
                    tempEdges.Add(holder);
                }
            }
            finally
            {
                RestoreGraphChangeWatch(previousWatch);
            }

            // Recreate inner edges with meta/to remapping.
            foreach (IVertex source in scope)
            {
                IVertex targetFrom = oldToNew[source];

                foreach (IEdge e in source.OutEdgesRaw)
                {
                    if (!CanCopy_ByEdge(e))
                        continue;

                    IVertex meta = oldToNew.ContainsKey(e.Meta) ? oldToNew[e.Meta] : e.Meta;
                    IVertex to = oldToNew.ContainsKey(e.To) ? oldToNew[e.To] : e.To;

                    targetFrom.AddEdge(meta, to);
                }
            }

            // Attach each root copy under copyTo using the root edge's meta (remapped if in scope).
            foreach (IEdge edgeToCopy in roots)
            {
                IVertex rootMeta = oldToNew.ContainsKey(edgeToCopy.Meta) ? oldToNew[edgeToCopy.Meta] : edgeToCopy.Meta;

                copyTo.AddEdge(rootMeta, oldToNew[edgeToCopy.To]);
            }

            // Drop the temporary holder edges. Every copy now has real incoming edges.
            previousWatch = TurnGraphChangeWatchOff();
            try
            {
                foreach (IEdge holder in tempEdges)
                    holder.From.DeleteEdge(holder);
            }
            finally
            {
                RestoreGraphChangeWatch(previousWatch);
            }
        }

        static void CollectCopyScope(IVertex vertex, HashSet<IVertex> visited)
        {
            if (!visited.Add(vertex))
                return;

            foreach (IEdge e in vertex.OutEdgesRaw)
                if (CanCopy_ByEdge(e) && !VertexOperations.IsLink(e))
                    CollectCopyScope(e.To, visited);
        }

        static bool TurnGraphChangeWatchOff()
        {
            ITransaction transaction = MinusZero.Instance.GetTopTransaction();

            if (transaction == null)
                return true;

            bool previousWatch = transaction.GraphChangeWatchActive;

            ExecutionFlowHelper.GraphChangeWatchOff();

            return previousWatch;
        }

        static void RestoreGraphChangeWatch(bool previousWatch)
        {
            ITransaction transaction = MinusZero.Instance.GetTopTransaction();

            if (transaction == null)
                return;

            if (previousWatch)
                ExecutionFlowHelper.GraphChangeWatchOn();
            else
                ExecutionFlowHelper.GraphChangeWatchOff();
        }

        /* if something is not working I'm leaving the old version also
        public static void CopyVertex_old(IEdge edgeToCopy, IVertex copyTo)
        {
            if (InstructionHelpers.CheckIfIsAtomType(edgeToCopy.To))
                copyTo.AddVertex(edgeToCopy.Meta, edgeToCopy.To.Value);
            else
                GraphUtil.DeepCopy(edgeToCopy, copyTo);
        }*/

        public static IVertex GetTargetFromStackTop(INoInEdgeInOutVertexVertex stack)
        {
            foreach (IEdge e in stack.OutEdgesRaw)
                if (GeneralUtil.CompareStrings(e.Meta, "Target"))
                    return e.To;

            return null;
        }

        public static bool IsSpecialVertex(IVertex v)
        {
            if (v.Value == null)
                return false;

            if (v.Value.ToString().StartsWith("$"))
            {
                if (GraphUtil.GetStringValue(v) == "$Empty")
                    return false;

                return true;
            }

            return false;
        }

        public static bool CanCopy_ByEdge(IEdge e)
        {
            foreach (string s in NoCopy_MetaValue)
                if (e.Meta.Value.ToString() == s)
                    return false;

            return true;
        }

        public static bool CanCopy_ByMeta(IVertex v)
        {
            foreach (string s in NoCopy_MetaValue)
                if (v.Value.ToString() == s)
                    return false;

            return true;
        }

        public static bool CanCopy_ByMetaString(string str)
        {
            foreach (string s in NoCopy_MetaValue)
                if (str == s)
                    return false;

            return true;
        }


        public static bool CanCopy_ByVertex(IVertex v)
        {
            foreach (string s in NoCopy_VertexIsValue)
                if (GraphUtil.ExistQueryOut(v, "$Is", s))
                    return false;

            return true;
        }

        public static bool IsMetaVertexOfManyMultiplicity(IVertex meta)
        {
            int? maxCardinality = GraphUtil.GetIntegerValue(GraphUtil.GetQueryOutFirst(meta, "$MaxCardinality", null));

            if (maxCardinality == null) {
                string metaValue = GraphUtil.GetStringValue(meta);

                if (GraphUtil.ExistQueryOut(meta, "$Is", "Association") || GraphUtil.ExistQueryOut(meta, "$Is", "Aggregation"))
                    return true;                

                int? minCardinality = GraphUtil.GetIntegerValue(GraphUtil.GetQueryOutFirst(meta, "$MinCardinality", null));

                if (minCardinality == null)
                    return false;

                if (minCardinality == -1)
                    return true;

                if (minCardinality > 1)
                    return true;

                return false;
            }
            
            if (maxCardinality == 1)
                return false;

            return true;
        }

        public static bool IsMetaVertexNullable(IVertex meta)
        {
            int? minCardinality = GraphUtil.GetIntegerValue(GraphUtil.GetQueryOutFirst(meta, "$MinCardinality", null));

            if (minCardinality == null)
                return true;

            if (minCardinality == 0)
                return true;

            return false;
        }

        public static bool IsLink_OldVersion(IVertex e_Meta) // not handling IsAssiciation. and now: yes IsLink_OldVersion and IsLink needs to be aligned
                                                             // BUT: to align them we migh need to have conistent "link" theory and for now it seems that there are holes in in (at least need two different versions)
        {
            if (e_Meta.Value.ToString() == "FormalTextLanguageProcessing")
            {
                int x = 0;
            }

            if (GeneralUtil.CompareStrings(e_Meta.Value, "$EdgeTarget"))
                return true;            

            if (GraphUtil.ExistQueryOut(e_Meta, "$EdgeTarget", null) && !GraphUtil.ExistQueryOut(e_Meta, "$IsAggregation", null))
                return true;

            if (GraphUtil.GetQueryOutFirst(e_Meta, "$IsLink", null) != null)
                return true;

            return false;
        }

        public static bool IsLink(IVertex e_Meta)
        {
            if (e_Meta.Value.ToString() == "FormalTextLanguageProcessing")
            {
                int x = 0;
            }

            if (GeneralUtil.CompareStrings(e_Meta.Value, "$EdgeTarget"))
                return true;

            if (GeneralUtil.CompareStrings(e_Meta.Value, "Association")) // THIS IS A HACK. as 
            // if (GraphUtil.ExistQueryOut(e_Meta, "$VertexTarget", null) && !GraphUtil.ExistQueryOut(e_Meta, "$IsAggregation", null)) return true;
            // causes problems in graph2vertex at least
                return true;

            if (GraphUtil.ExistQueryOut(e_Meta, "$EdgeTarget", null) && !GraphUtil.ExistQueryOut(e_Meta, "$IsAggregation", null))
                return true;            

            if (GraphUtil.GetQueryOutFirst(e_Meta, "$IsLink", null) != null)
                return true;

            return false;
        }

        public static bool IsLink(IEdge e)
        {
            return IsLink(e.Meta);
        }

        public static bool IsLink_OldVersion(IEdge e)
        {
            return IsLink_OldVersion(e.Meta);
        }

        public static bool IsMetaAndToVertexEnoughToIdentifyEdge(IVertex baseVertex, IVertex meta, IVertex to)
        {
            if (GraphUtil.GetQueryOutCount(baseVertex, meta.Value, to.Value) > 1)
                return false;
            else
                return true;
        }

        public static bool IsToVertexEnoughToIdentifyEdge(IVertex baseVertex, IVertex to)
        {
            if (GeneralUtil.CompareStrings(to.Value, ""))
            {
                if (baseVertex.OutEdges.Count() == 1)
                    return true;
                else
                    return false;
            }

            if (GraphUtil.GetQueryOutCount(baseVertex, null, to.Value) > 1)
                return false;
            else
                return true;
        }

        public static bool IsInherited(IVertex baseVertex, string isInheritedFrom_String)
        {
            foreach (IEdge e in GraphUtil.GetQueryOut(baseVertex, "$Inherits",null))
                if (_IsInherited(e.To, isInheritedFrom_String))
                    return true;

            return false;
        }

        private static bool _IsInherited(IVertex baseVertex, string isInheritedFrom_String)
        {
            if (GraphUtil.ExistQueryOut(baseVertex, isInheritedFrom_String, null))
                return true;

            foreach (IEdge e in GraphUtil.GetQueryOut(baseVertex, "$Inherits", null))
                if (_IsInherited(e.To, isInheritedFrom_String))
                    return true;

            return false;
        }

        public static void DeleteOneEdge(IVertex source, IVertex metaVertex, IVertex toVertex)
        {
            if (source == null || toVertex == null)
                return;

            GraphUtil.DeleteEdge(source, metaVertex, toVertex);
        }        

        public static bool IsViewVertex(IVertex vertex)
        {
            if (GraphUtil.ExistQueryOut(vertex, "$GraphChangeTrigger", "CreateView"))
                return true;

            return false;
        }

        public static bool DoOutEdgesDictionaryValueContainViewVertex(object value)
        {
            if (value is IEdge)
                return IsViewVertex(((IEdge)value).Meta);

            List_VertexBase edges = value as List_VertexBase;

            foreach (IEdge e in edges)
                if (IsViewVertex(e.Meta))
                    return true;

            return false;
        }

        public static bool IsAtomicVertex(IVertex vertex) // vertex can always have multiple $GraphChangeTrigger
        {
            if (vertex.OutEdges.Count() == 0)
                return true;

            int cnt = 0;

            foreach (string s in NoCopy_MetaValue)
                cnt += GraphUtil.GetQueryOutCount(vertex, s, null);

            if (vertex.OutEdges.Count() == cnt)
                return true;

            return false;
        }

        public static bool IsAtomicEdge(IEdge edge) // vertex can always have multiple $GraphChangeTrigger
        {
            if (IsInherited(edge.Meta, "AtomType"))
                return true;

            return IsAtomicVertex(edge.To);
        }

        public static bool IsAtomicType(IVertex metaVertex)
        {
            if (IsInherited(metaVertex, "AtomType"))
                return true;
            return false;
        }

        public static IVertex GetChildEdges(IVertex metaVertex)
        {
            if (GraphUtil.GetQueryOutCount(metaVertex, "$Is", "Class") > 0)
                return metaVertex.GetAll(false, "{$Inherits:Selector}:");

            IVertex edgeTarget = GraphUtil.GetQueryOutFirst(metaVertex, "$EdgeTarget", null);

            if (edgeTarget != null && edgeTarget != metaVertex)
                return GetChildEdges(edgeTarget);

            IVertex ret = m0.MinusZero.Instance.CreateTempVertex();

            foreach (IEdge e in metaVertex)
            {
                if (GeneralUtil.CompareStrings(e.Meta, "$VertexTarget"))
                    ret.AddEdge(null, m0.MinusZero.Instance.EdgeTarget);
                else
                    if (e.To.Value != null && // && e.To.Value.ToString() != "" && (e.To.Value.ToString()[0] != '$') &&                    
                        
                        (GeneralUtil.CompareStrings(e.Meta, "$Empty") ||
                        (e.Meta.Value.ToString() != "" && e.Meta.Value.ToString()[0] != '$') ) )
                    
                    // is extanded                    // ???
                    // if (e.To.Get(false, "$VertexTarget:") != null || e.To.Get(false, "$EdgeTarget:") != null) // ???
                    ret.AddEdge(null, e.To);
            }

            return ret;
        }

        // as this is one of most important conceptual definitions, the historic version of the method. it does not support meta Vertices that creates edge+vertex
        /*        public static IVertex GetChildEdges(IVertex metaVertex)
                {                        
                    IVertex edgeTarget = metaVertex.Get(false, "$EdgeTarget:");
                    if (edgeTarget != null && edgeTarget!=metaVertex)
                        return GetChildEdges(edgeTarget);

                    IVertex ret = m0.MinusZero.Instance.CreateTempVertex();

                    foreach (IEdge e in metaVertex)
                    {
                        if(GeneralUtil.CompareStrings(e.Meta,"$VertexTarget"))
                            ret.AddEdge(null,m0.MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"));
                        else
                            //if (!GeneralUtil.CompareStrings(e.Meta, "$Is") && !GeneralUtil.CompareStrings(e.Meta, "$Inherits")) // to be extanded
                            if (GeneralUtil.CompareStrings(e.Meta, "$Empty")||((string)e.Meta.Value)[0] != '$') // is extanded                    
                                if (e.To.Get(false, "$VertexTarget:") != null || e.To.Get(false, "$EdgeTarget:") != null)
                                    ret.AddEdge(null,e.To);
                    }

                    return ret;
                }*/

        public static IVertex DoFilter(IVertex baseVertex, IVertex FilterQuery)
        {
            return baseVertex.GetAll(false, (string)FilterQuery.Value);
        }

        public static bool InheritanceCompare(IVertex baseVertex, string toCompare)
        {
            if (GeneralUtil.CompareStrings(baseVertex.Value, toCompare))
                return true;

            foreach (IEdge e in GraphUtil.GetQueryOut(baseVertex, "$Inherits", null))
                if (InheritanceCompare(e.To, toCompare))
                    return true;

            return false;
        }

        public static IVertex TestIfNewEdgeValid(IVertex baseVertex, IVertex metaVertex, IVertex toVertex)
        {
            int? MaxCardinality = GraphUtil.GetIntegerValue(GraphUtil.GetQueryOutFirst(metaVertex,"$MaxCardinality",null));

            if (MaxCardinality != -1 && MaxCardinality != null)
            {
                int cnt = 0;

                foreach (IEdge e in baseVertex)
                    if (e.Meta == metaVertex)
                        cnt++;

                if ((cnt + 1) > MaxCardinality)
                {
                    IVertex v = MinusZero.Instance.CreateTempVertex();

                    v.Value = "Source vertex allready contains $MaxCardinality count of edges of desired meta.";

                    return v;
                }
            }

            int? MaxTargetCardinality = GraphUtil.GetIntegerValue(GraphUtil.GetQueryOutFirst(metaVertex, "$MaxTargetCardinality", null));

            if (MaxTargetCardinality != -1 && MaxTargetCardinality != null && toVertex!=null)
            {
                int cnt = 0;

                foreach (IEdge e in toVertex.InEdges)
                    if (e.Meta == metaVertex)
                        cnt++;

                if ((cnt + 1) > MaxCardinality)
                {
                    IVertex v = MinusZero.Instance.CreateTempVertex();

                    v.Value = "Target vertex allready contains $MaxTargetCardinality count of in edges of desired meta.";

                    return v;
                }
            }

            return null;
        }

        public static IEdge AddEdgeOrVertexByMeta(IVertex baseVertex, IVertex metaVertex, IVertex toVertex, bool CreateEdgeOnly, bool ForceShowEditForm)
        {
            if (GraphUtil.ExistQueryOut(metaVertex,"$VertexTarget", null)
                && !CreateEdgeOnly
                )
            {                
                IVertex n = VertexOperations.AddInstance(baseVertex, metaVertex);

                IEdge e = new EasyEdge(baseVertex, metaVertex, n);

                n.AddEdge(MinusZero.Instance.EdgeTarget, toVertex);

                if (ForceShowEditForm == true)
                    MinusZero.Instance.UserInteraction.EditEdge(e.To);

                return e;
            }
            else
            {
                return baseVertex.AddEdge(metaVertex, toVertex); ;
            }
        }

        public static IVertex AddInstance(IVertex baseVertex, IVertex metaVertex, IVertex edgeVertex)
        {
            return AddInstanceAndReturnEdge(baseVertex, metaVertex, edgeVertex).To;
        }

        public static IEdge AddInstanceAndReturnEdge(IVertex baseVertex,IVertex metaVertex, IVertex edgeVertex)
        {
            IEdge ne = null;
            IVertex nv;

            if (baseVertex != null)
            {
                if (GraphUtil.ExistQueryOut(metaVertex, "$EmptyMetaInstance", null))
                    ne = baseVertex.AddVertexAndReturnEdge(null, null);
                else
                    ne = baseVertex.AddVertexAndReturnEdge(edgeVertex, null);
            }
            else
                ne = MinusZero.Instance.CreateTempEdge();

            if (ne == null)
                return null;

            nv = ne.To;

            if (MinusZero.Instance.Root.Store.DetachState == DetachStateEnum.Attached && !GeneralUtil.CompareStrings(metaVertex.Value, "$Empty")) // XXX WTF?????
                nv.AddEdge(MinusZero.Instance.Is, metaVertex);

            ///

            if (GraphUtil.ExistQueryOut(metaVertex,"$IsAggregation",null))
                nv.AddEdge(MinusZero.Instance.IsAggregation, MinusZero.Instance.Empty); // 2026.04.09 wtf

            ///

            IVertex children = metaVertex; // can use VertexOperations.GetChildEdges, but $DefaultValue: should be OK

            foreach (IEdge child in children)
            {                
                bool canAdd = false;

                IVertex childMetaVertex = child.Meta;

                IVertex MinCardinality = GraphUtil.GetQueryOutFirst(child.To, "$MinCardinality", null);

                if (InstructionHelpers.CheckIfInherits_WRONG(childMetaVertex, "Selector"))
                {
                    if (MinCardinality != null)
                    {
                        if (GraphUtil.GetIntegerValueOr0(MinCardinality) == 1)
                            canAdd = true;
                        else
                            canAdd = false;
                    }
                    else
                        //canAdd = false; // it was true. 2025.04.17 we do not want new edges when mincardinality is not specified
                        canAdd = true; // aparently we need that as a lot of code depends on that :/
                }
                else
                {                    
                    if (MinCardinality != null && GraphUtil.GetIntegerValueOr0(MinCardinality) == 1)
                        canAdd = true;
                }

                if (canAdd)
                    if (GraphUtil.ExistQueryOut(child.To, "$DefaultValue", null))
                    {   
                        if (IsLink(child.Meta)) // if link, then we do not want to add default value, but just edge
                            nv.AddEdge(child.To, GraphUtil.GetQueryOutFirst(child.To, "$DefaultValue", null));
                        else
                            nv.AddVertex(child.To, GraphUtil.GetQueryOutFirst(child.To, "$DefaultValue", null).Value);
                    }
                    else
                        nv.AddVertex(child.To, null); // ? XXX
            }

            return ne;
        }

        public static IVertex AddInstance(IVertex baseVertex, IVertex metaVertex)
        {
            return AddInstance(baseVertex, metaVertex, metaVertex);
        }

        public static IEdge AddInstanceAndReturnEdge(IVertex baseVertex, IVertex metaVertex)
        {
            return AddInstanceAndReturnEdge(baseVertex, metaVertex, metaVertex);
        }

        public static IVertex AddInstanceByEdgeVertex(IVertex baseVertex, IVertex edgeVertex) // by EdgeTarget or VertexTarget or by iself
        {
            // $EdgeTarget
            IVertex edgeVertexEdgeTarget = GraphUtil.GetQueryOutFirst(edgeVertex,"$EdgeTarget",null);

            if (edgeVertexEdgeTarget != null)
                return AddInstance(baseVertex, edgeVertexEdgeTarget, edgeVertex);

            // $VertexTarget
            IVertex edgeVertexVertexTarget = GraphUtil.GetQueryOutFirst(edgeVertex, "$VertexTarget", null);

            if (edgeVertexVertexTarget != null)
            {
                IVertex ret = AddInstance(baseVertex, edgeVertex, edgeVertex);

                return ret;
            }

            // EMPTY (edge+vertex one)
            AddInstance(baseVertex, edgeVertex, edgeVertex);

            return null;
        }
    }
}
