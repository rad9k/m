using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Util
{
    public class MathUtil
    {
        public static double Round(double toRound, int digits)
        {
            if (digits > 0)
                return Math.Floor (toRound);
            else
                return Math.Round(toRound, digits * -1);
        }

        public static double RoundUp(double toRound, int digits)
        {
            if (toRound == 0)
                return 0;

            double step = System.Math.Pow(10, digits);

            if (toRound > 0)
            {
                double counter = 0;

                while (MathUtil.Round(counter, digits) < toRound)
                    counter += step;

                return MathUtil.Round(counter, digits);
            }
            else
            {
                double counter = 0;

                while (MathUtil.Round(counter, digits) > toRound)
                    counter -= step;

                return MathUtil.Round(counter + step, digits);
            }
        }

        public static double RoundDown(double toRound, int digits)
        {
            if (toRound == 0)
                return 0;

            double step = System.Math.Pow(10, digits);

            if (toRound > 0)
            {
                double counter = 0;

                while (MathUtil.Round(counter, digits) < toRound)
                    counter += step;

                return MathUtil.Round(counter - step, digits);
            }
            else
            {
                double counter = 0;

                while (MathUtil.Round(counter, digits) > toRound)
                    counter -= step;

                return MathUtil.Round(counter, digits);
            }
        }
    }
}
