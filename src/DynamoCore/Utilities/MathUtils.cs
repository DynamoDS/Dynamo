using System;
using Dynamo.Units;

namespace Dynamo.Utilities
{
    class MathUtils
    {
        public static SIUnit Round(SIUnit input)
        {
            return input.Round();
        }

        public static double Round(double input)
        {
            return Math.Round(input);
        }

        public static SIUnit Pow(Length input, double power)
        {
            if (power == 2)
            {
                return new Area(Math.Pow(input.Value, power));
            }
            else if (power == 3)
            {
                return new Volume(Math.Pow(input.Value, power));
            }

            throw new MathematicalArgumentException();
        }

        public static double Pow(double input, double power)
        {
            return Math.Pow(input, power);
        }

        public static SIUnit Ceiling(SIUnit input)
        {
            return input.Ceiling();
        }

        public static double Ceiling(double input)
        {
            return Math.Ceiling(input);
        }

        public static SIUnit Floor(SIUnit input)
        {
            return input.Floor();
        }

        public static double Floor(double input)
        {
            return Math.Floor(input);
        }
    }
}
