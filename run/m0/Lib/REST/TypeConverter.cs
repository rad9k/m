using System.Collections.Generic;
using System.Text.Json;

namespace m0.Lib.REST
{
    /// <summary>
    /// Handles conversion between GVM types and JSON/OpenAPI types.
    /// </summary>
    public static class TypeConverter
    {
        // Primitive type names in GVM
        private static readonly HashSet<string> PrimitiveTypes = new HashSet<string>
        {
            "String", "Integer", "Float", "Double", "Boolean", "Decimal"
        };

        /// <summary>
        /// Checks if the given type name is a primitive GVM type.
        /// </summary>
        public static bool IsPrimitiveType(string typeName)
        {
            return PrimitiveTypes.Contains(typeName);
        }

        /// <summary>
        /// Writes the OpenAPI type definition for a GVM type.
        /// </summary>
        public static void WriteOpenApiTypeDefinition(Utf8JsonWriter writer, string typeName, bool isNullable = false)
        {
            writer.WriteStartObject();

            switch (typeName)
            {
                case "Integer":
                    writer.WriteString("type", "integer");
                    writer.WriteString("format", "int32");
                    break;
                case "Float":
                case "Double":
                    writer.WriteString("type", "number");
                    writer.WriteString("format", "double");
                    break;
                case "Boolean":
                    writer.WriteString("type", "boolean");
                    break;
                case "String":
                default:
                    writer.WriteString("type", "string");
                    break;
            }

            if (isNullable)
                writer.WriteBoolean("nullable", true);

            writer.WriteEndObject();
        }
    }
}
