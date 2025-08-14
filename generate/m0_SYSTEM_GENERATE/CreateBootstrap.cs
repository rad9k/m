using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using m0.Store.Json;
using m0.Foundation;
using m0.Util;
using m0.Graph;
using System.IO;

namespace m0_SYSTEM_GENERATE
{
    class CreateBootstrap
    {
        public static void Create(string fileName, bool music)
        {
            File.Delete(fileName);

            JsonSerializationStore store = new JsonSerializationStore(fileName, m0.MinusZero.Instance.root.Store.StoreUniverse, new AccessLevelEnum[] { });

            IVertex r = store.Root;

            IVertex system = r.AddVertex(null, "System");
            system.AddVertex(null, "system.m0j");

            IVertex user = r.AddVertex(null, "User");
            user.AddVertex(null, "user.m0j");

            IVertex quick = r.AddVertex(null, "Quick");
            quick.AddVertex(null, "quick.m0j");

            IVertex examples = r.AddVertex(null, "examples");
            examples.AddVertex(null, "examples.m0j");

            IVertex lib_std = r.AddVertex(null, "System\\Lib\\Std");
            lib_std.AddVertex(null, "lib_std.m0j");

            IVertex lib_sys = r.AddVertex(null, "System\\Lib\\Sys");
            lib_sys.AddVertex(null, "lib_sys.m0j");

            IVertex lib_stdview = r.AddVertex(null, "System\\Lib\\StdView");
            lib_stdview.AddVertex(null, "lib_stdview.m0j");

            IVertex lib_stdui = r.AddVertex(null, "System\\Lib\\StdUI");
            lib_stdui.AddVertex(null, "lib_stdui.m0j");

            IVertex lib_net = r.AddVertex(null, "System\\Lib\\Net");
            lib_net.AddVertex(null, "lib_net.m0j");

            if (music)
            {
                IVertex lib_music = r.AddVertex(null, "System\\Lib\\Music");
                lib_music.AddVertex(null, "lib_music.m0j");
            }

            GeneralUtil.SaveStore(store);
        }
    }
}