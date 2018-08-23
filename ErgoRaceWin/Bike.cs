using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using RJCP.IO.Ports;

namespace ErgoRaceWin
{
    public class Bike
    {
        private Task bikeCommunicationLoopTask;
        private Task serialKeyPadCommunicationLoopTask;
        private readonly bool stopRequested = false;
        private readonly MainViewModel model;

        public Bike(MainViewModel model)
        {
            this.model = model;
        }

        public void StartCommunication()
        {
            bikeCommunicationLoopTask = Task.Run(() => BikeCommunicationLoop());
            serialKeyPadCommunicationLoopTask = Task.Run(() => SerialKeyPadCommunicationLoop());
        }

        public void BikeCommunicationLoop()
        {
            const int heartRateIndex = 0;
            const int cadenceIndex = 1;
            const int targetPowerIndex = 4;
            const int currentPowerIndex = 7;
            const int esOffset = 4;

            var ergoRace = new SerialPort("COM3");
            ergoRace.Open();

            do
            {
                ergoRace.WriteLine("RS");
            } while (ergoRace.ReadLine().Trim() != "ACK");

            do
            {
                ergoRace.WriteLine("CM");
            } while (ergoRace.ReadLine().Trim() != "ACK");

            do
            {
                ergoRace.WriteLine("SP1");
            } while (ergoRace.ReadLine().Trim() != "ACK");

            do
            {
                ergoRace.WriteLine("LB");
            } while (ergoRace.ReadLine().Trim() != "ACK");

            while (!stopRequested)
            {
                ergoRace.WriteLine($"PW {model.TargetPower}");
                var pwValues = ergoRace.ReadLine().Trim().Split('\t');
                model.HeartRate = int.Parse(pwValues[heartRateIndex]);
                model.Cadence = int.Parse(pwValues[cadenceIndex]);
                model.BikeTargetPower = int.Parse(pwValues[targetPowerIndex]);
                model.CurrentBikePower = int.Parse(pwValues[currentPowerIndex]);

                ergoRace.WriteLine("ES1");
                var esValues = ergoRace.ReadLine().Trim().Split('\t');
                model.HeartRate = int.Parse(esValues[heartRateIndex + esOffset]);
                model.Cadence = int.Parse(esValues[cadenceIndex + esOffset]);
                model.BikeTargetPower = int.Parse(esValues[targetPowerIndex + esOffset]);
                model.CurrentBikePower = int.Parse(esValues[currentPowerIndex + esOffset]);
            }
        }

        public async Task SerialKeyPadCommunicationLoop()
        {
            var lastDirection = Direction.None;

            var stream = new SerialPortStream("COM4");
            stream.Open();

            while (!stopRequested)
            {
                var cts = stream.CtsHolding;
                var dsr = stream.DsrHolding;
                var ring = stream.RingHolding;
                var cd = stream.CDHolding;
                var direction = Direction.None;

                if (cts && ring)
                    direction = Direction.Up;
                else if (dsr)
                    direction = Direction.Down;
                else if (cd)
                    direction = Direction.Left;
                else if (cts)
                    direction = Direction.Right;

                if (direction != lastDirection)
                {
                    model.KeyPadDirection = direction;
                    lastDirection = direction;
                }

                await Task.Delay(1);
            }

        }
    }
}