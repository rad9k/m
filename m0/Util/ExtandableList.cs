using m0.Foundation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Util
{
    [Serializable]
    public class ExtandableList<T> : IList<T>
    {
        public virtual T Get(T toCheckEdge) { return default(T); }

        private List<T> list = new List<T>();

        public virtual void OnAdd(T item) { }

        public virtual void OnRemove(T item) { }

        public int IndexOf(T item)
        {
            return ((IList<T>)list).IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ((IList<T>)list).Insert(index, item);

            OnAdd(item);
        }

        public void RemoveAt(int index)
        {
            T removedItem = list[index];

            ((IList<T>)list).RemoveAt(index);

            OnRemove(removedItem);
        }

        public void Add(T item)
        {
            ((IList<T>)list).Add(item);

            OnAdd(item);
        }

        public void Clear()
        {
            ((IList<T>)list).Clear();
        }

        public bool Contains(T item)
        {
            return ((IList<T>)list).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((IList<T>)list).CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            bool ret = ((IList<T>)list).Remove(item);

            if (!ret)
            {
                item = Get(item);

                if (item != null)
                {
                    ret = ((IList<T>)list).Remove(item);
                    OnRemove(item);
                }
            }else
                OnRemove(item);
            
            return ret;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IList<T>)list).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<T>)list).GetEnumerator();
        }

        public int Count => ((IList<T>)list).Count;

        public bool IsReadOnly => ((IList<T>)list).IsReadOnly;

        public T this[int index] { get => ((IList<T>)list)[index]; set => ((IList<T>)list)[index] = value; }
    }
}
