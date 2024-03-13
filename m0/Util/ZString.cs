using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Util
{
    public class ZString
    {
        public string internalString;

        bool hashGenerted = false;
        int hash;

        public ZString(string s)
        {
            internalString = s;
        }

        public override int GetHashCode()
        {
            if (!hashGenerted)
            {
                hash = internalString.GetHashCode();

                hashGenerted = true;
            }

            return hash;
        }

        public override string ToString()
        {
            return internalString.ToString();
        }

        public int this[int key]
        {
            get => internalString[key];
        }
    }
}
