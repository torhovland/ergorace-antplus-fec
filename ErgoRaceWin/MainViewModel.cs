using System;
using System.Threading.Tasks;

namespace ErgoRaceWin
{
    public class MainViewModel : ObservableObject
    {
        private readonly object lockObject = new object();
        private DateTime clock;
        private int heartRate;
        private int cadence;
        private double gradient;
        private double speed;
        private int userTargetPower = 25;
        private int bikeTargetPower = 25;
        private int currentBikePower;
        private int currentCalculatedPower;
        private Direction keyPadDirection = Direction.None;
        private readonly bool stopRequested = false;
        private Task clockLoopTask;
        private Task keyPadLoopTask;

        public MainViewModel()
        {
            clockLoopTask = Task.Run(() => ClockLoop());
            keyPadLoopTask = Task.Run(() => KeyPadLoop());
        }

        private async Task ClockLoop()
        {
            while (!stopRequested)
            {
                Clock = DateTime.Now;
                await Task.Delay(1000);
            }
        }

        private async Task KeyPadLoop()
        {
            var lastDirection = Direction.None;
            int streak = 0;
            const int repeatDelay = 20;

            while (!stopRequested)
            {
                if (keyPadDirection == lastDirection)
                    streak++;
                else
                    streak = 0;

                if (keyPadDirection == Direction.Up)
                {
                    if (streak == 0 || streak > repeatDelay)
                        UserTargetPower++;
                }
                else if (keyPadDirection == Direction.Down)
                {
                    if (streak == 0 || streak > repeatDelay)
                        UserTargetPower--;
                }
                else if (keyPadDirection == Direction.Left)
                {
                    if (streak == 0 || streak > repeatDelay)
                        UserTargetPower -= 10;
                }
                else if (keyPadDirection == Direction.Right)
                {
                    if (streak == 0 || streak > repeatDelay)
                        UserTargetPower += 10;
                }

                if (UserTargetPower < 0)
                    UserTargetPower = 0;

                lastDirection = keyPadDirection;

                await Task.Delay(20);
            }
        }

        public DateTime Clock
        {
            get => clock;
            set
            {
                lock (lockObject)
                {
                    clock = value;
                }

                RaisePropertyChangedEvent("Clock");
            }
        }

        public int HeartRate
        {
            get => heartRate;
            set
            {
                lock (lockObject)
                {
                    heartRate = value;
                }

                RaisePropertyChangedEvent("HeartRate");
            }
        }

        public int Cadence
        {
            get => cadence;
            set
            {
                lock (lockObject)
                {
                    cadence = value;
                }

                RaisePropertyChangedEvent("Cadence");
                Calculate();
            }
        }

        public double Gradient
        {
            get => gradient;
            set
            {
                lock (lockObject)
                {
                    gradient = value;
                }

                RaisePropertyChangedEvent("Gradient");
                Calculate();
            }
        }

        public double Speed
        {
            get => speed;
            set
            {
                lock (lockObject)
                {
                    speed = value;
                }

                RaisePropertyChangedEvent("Speed");
            }
        }

        private void Calculate()
        {
            double v = 0.0;
            double power = 0.0;

            lock (lockObject)
            {
                const double driveTrainLoss = .03;
                const double wheelCircumference = 2.0;
                const double bikeWeight = 10.0;
                const double riderWeight = 80.0;
                const double g = 9.8067;
                const double crr = .005;
                const double cd = .63;
                const double a = .5;
                const double rho = 1.226;
                const int chainRing = 34;
                const int sprocket = 16;

                var w = bikeWeight + riderWeight;
                v = Cadence / 60.0 * wheelCircumference * chainRing / sprocket;
                var fGravity = g * w * Math.Sin(Math.Atan(Gradient));
                var fRolling = g * w * crr * Math.Cos(Math.Atan(Gradient));
                var fDrag = .5 * cd * a * rho * v * v;
                power = (fGravity + fRolling + fDrag) * v / (1 - driveTrainLoss);
            }

            CurrentCalculatedPower = (int)Math.Round(power);
            Speed = v * 3600.0 / 1000.0;
        }

        public int UserTargetPower
        {
            get => userTargetPower;
            set
            {
                lock (lockObject)
                {
                    userTargetPower = value;
                }

                RaisePropertyChangedEvent("UserTargetPower");
            }
        }

        public int BikeTargetPower
        {
            get => bikeTargetPower;
            set
            {
                lock (lockObject)
                {
                    bikeTargetPower = value;
                }

                RaisePropertyChangedEvent("BikeTargetPower");
            }
        }

        public int CurrentBikePower
        {
            get => currentBikePower;
            set
            {
                lock (lockObject)
                {
                    currentBikePower = value;
                }

                RaisePropertyChangedEvent("CurrentBikePower");
            }
        }

        public int CurrentCalculatedPower
        {
            get => currentCalculatedPower;
            set
            {
                lock (lockObject)
                {
                    currentCalculatedPower = value;
                }

                RaisePropertyChangedEvent("CurrentCalculatedPower");
            }
        }

        public Direction KeyPadDirection
        {
            get => keyPadDirection;
            set
            {
                lock (lockObject)
                {
                    keyPadDirection = value;
                }

                RaisePropertyChangedEvent("KeyPadDirection");
            }
        }
    }
}