using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;
using m0.Graph;
using System.Reflection;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using m0.Store.Json;
using System.IO;
using System.Runtime.CompilerServices;

namespace m0.Util
{
    public class GeneralUtil
    {
        [DllImport("ole32.dll", SetLastError = true)]

        private static extern int CoCreateGuid(ref Guid pguid);

        public static Guid NewGuid()
        {
            /* Guid val = Guid.Empty;

             int hresult = 0;

             hresult = CoCreateGuid(ref val);

             if (hresult != 0)            
                 throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Error creating new Guid");            

             return val;*/

            return Guid.NewGuid();
        }

        public static int GetHashCode(object o)
        {
            if (o == null)
                return 0;

            //return o.GetHashCode();
            return RuntimeHelpers.GetHashCode(o);
        }

        public static int Double2Int(double d)
        {
            double floor = Math.Floor(d);

            double rest = d - floor;

            if (rest > 0.5)
                return (int)(floor + 1);
            else
                return (int)floor;
        }

        public static void SaveStore(IStore s)
        {
            s.Detach();
            s.CommitTransaction();
            s.Attach();
        }

        public static IVertex CreateM0JAndMoveEdgesIntoIt(string fileName, IVertex baseVertex, int vertexIdentifierCompensate)
        {
            File.Delete(fileName);

            JsonSerializationStore s = new JsonSerializationStore(fileName, MinusZero.Instance, new AccessLevelEnum[] { });

            s.VertexIdentifierCompensate(vertexIdentifierCompensate);

            ZeroUML.Instructions.ZeroUMLInstructionHelpers.MoveEdgesIntoVertex(baseVertex, s.Root);            

            s.Detach();
            s.CommitTransaction();
            s.Attach();

            return s.Root;
        }

        public static void CreateM0JAndMoveEdgesIntoIt_IncludeEverythingBesidesList(string fileName, IVertex baseVertex, HashSet<IVertex> vertexToLeave, Dictionary<string, StoreId> storeOverride)
        {
            File.Delete(fileName);

            JsonSerializationStore s = new JsonSerializationStore(fileName, MinusZero.Instance, new AccessLevelEnum[] { });

            ZeroUML.Instructions.ZeroUMLInstructionHelpers.MoveEdgesIntoVertex_IncludeEverythingBesidesList(baseVertex, s.Root, vertexToLeave);

            s.SetStoreOverride(storeOverride);

            s.Detach();
            s.CommitTransaction();
            s.Attach();
        }

        public static void DictionaryAdd<key,value>(Dictionary<key, List<value>> dict, key _key, value _value)
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

        public static string GetRegexpEXTRACT(string s, string r)
        {
            Regex rgx = new Regex(r);

            foreach (Match match in rgx.Matches(s))
            {
                return match.Groups["EXTRACT"].Value;
            }

            return null;
        }

        public static string GetTypeName(object obj)
        {
            return obj.GetType().AssemblyQualifiedName;
        }

        public static bool SetPropertyIfPresent(object o, string propertyname, object value)
        {
            if (o == null)
                return false;

            PropertyInfo p = o.GetType().GetProperties().Where(x => x.Name == propertyname).FirstOrDefault();

            if (p != null)
            {
                p.SetValue(o, value, null);
                return true;
            }

            return false;
        }

        public static List<T> GetList<T>(IEnumerable<T> source)
        {
            if (source is List<T>)
                return (List<T>)source;

            return source.ToList<T>();
        }

        public static IList<T> CreateAndCopyList<T>(IEnumerable<T> source) {
            IList<T> list=new List<T>();

            foreach (T o in source)
            {
                list.Add(o);
            }

            return list;
        }

        public static IList CreateAndCopyList(System.Collections.IEnumerable source)
        {
            IList list = new List<object>();

            foreach (object o in source)
            {
                list.Add(o);
            }

            return list;
        }        

        public static bool CompareStrings(object o, string s)
        {
            if (o == null)
                return false;
            else
                return o.ToString()==s;
        }

        public static bool CompareStrings(object o, object o2)
        {
            if (o == null || o2 == null)
                return false;
            else
                return o.ToString() == o2.ToString();
        }

        public static bool CompareStrings(object o, string[] o2)
        {
            if (o != null && o2 != null) {
                string os = o.ToString();

                foreach (string s in o2)
                    if (os == s.ToString())
                        return true;
            } 
             
            return false;
        }

        public static IVertex GetVertexByStoreIdAndId(string storeIdentifier, object identifier)
        {
            foreach (IStore s in MinusZero.Instance.Stores)
                if (s.Identifier == storeIdentifier)
                    return s.GetVertexByIdentifier(identifier);

            return null;
        }

        public static bool DoDelegateListContainDelegate(Delegate[] delegates, Delegate _delegate)
        {
            if(delegates !=null)
            foreach(Delegate d in delegates)
                if(d==_delegate)
                    return true;

            return false;
        }

        public static String EmptyIfNull(object o)
        {
            if (o == null)
                return "";
            else
                return o.ToString();
        }
    }
}
