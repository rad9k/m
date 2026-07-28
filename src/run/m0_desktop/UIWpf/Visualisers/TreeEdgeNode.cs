using System.Collections.ObjectModel;
using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;

namespace m0.UIWpf.Visualisers
{
    public class TreeEdgeNode
    {
        public TreeEdgeNode(
            IEdge edge,
            TreeEdgeNode parent,
            bool hasChildren,
            bool doNotTrackGraphChanges)
        {
            Edge = edge;
            Parent = parent;
            HasChildren = hasChildren;
            DoNotTrackGraphChanges = doNotTrackGraphChanges;
            Children = new ObservableCollection<TreeEdgeNode>();
        }

        public IEdge Edge { get; private set; }

        public TreeEdgeNode Parent { get; private set; }

        public bool HasChildren { get; private set; }

        public bool DoNotTrackGraphChanges { get; private set; }

        public bool ChildrenLoaded { get; set; }

        public ObservableCollection<TreeEdgeNode> Children { get; private set; }

        public void UpdateHasChildren(bool hasChildren)
        {
            HasChildren = hasChildren;
        }
    }
}
