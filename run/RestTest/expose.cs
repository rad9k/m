using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace RestTest
{
    public enum IsWise {
        Yes,
        No,
        Unknown,
        PreferNotToSay
    }

    public class User
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }

        public IsWise IsWise { get; set; }

        public IList<Product> Products { get; set; } = new List<Product>();
    }

    public class Product
    {
        public string ProductName { get; set; } = string.Empty;
        public double Price { get; set; }
    }

public class expose
    {
        public User ProcessUser(User input)
        {
            return input;
        }
        public static int Add(int a, int b)
        {
            return a + b;
        }

        public static string Concat(string str1, string str2)
        {
            return str1 + str2;
        }
    }
}
