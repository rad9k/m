using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Util
{
    public class zstring
    {
        public string internalString;

        bool hashGenerted = false;
        int hash;

        public zstring(string s)
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
            return internalString;
        }

        public zstring Substring(int start, int length)
        {
            return new zstring(internalString.Substring(start, length));
        }

        public zstring Substring(int start)
        {
            return new zstring(internalString.Substring(start));
        }

        public int this[int key]
        {
            get => internalString[key];
        }

        public int Length
        {
            get => internalString.Length;
        }

        public static bool operator ==(zstring obj1, zstring obj2)
        {
            return obj1.internalString == obj2.internalString;
        }

        public static bool operator !=(zstring obj1, zstring obj2)
        {
            return obj1.internalString != obj2.internalString;
        }

        public bool Equals(zstring other)
        {
            return internalString == other.internalString;
        }

        public static bool operator ==(zstring obj1, String obj2)
        {
            return obj1.internalString == obj2;
        }

        public static bool operator !=(zstring obj1, String obj2)
        {
            return obj1.internalString != obj2;
        }

        public bool Equals(string other)
        {
            return internalString == other;
        }

        public override bool Equals(object other)
        {
            return internalString == other.ToString();
        }
    }
}
