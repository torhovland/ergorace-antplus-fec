namespace ErgoRaceWin
{
    public class MainViewModel : ObservableObject
    {
        private readonly object lockObject = new object();
        private int heartRate = 0;
        private int cadence = 0;
        private int targetPower = 0;
        private int currentPower = 0;

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

        public int TargetPower
        {
            get => targetPower;
            set
            {
                lock (lockObject)
                {
                    targetPower = value;
                }

                RaisePropertyChangedEvent("TargetPower");
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
    }
}