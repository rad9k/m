using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.Base
{
    public class RealTime
    {
        public int Minute { get; set; }

        public int Second { get; set; }

        public int Milisecond { get; set; }        

        public int Combined
        {
            get
            {
                return Milisecond + (Second * 100) + (Minute * 100 * 100);
            }

            set
            {
                int MilisecondsSeconds = value % (100 * 100);

                Minute = (value - MilisecondsSeconds) / (100 * 100);

                Milisecond = MilisecondsSeconds % 100;

                Second = (MilisecondsSeconds - Milisecond) / 100;
            }
        }

        public double Minutes
        {
            get
            {
                return Minute + (Second / 60.0) + (Milisecond / (100.0 * 60.0));
            }

            set
            {
                Minute = (int)Math.Floor(value);

                double rest = value - Minute;

                rest = rest * 60;

                Second = (int)Math.Floor(rest);

                rest = rest - Second;

                rest = rest * 100;

                Milisecond = GeneralUtil.Double2Int(rest);
            }
        }

        public MusicTime GetMusicTime(double bpm)
        {
            MusicTime mt = new MusicTime();

            mt.Combined = GeneralUtil.Double2Int(bpm * Minutes * MusicTime.TicksPerBeat);

            return mt;
        }

        public void SetMusicTime(double bpm, MusicTime musicTime)
        {
            Minutes = musicTime.Combined / (bpm * MusicTime.TicksPerBeat);
        }

    }
}
