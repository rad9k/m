using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace m0.UIWpf.Visualisers
{
    public class IntegerVisualiser: NumberVisualiser<int?>
    {
        protected new string visualiserName { get { return "IntegerVisualiser"; } set { } }

        protected new IVertex visualiserMetaVertex { get { return MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Integer"); } set { } }
    }
}
