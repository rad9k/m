using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes
{
    public enum CodeRepresentationEnum { EdgeOneLine, LinearizedManyLines, ManyLinesExcludingParent, VertexAndManyLines, EdgeAndManyLines }

    public class CodeRepresentationEnumHelper
    {
        static IVertex OneLine_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\CodeRepresentationEnum\OneLine");
        static IVertex EdgeOneLine_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\CodeRepresentationEnum\EdgeOneLine");
        static IVertex LinearizedManyLines_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\CodeRepresentationEnum\LinearizedManyLines");
        static IVertex ManyLinesExcludingParent_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\CodeRepresentationEnum\ManyLinesExcludingParent");
        static IVertex VertexAndManyLines_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\CodeRepresentationEnum\VertexAndManyLines");
        static IVertex EdgeAndManyLines_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\CodeRepresentationEnum\EdgeAndManyLines");

        public static CodeRepresentationEnum GetEnum(IVertex v)
        {
            if (v == null || v.Value == null)
                return CodeRepresentationEnum.VertexAndManyLines;

            switch (v.Value.ToString())
            {                
                case "EdgeOneLine": return CodeRepresentationEnum.EdgeOneLine;

                case "LinearizedManyLines": return CodeRepresentationEnum.LinearizedManyLines;

                case "ManyLinesExcludingParent": return CodeRepresentationEnum.ManyLinesExcludingParent;

                case "VertexAndManyLines": return CodeRepresentationEnum.VertexAndManyLines;

                case "EdgeAndManyLines": return CodeRepresentationEnum.EdgeAndManyLines;

                default: return CodeRepresentationEnum.VertexAndManyLines;
            }
        }

        public static IVertex GetVertex(CodeRepresentationEnum e)
        {
            switch (e)
            {                
                case CodeRepresentationEnum.EdgeOneLine: return EdgeOneLine_meta;

                case CodeRepresentationEnum.LinearizedManyLines: return LinearizedManyLines_meta;

                case CodeRepresentationEnum.ManyLinesExcludingParent: return ManyLinesExcludingParent_meta;

                case CodeRepresentationEnum.VertexAndManyLines: return VertexAndManyLines_meta;

                case CodeRepresentationEnum.EdgeAndManyLines: return EdgeAndManyLines_meta;
            }

            return VertexAndManyLines_meta;
        }
    }
}
