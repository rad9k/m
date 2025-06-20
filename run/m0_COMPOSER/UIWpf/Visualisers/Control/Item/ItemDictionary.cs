using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.UIWpf.Visualisers.Control.Item
{
    public class ItemDictionary
    {
        static IDictionary<IVertex, IList<IItem>> dict = new Dictionary<IVertex, IList<IItem>>();

        public static void Add(IItem item)
        {
            IVertex eventVertex = item.BaseEdge.To;

            if (dict.ContainsKey(eventVertex))
                dict[eventVertex].Add(item);
            else
            {
                IList<IItem> list = new List<IItem>();
                list.Add(item);
                dict.Add(eventVertex, list);
            }
        }

        public static IList<IItem> Get(IVertex eventVertex)
        {
            if (dict.ContainsKey(eventVertex))
                return dict[eventVertex];
            else
                return null;
        }

        public static void Remove(IItem item)
        {
            IVertex eventVertex = item.BaseEdge.To;

            if (dict.ContainsKey(eventVertex))
                dict[eventVertex].Remove(item);            
        }

        public static void RemoveAllByHost(IZoomScrollViewerHost host)
        {
            foreach (KeyValuePair<IVertex, IList<IItem>> kvp in dict)
                foreach (IItem i in kvp.Value.ToList())
                    if (i.Host == host)
                        kvp.Value.Remove(i);            
        }

    }
}
