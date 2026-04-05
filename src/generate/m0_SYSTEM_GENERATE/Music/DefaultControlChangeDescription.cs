using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_SYSTEM_GENERATE.Music
{
    public class CCDescription
    {
        public int Number;
        public String Description;
        public String Type;

        public CCDescription(int number, String description, String type)
        {
            Number = number;
            Description = description;
            Type = type;
        }
    }

    public class DefaultControlChangeDescription
    {
        public static CCDescription[] DefaultControlChangeDescriptionSet = new CCDescription[] {
        new CCDescription(-1, "Velocity", ""),
        new CCDescription(0, "Bank Select", "MSB"),
        new CCDescription(1, "Modulation Wheel or Lever", "MSB"),
        new CCDescription(2, "Breath Controller", "MSB"),
        new CCDescription(3, "Undefined", "MSB"),
        new CCDescription(4, "Foot Controller", "MSB"),
        new CCDescription(5, "Portamento Time", "MSB"),
        new CCDescription(6, "Data Entry MSB", "MSB"),
        new CCDescription(7, "Channel Volume", "MSB"),
        new CCDescription(8, "Balance", "MSB"),
        new CCDescription(9, "Undefined", "MSB"),
        new CCDescription(10, "Pan", "MSB"),
        new CCDescription(11, "Expression Controller", "MSB"),
        new CCDescription(12, "Effect Control 1", "MSB"),
        new CCDescription(13, "Effect Control 2", "MSB"),
        new CCDescription(32, "Bank Select", "LSB"),
        new CCDescription(33, "Modulation Wheel or Lever", "LSB"),
        new CCDescription(34, "Breath Controller", "LSB"),
        new CCDescription(36, "Foot Controller", "LSB"),
        new CCDescription(37, "Portamento Time", "LSB"),
        new CCDescription(38, "Data Entry", "LSB"),
        new CCDescription(39, "Channel Volume", "LSB"),
        new CCDescription(40, "Balance", "LSB"),
        new CCDescription(42, "Pan", "LSB"),
        new CCDescription(43, "Expression Controller", "LSB"),
        new CCDescription(64, "Damper Pedal", "On/off"),
        new CCDescription(65, "Portamento", "On/Off"),
        new CCDescription(66, "Sostenuto", "On/Off"),
        new CCDescription(67, "Soft Pedal", "On/Off"),
        new CCDescription(68, "Legato Footswitch", ""),
        new CCDescription(69, "Hold 2", ""),
        new CCDescription(70, "Sound Variation", "LSB"),
        new CCDescription(71, "Timbre/Harmonic Intens.", "LSB"),
        new CCDescription(72, "Release Time", "LSB"),
        new CCDescription(73, "Attack Time", "LSB"),
        new CCDescription(74, "Brightness", "LSB"),
        new CCDescription(75, "Decay Time", "LSB"),
        new CCDescription(76, "Vibrato Rate", "LSB"),
        new CCDescription(77, "Vibrato Depth", "LSB"),
        new CCDescription(78, "Vibrato Delay", "LSB"),
        new CCDescription(84, "Portamento Control", "LSB"),
        new CCDescription(88, "High Resolution Velocity Prefix", "LSB"),
        new CCDescription(91, "Effects 1 Depth", ""),
        new CCDescription(92, "Effects 2 Depth", ""),
        new CCDescription(93, "Effects 3 Depth", ""),
        new CCDescription(94, "Effects 4 Depth", ""),
        new CCDescription(95, "Effects 5 Depth", ""),
        new CCDescription(98, "Non-Registered Parameter Number (NRPN)", "LSB"),
        new CCDescription(99, "Non-Registered Parameter Number (NRPN)", "MSB"),
        new CCDescription(100, "Registered Parameter Number (RPN)", "LSB"),
        new CCDescription(101, "Registered Parameter Number (RPN)", "MSB")
    };

    }
}
