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
                return Minute + (Second / 60) + (Milisecond / (100*60));
            }

            set
            {
                int MilisecondsSeconds = (int)(value % (60 * 100));

                Minute = (int) (value - MilisecondsSeconds) / (60 * 100);

                Milisecond = MilisecondsSeconds % 100;

                Second = (MilisecondsSeconds - Milisecond) / 60;
            }
        }

        public MusicTime GetMusicTime(double bpm)
        {
            return null;
        }

        public void SetMusicTime(double bpm, MusicTime musicTime)
        {

        }

    }
}
