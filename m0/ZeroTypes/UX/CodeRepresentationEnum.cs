using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public enum CodeRepresentationEnum { OneLine, ManyLines, VertexAndManyLines, EdgeAndManyLines }

    class CodeRepresentationEnumHelper
    {
        static IVertex OneLine_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\CodeRepresentationEnum\OneLine");
        static IVertex ManyLines_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\CodeRepresentationEnum\ManyLines");
        static IVertex VertexAndManyLines_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\CodeRepresentationEnum\VertexAndManyLines");
        static IVertex EdgeAndManyLines_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\CodeRepresentationEnum\EdgeAndManyLines");

        public static CodeRepresentationEnum GetEnum(IVertex v)
        {
            if (v == null || v.Value == null)
                return CodeRepresentationEnum.VertexAndManyLines;

            switch (v.Value.ToString())
            {
                case "OneLine": return CodeRepresentationEnum.OneLine;

                case "ManyLines": return CodeRepresentationEnum.ManyLines;

                case "VertexAndManyLines": return CodeRepresentationEnum.VertexAndManyLines;

                case "EdgeAndManyLines": return CodeRepresentationEnum.EdgeAndManyLines;

                default: return CodeRepresentationEnum.VertexAndManyLines;
            }
        }

        public static IVertex GetVertex(CodeRepresentationEnum e)
        {
            switch (e)
            {
                case CodeRepresentationEnum.OneLine: return OneLine_meta;

                case CodeRepresentationEnum.ManyLines: return ManyLines_meta;

                case CodeRepresentationEnum.VertexAndManyLines: return VertexAndManyLines_meta;

                case CodeRepresentationEnum.EdgeAndManyLines: return EdgeAndManyLines_meta;
            }

            return VertexAndManyLines_meta;
        }
    }
}
