using System.Threading.Tasks;

namespace ErgoRaceWin
{
    public class MainViewModel : ObservableObject
    {
        private readonly object lockObject = new object();
        private int heartRate = 0;
        private int cadence = 0;
        private int userTargetPower = 25;
        private int bikeTargetPower = 25;
        private int currentPower = 0;
        private Direction keyPadDirection = Direction.None;
        private readonly bool stopRequested = false;
        private Task keyPadLoopTask;

        public MainViewModel()
        {
            keyPadLoopTask = Task.Run(() => KeyPadLoop());
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
            }
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

        public int CurrentPower
        {
            get => currentPower;
            set
            {
                lock (lockObject)
                {
                    currentPower = value;
                }

                RaisePropertyChangedEvent("CurrentPower");
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