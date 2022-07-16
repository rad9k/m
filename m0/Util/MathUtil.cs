using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Util
{
    public class MathUtil
    {
        public static double RoundUp(double toRound, int digits)
        {
            if (toRound == 0)
                return 0;

            double step = System.Math.Pow(10, digits);

            if (toRound > 0)
            {
                double counter = 0;

                while (counter < toRound)
                    counter += step;

                return counter;
            }
            else
            {
                double counter = 0;

                while (counter > toRound)
                    counter -= step;

                return counter + step;
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

                while (counter < toRound)
                    counter += step;

                return counter - step;
            }
            else
            {
                double counter = 0;

                while (counter > toRound)
                    counter -= step;

                return counter;
            }
        }
    }
}
