using m0.Foundation;
using m0_COMPOSER.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using m0.ZeroTypes;
using m0.Graph;

namespace m0_COMPOSER.Runtime
{
    public class Initialisation
    {
        static void AddDevices()
        {
            IVertex LocalComputer = m0.MinusZero.Instance.root.Get(false, @"Hardware\LocalComputer:");

            int numDevs = WinmmMidiLib.midiOutGetNumDevs();
    
            for (int x = 0; x < numDevs; x++)
            {
                MidiOutCaps myCaps = new MidiOutCaps();

                WinmmMidiLib.midiOutGetDevCaps(x, ref myCaps, (UInt32)Marshal.SizeOf(myCaps));

                AddDevices(LocalComputer, x, myCaps);
            }
    
        }

        static void AddDevices(IVertex baseVertex, int deviceNumber, MidiOutCaps caps)
        {
            IVertex r = m0.MinusZero.Instance.root;

            IVertex MidiDevice = r.Get(false, @"System\Lib\Music\MidiDevice");

            IVertex dev = VertexOperations.AddInstance(baseVertex, MidiDevice);

            string name = caps.szPname;

            dev.Value = name;            
            
            dev.AddVertex(MidiDevice.Get(false, "DeviceNumber"), deviceNumber);
            dev.AddVertex(MidiDevice.Get(false, "Mid"), (int)caps.wMid);
            dev.AddVertex(MidiDevice.Get(false, "Pid"), (int)caps.wPid);
            dev.AddVertex(MidiDevice.Get(false, "DriverVersion"), (int)caps.vDriverVersion);
            dev.AddVertex(MidiDevice.Get(false, "Technology"), (int)caps.wTechnology);
            dev.AddVertex(MidiDevice.Get(false, "Voices"), (int)caps.wVoices);
            dev.AddVertex(MidiDevice.Get(false, "Notes"), (int)caps.wNotes);            
            dev.AddVertex(MidiDevice.Get(false, "ChannelMask"), (int)caps.wChannelMask);
            dev.AddVertex(MidiDevice.Get(false, "Support"), (int)caps.dwSupport);

            AddOutputs(dev);
        }

        static void AddOutputs(IVertex MidiDevice)
        {
            IVertex r = m0.MinusZero.Instance.root;

            IVertex MidiOutput = r.Get(false, @"System\Lib\Music\MidiOutput");

            for (int channel = 0; channel <= 15; channel++)
            {
                IVertex Out = VertexOperations.AddInstance(MidiDevice, MidiOutput, r.Get(false, @"System\Lib\Music\MidiDevice\Output"));

                string name = MidiDevice.Value.ToString() + " [channel " + (channel + 1) + "]";

                Out.Value = name;

               // GraphUtil.SetVertexValue(Out, MidiOutput.Get(false, "Name"), name);
                GraphUtil.SetVertexValue(Out, MidiOutput.Get(false, "Channel"), channel);
                GraphUtil.CreateOrReplaceEdge(Out, MidiOutput.Get(false, "Device"), MidiDevice);
            }
        }

        public static void Execute()
        {
            AddDevices();
        }
    }
}
