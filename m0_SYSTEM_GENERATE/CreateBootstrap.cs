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
            system.AddVertex(null, "system.m0");

            IVertex user = r.AddVertex(null, "User");
            user.AddVertex(null, "user.m0");

            IVertex examples = r.AddVertex(null, "examples");
            examples.AddVertex(null, "examples.m0");

            IVertex lib_std = r.AddVertex(null, "System\\Lib\\Std");
            lib_std.AddVertex(null, "lib_std.m0");

            IVertex lib_sys = r.AddVertex(null, "System\\Lib\\Sys");
            lib_sys.AddVertex(null, "lib_sys.m0");

            if (music)
            {
                IVertex lib_music = r.AddVertex(null, "System\\Lib\\Music");
                lib_music.AddVertex(null, "lib_music.m0");
            }

            GeneralUtil.SaveStore(store);
        }
    }
}