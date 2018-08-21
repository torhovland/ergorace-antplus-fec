using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace ErgoRaceWin
{
    public class Bike
    {
        private Task communicationLoopTask;
        private readonly bool stopRequested = false;
        private readonly MainViewModel model;

        public Bike(MainViewModel model)
        {
            this.model = model;
        }

        public void StartCommunication()
        {
            communicationLoopTask = Task.Run(() => CommunicationLoop());
        }

        public void CommunicationLoop()
        {
            const int heartRateIndex = 0;
            const int cadenceIndex = 1;
            const int targetPowerIndex = 4;
            const int currentPowerIndex = 7;
            const int esOffset = 4;

            var serialPorts = SerialPort.GetPortNames();
            var ergoRacePortName = serialPorts.FirstOrDefault();
            var ergoRace = new SerialPort(ergoRacePortName);
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
                ergoRace.WriteLine("PW 145");
                var pwValues = ergoRace.ReadLine().Trim().Split('\t');
                model.HeartRate = int.Parse(pwValues[heartRateIndex]);
                model.Cadence = int.Parse(pwValues[cadenceIndex]);
                model.TargetPower = int.Parse(pwValues[targetPowerIndex]);
                model.CurrentPower = int.Parse(pwValues[currentPowerIndex]);

                ergoRace.WriteLine("ES1");
                var esValues = ergoRace.ReadLine().Trim().Split('\t');
                model.HeartRate = int.Parse(esValues[heartRateIndex + esOffset]);
                model.Cadence = int.Parse(esValues[cadenceIndex + esOffset]);
                model.TargetPower = int.Parse(esValues[targetPowerIndex + esOffset]);
                model.CurrentPower = int.Parse(esValues[currentPowerIndex + esOffset]);
            }
        }
    }
}