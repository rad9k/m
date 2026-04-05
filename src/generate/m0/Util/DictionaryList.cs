using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Util
{
    public class DictionaryList<key, value>
    {
        Dictionary<key, List<value>> dict;

        public DictionaryList()
        {
            dict = new Dictionary<key, List<value>>();
        }

        public void Add(key _key, value _value)
        {
            if (dict.ContainsKey(_key))
                dict[_key].Add(_value);
            else
            {
                List<value> l = new List<value>();
                l.Add(_value);
                dict.Add(_key, l);
            }
        }

        public void Remove(key _key, value _value)
        {
            if (dict.ContainsKey(_key))
                return;
            else
            {
                List<value> l = dict[_key];
                l.Remove(_value);
            }
        }

        public bool ContainsKey(key _key)
        {
            return dict.ContainsKey(_key);
        }

        public bool Contains(key _key, value _value)
        {
            if (!dict.ContainsKey(_key))
                return false;
            else
            {
                List<value> l = dict[_key];
                return l.Contains(_value);
            }
        }
    }
}
