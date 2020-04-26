using m0.Foundation;
using m0.Graph;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static m0_SYSTEM_GENERATE.Program;

namespace m0_SYSTEM_GENERATE.Music
{
    public class CreateMusic
    {
        public static void Save(List<IVertex> systemSubGraphWithLinks, Dictionary<string, StoreId> storeOverride)
        {            
            print("* saving Lib\\Music");

            GeneralUtil.CreateM0AndMoveEdgesIntoIt_IncludeEverythingBesidesList("lib_music.m0", music, systemSubGraphWithLinks, storeOverride);                        
        }

        static IVertex music;

        public static void Create()
        {
            print("* creating Lib\\Music");

            IVertex root = m0.MinusZero.Instance.root;

            IVertex lib = root.Get(false, @"System\Lib");

            music = lib.AddVertex(null, "Music");

            string type = "m0.Lib.Std, m0_COMPOSER, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

            IVertex midiDevice = GraphUtil.AddClass(music, "MidiDevice");

            IVertex midiOutput = GraphUtil.AddClass(music, "MidiOutput");            
        }
    }
}
