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

        public static string GetTypeName(object obj)
        {
            return obj.GetType().AssemblyQualifiedName;
        }

        public static bool SetPropertyIfPresent(object o, string propertyname, object value)
        {
            PropertyInfo p = o.GetType().GetProperties().Where(x => x.Name == propertyname).FirstOrDefault();

            if (p != null)
            {
                p.SetValue(o, value, null);
                return true;
            }

            return false;
        }

        public static IList<T> CreateAndCopyList<T>(IEnumerable<T> source){
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
            if (o == null||o2==null)
                return false;
            else
                return o.ToString() == o2.ToString();
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

        public static IVertex ParseAndExcute(IVertex baseVertex, IVertex inputVertex, string expressionAsString)
        {
            MinusZero z = MinusZero.Instance;

            IVertex expressionAsVertex = MinusZero.Instance.CreateTempVertex();

            z.DefaultParser.Parse(expressionAsVertex, expressionAsString);

            return z.DefaultExecuter.Execute(baseVertex, inputVertex, expressionAsVertex);
        }
    }
}
