using System;

namespace ErgoRaceWin
{
    public partial class MainViewModel
    {
        public static class BikeCalculator
        {
            public static double CalculateSpeed(int cadence, int chainRing, int sprocket)
            {
                const double wheelCircumference = 2.07;
                return cadence / 60.0 * wheelCircumference * chainRing / sprocket;
            }

            public static double CalculatePower(int cadence, double gradient, int chainRing, int sprocket)
            {
                const double driveTrainLoss = .03;
                const double bikeWeight = 10.0;
                const double riderWeight = 85.0; // Tor
                // const double riderWeight = 58.0; // Nina
                const double g = 9.8067;
                const double crr = .005;
                const double cd = .63;
                const double a = .5;
                const double rho = 1.226;
                const double w = bikeWeight + riderWeight;

                var v = CalculateSpeed(cadence, chainRing, sprocket);
                var fGravity = g * w * Math.Sin(Math.Atan(gradient));
                var fRolling = g * w * crr * Math.Cos(Math.Atan(gradient));
                var fDrag = .5 * cd * a * rho * v * v;
                return (fGravity + fRolling + fDrag) * v / (1 - driveTrainLoss);
            }
        }
    }
}