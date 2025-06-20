using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Util
{
    using System;
    using System.Reflection;

    public class DynamicMethodInvoker
    {
        /// <summary>
        /// Invokes a method from the specified assembly
        /// </summary>
        /// <param name="assemblyPath">Path to assembly file (.dll or .exe)</param>
        /// <param name="typeName">Full type name (namespace.className)</param>
        /// <param name="methodName">Name of the method to invoke</param>
        /// <param name="parameters">Method parameters (null if none)</param>
        /// <returns>Result of method invocation</returns>
        public static object InvokeMethod(string assemblyPath, string typeName, string methodName, object[] parameters = null)
        {
            try
            {
                // Load assembly
                Assembly assembly = Assembly.LoadFrom(assemblyPath);

                // Find type
                Type type = assembly.GetType(typeName);
                if (type == null)
                {
                    throw new ArgumentException($"Type not found: {typeName}");
                }

                // Find method
                MethodInfo method = type.GetMethod(methodName);
                if (method == null)
                {
                    throw new ArgumentException($"Method not found: {methodName} in type: {typeName}");
                }

                object result = null;

                if (method.IsStatic)
                {
                    // Invoke static method
                    result = method.Invoke(null, parameters);
                }
                else
                {
                    // Create instance and invoke instance method
                    object instance = Activator.CreateInstance(type);
                    result = method.Invoke(instance, parameters);
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while invoking method: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Invokes a method with parameters of specified types
        /// </summary>
        public static object InvokeMethodWithTypes(string assemblyPath, string typeName, string methodName, Type[] parameterTypes, object[] parameters)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(assemblyPath);
                Type type = assembly.GetType(typeName);

                if (type == null)
                {
                    throw new ArgumentException($"Type not found: {typeName}");
                }

                // Find method with specified parameter types
                MethodInfo method = type.GetMethod(methodName, parameterTypes);
                if (method == null)
                {
                    throw new ArgumentException($"Method not found: {methodName} with specified parameter types");
                }

                object result = null;

                if (method.IsStatic)
                {
                    result = method.Invoke(null, parameters);
                }
                else
                {
                    object instance = Activator.CreateInstance(type);
                    result = method.Invoke(instance, parameters);
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while invoking method: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Invokes a method on an existing object instance
        /// </summary>
        public static object InvokeMethodOnInstance(object instance, string methodName, object[] parameters = null)
        {
            try
            {
                Type type = instance.GetType();
                MethodInfo method = type.GetMethod(methodName);

                if (method == null)
                {
                    throw new ArgumentException($"Method not found: {methodName}");
                }

                return method.Invoke(instance, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while invoking method: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Lists all public methods in the specified type
        /// </summary>
        public static void ListMethods(string assemblyPath, string typeName)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(assemblyPath);
                Type type = assembly.GetType(typeName);

                if (type == null)
                {
                    Console.WriteLine($"Type not found: {typeName}");
                    return;
                }

                Console.WriteLine($"Methods in type {typeName}:");
                MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

                foreach (MethodInfo method in methods)
                {
                    string modifiers = method.IsStatic ? "static " : "";
                    Console.WriteLine($"  {modifiers}{method.ReturnType.Name} {method.Name}({string.Join(", ", Array.ConvertAll(method.GetParameters(), p => $"{p.ParameterType.Name} {p.Name}"))})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Usage example
        public static void Example()
        {
            try
            {
                // Example 1: Invoking a static method without parameters
                // object result1 = InvokeMethod(@"C:\path\to\your\assembly.dll", "MyNamespace.MyClass", "MyStaticMethod");

                // Example 2: Invoking a method with parameters
                // object[] parameters = { "Hello", 42 };
                // object result2 = InvokeMethod(@"C:\path\to\your\assembly.dll", "MyNamespace.MyClass", "MyMethod", parameters);

                // Example 3: Invoking a method with specified parameter types
                // Type[] paramTypes = { typeof(string), typeof(int) };
                // object[] parameters3 = { "Test", 123 };
                // object result3 = InvokeMethodWithTypes(@"C:\path\to\your\assembly.dll", "MyNamespace.MyClass", "MyOverloadedMethod", paramTypes, parameters3);

                // Example 4: Listing methods
                // ListMethods(@"C:\path\to\your\assembly.dll", "MyNamespace.MyClass");

                Console.WriteLine("Check the commented examples in the Example() method");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in example: {ex.Message}");
            }
        }
    }
}
