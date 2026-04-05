using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Util
{
    class ExtandableDictionaryListAsList<T> : IList<T>
    {
        bool needToRegenerate = true;


        private List<T> l = new List<T>();

        public virtual void OnAdd(T item) { }

        public virtual void OnRemove(T item) { }

        public int IndexOf(T item)
        {
            return ((IList<T>)l).IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ((IList<T>)l).Insert(index, item);

            OnAdd(item);
        }

        public void RemoveAt(int index)
        {
            T removedItem = l[index];

            ((IList<T>)l).RemoveAt(index);

            OnRemove(removedItem);
        }

        public void Add(T item)
        {
            ((IList<T>)l).Add(item);

            OnAdd(item);
        }

        public void Clear()
        {
            ((IList<T>)l).Clear();
        }

        public bool Contains(T item)
        {
            return ((IList<T>)l).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((IList<T>)l).CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            bool ret = ((IList<T>)l).Remove(item);

            OnRemove(item);

            return ret;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IList<T>)l).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<T>)l).GetEnumerator();
        }

        public int Count => ((IList<T>)l).Count;

        public bool IsReadOnly => ((IList<T>)l).IsReadOnly;

        public T this[int index] { get => ((IList<T>)l)[index]; set => ((IList<T>)l)[index] = value; }
    }
}
