using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.LegacySystem
{
    public class LegacySystem
    {
        static public IVertex DefaultLanguageDefinition_ForOldParser;

        static public IVertex MetaFormalTextLanguageParsedTreeVertex;

        static public m0.LegacySystem.ZeroCode.ZeroCodeEngine_OLD ZeroCodeEngine_OLD;

        static public void LegacyInit()
        {
            CreateLegacySystem();

            ZeroCodeEngine_OLD = new m0.LegacySystem.ZeroCode.ZeroCodeEngine_OLD();

            Graph.EasyVertex.DefaultExecuter = ZeroCodeEngine_OLD;
            Graph.EasyVertex.DefaultParser = ZeroCodeEngine_OLD;
        }

        static void CreateLegacySystem()
        {
            IVertex legacySystem = m0.MinusZero.Instance.CreateTempVertex();
            //= m0.MinusZero.Instance.Root.AddVertex(null, "LegacySystem");

            legacySystem.AddExternalReference();

            IVertex meta = legacySystem.AddVertex(null, "Meta");

            IVertex tl = legacySystem.AddVertex(null, "FormalTextLanguage");

            IVertex mtl = meta.AddVertex(null, "FormalTextLanguage");

            // Meta\FormalTextLanguage\Parser

            IVertex mtp = mtl.AddVertex(null, "Parser");

            IVertex ptmd = mtp.AddVertex(null, "PreviousTerminalMoveDown");

            IVertex mdtpnltoce = mtp.AddVertex(null, "MoveDownToPreviousContainerTerminalOrCretedEmpty");

            IVertex ct = mtp.AddVertex(null, "ContainerTerminal");


            // Meta\FormalTextLanguage\ParsedTree

            IVertex mtpt = mtl.AddVertex(null, "ParsedTree");

            MetaFormalTextLanguageParsedTreeVertex = mtpt;

            IVertex empty = mtpt.AddVertex(null, "$EmptyContainerTerminal");
            empty.AddVertex(ct, null);


            // FormalTextLanguage\ZeroCode_OLD

            IVertex zco = tl.AddVertex(null, "ZeroCode_OLD");

            DefaultLanguageDefinition_ForOldParser = zco;

            zco.AddVertex(null, ",");

            IVertex colon = zco.AddVertex(null, ":");
            colon.AddVertex(ptmd, 1);
            colon.AddVertex(ct, null);

            zco.AddVertex(null, "\\");
            zco.AddVertex(null, "*");
            zco.AddVertex(null, "{").AddVertex(mdtpnltoce, null);
            zco.AddVertex(null, "}").AddVertex(mdtpnltoce, null);
            zco.AddVertex(null, "=");
            zco.AddVertex(null, "!=");
        }
    }
}
