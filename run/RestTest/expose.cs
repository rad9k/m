using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace RestTest
{
    public class expose
    {
        public static int Add(int a, int b)
        {
            return a + b;
        }

        public static string Concat(string str1, string str2)
        {
            return str1 + str2;
        }

        public static double Multiply(double x, double y)
        {
            return x * y;
        }

        public static int Divide(int x, int y)
        {
            if (y == 0)
            {
                throw new DivideByZeroException("Denominator cannot be zero.");
            }
            return x / y;
        }

        public static int subtract(int a, int b)
        {
            return a - b;
        }
    }
}
