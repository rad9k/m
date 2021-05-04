using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Util
{
    public class ExtandableHashSetAsList<T> : IList<T>
    {        
        private HashSet<T> hs = new HashSet<T>();

        public virtual void OnAdd(T item) { }

        public virtual void OnRemove(T item) { }

        public int IndexOf(T item)
        {
            return hs.ToList().IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            Add(item);            
        }

        public void RemoveAt(int index)
        {
            throw new Exception("RemoveAt not supported in ExtandableHashSetAsList");
        }

        public void Add(T item)
        {
            if (!hs.Contains(item))
            {
                hs.Add(item);

                OnAdd(item);
            }
        }

        public void Clear()
        {
            hs.Clear();            
        }

        public bool Contains(T item)
        {
            return hs.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            hs.CopyTo(array);            
        }

        public bool Remove(T item)
        {
            bool ret = hs.Remove(item);
            
            OnRemove(item);

            return ret;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return hs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return hs.GetEnumerator();
        }

        public int Count => hs.Count;

        public bool IsReadOnly => false;

        public T this[int index] {
            get => hs.ToList()[index];
            set => hs.ToList()[index] = value;
        }
    }
}
