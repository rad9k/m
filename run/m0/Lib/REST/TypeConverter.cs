using m0.Graph;
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
            "String", "Integer", "Float", "Double", "Boolean", "Decimal", "Null"
        };

        /// <summary>
        /// Checks if the given type name is a primitive GVM type.
        /// </summary>
        public static bool IsPrimitiveType(string typeName)
        {
            return PrimitiveTypes.Contains(typeName);                  
        }

        /// <summary>
        /// Gets the GVM type name from a JSON element value kind.
        /// </summary>
        public static string GetGvmTypeNameFromJsonElement(JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.String:
                    return "String";
                case JsonValueKind.Number:
                    if (value.TryGetInt64(out _))
                        return "Integer";
                    return "Double";
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return "Boolean";
                case JsonValueKind.Null:
                    return "Null";
                default:
                    return "String";
            }
        }

        /// <summary>
        /// Converts a JSON element to a .NET primitive value.
        /// </summary>
        public static object ConvertJsonElementToPrimitive(JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.String:
                    return value.GetString();
                case JsonValueKind.Number:
                    if (value.TryGetInt64(out long longValue))
                        return longValue;
                    return value.GetDouble();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                    return null;
                default:
                    return value.ToString();
            }
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
                case "Decimal":
                    writer.WriteString("type", "number");
                    writer.WriteString("format", "double");
                    break;
                case "Boolean":
                    writer.WriteString("type", "boolean");
                    break;
                case "Null":
                    writer.WriteString("type", "string");
                    isNullable = true;
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
