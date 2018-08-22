using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using ANT_Managed_Library;

namespace ErgoRaceWin
{
    internal class AntCommunication
    {
        private readonly MainViewModel model;
        ANT_Device antDevice;
        private byte eventNumber = 0;

        delegate void dAppendText(String toAppend);

        public AntCommunication(MainViewModel model)
        {
            this.model = model;
        }

        public void StartCommunication()
        {
            antDevice = new ANT_Device();
            var networkKeyString = File.ReadAllLines("ant-network.key").FirstOrDefault(l => !l.StartsWith("#"))?.Trim();
            antDevice.setNetworkKey(0, StringToByteArray(networkKeyString));
            antDevice.deviceResponse += new ANT_Device.dDeviceResponseHandler(device0_deviceResponse);
            antDevice.getChannel(0).channelResponse += new dChannelResponseHandler(d0channel0_channelResponse);
            threadSafePrintLine("ANT+ USB Device Connected");
            setupAndOpen(antDevice, ANT_ReferenceLibrary.ChannelType.BASE_Master_Transmit_0x10);
            SetNextBroadcastMessage();
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private void d0channel0_channelResponse(ANT_Response response)
        {
            threadSafePrintLine(decodeChannelFeedback(response));
        }

        private void device0_deviceResponse(ANT_Response response)
        {
            threadSafePrintLine(decodeDeviceFeedback(response));
        }

        void threadSafePrintLine(String stringToPrint, TextBox boxToPrintTo = null)
        {
            return;

            if (String.IsNullOrWhiteSpace(stringToPrint))
                return;

            //We need to put this on the dispatcher because sometimes it is called by the feedback thread
            //If you set the priority to 'background' then it never interferes with the UI interaction if you have a high message rate (we don't have to worry about it in the demo)
            boxToPrintTo.Dispatcher.BeginInvoke(new dAppendText(boxToPrintTo.AppendText), System.Windows.Threading.DispatcherPriority.Background, stringToPrint + Environment.NewLine);
        }

        void SetNextBroadcastMessage()
        {
            var messageNumber = eventNumber % 4;

            if (messageNumber < 2)
                SetGeneralBroadcastMessage();
            else
                SetTrainerBroadcastMessage(eventNumber);

            eventNumber++;
        }

        private void SetGeneralBroadcastMessage()
        {
            //At the given frequency specified by the set message period, a master channel will broadcast the contents of its buffer.
            //This buffer contains whatever was last sent by the device, so it is important to manage it if the master broadcast
            //contains information being used elsewhere. It is recommended to set the broadcast message to what it should be every time
            //you receive an EVENT_TX message
            //The sendBroadcast function sets the buffer. On a slave device, a single broadcast
            //message will be sent.
            byte[] randArray = new byte[8];
            randArray[0] = 0x10; // General FE Data Page
            randArray[1] = 25; // Trainer
            randArray[2] = 0;
            randArray[3] = 0;
            randArray[4] = 0xFF;
            randArray[5] = 0xFF;
            randArray[6] = 0xFF;
            randArray[7] = 0x18;

            if (!antDevice.getChannel(0).sendBroadcastData(randArray))
                threadSafePrintLine("Broadcast Message Failed");
        }

        private void SetTrainerBroadcastMessage(byte eventNumber)
        {
            var power = model.CurrentPower;

            byte[] randArray = new byte[8];
            randArray[0] = 0x19; // Trainer Data Page
            randArray[1] = eventNumber;
            randArray[2] = (byte)model.Cadence;
            randArray[3] = 0;
            randArray[4] = 0;
            randArray[5] = (byte)power;
            randArray[6] = (byte)((power & 0xFF00) >> 8);
            randArray[7] = 0x10;

            if (!antDevice.getChannel(0).sendBroadcastData(randArray))
                threadSafePrintLine("Broadcast Message Failed");
        }

        //This function decodes the message code into human readable form, shows the error value on failures, and also shows the raw message contents
        String decodeDeviceFeedback(ANT_Response response)
        {
            string toDisplay = "Device: ";

            //The ANTReferenceLibrary contains all the message and event codes in user-friendly enums
            //This allows for more readable code and easy conversion to human readable strings for displays

            // So, for the device response we first check if it is an event, then we check if it failed,
            // and display the failure if that is the case. "Events" use message code 0x40.
            if (response.responseID == (byte)ANT_ReferenceLibrary.ANTMessageID.RESPONSE_EVENT_0x40)
            {
                //We cast the byte to its messageID string and add the channel number byte associated with the message
                toDisplay += (ANT_ReferenceLibrary.ANTMessageID)response.messageContents[1] + ", Ch:" + response.messageContents[0];
                //Check if the eventID shows an error, if it does, show the error message
                if ((ANT_ReferenceLibrary.ANTEventID)response.messageContents[2] != ANT_ReferenceLibrary.ANTEventID.RESPONSE_NO_ERROR_0x00)
                    toDisplay += Environment.NewLine + ((ANT_ReferenceLibrary.ANTEventID)response.messageContents[2]).ToString();
            }
            else   //If the message is not an event, we just show the messageID
                toDisplay += ((ANT_ReferenceLibrary.ANTMessageID)response.responseID).ToString();

            //Finally we display the raw byte contents of the response, converting it to hex
            toDisplay += Environment.NewLine + "::" + Convert.ToString(response.responseID, 16) + ", " + BitConverter.ToString(response.messageContents) + Environment.NewLine;
            return toDisplay;
        }

        String decodeChannelFeedback(ANT_Response response)
        {
            //Decoding channel events is an important consideration when building applications.
            //The channel events will be where broadcast, ack and burst messages are received, you
            //are also notified of messages sent and, for acknowledged and burst messages, their success or failure.
            //In a burst transfer, or if the device is operating at a small message period, then it is important that
            //decoding the messages is done quickly and efficiently.
            //The way the response retrieval works, messages should never be lost. If you decode messages too slow, they are simply queued.
            //However, you will be retrieving messages at a longer and longer delay from when they were actually received by the device.

            //One further consideration when handling channel events is to remember that the feedback functions will be triggered on a different thread
            //which is operating within the managed library to retrieve the feedback from the device. So, if you need to interact with your UI objects
            //you will need to use a Dispatcher invoke with a prority that will not make the UI unresponsive (see the threadSafePrint function used here).

            StringBuilder stringToPrint;    //We use a stringbuilder for speed and better memory usage, but, it doesn't really matter for the demo.
            stringToPrint = new StringBuilder(100); //Begin the string and allocate some more space

            //In the channel feedback we will get either RESPONSE_EVENTs or receive events,
            //If it is a response event we display what the event was and the error code if it failed.
            //Mostly, these response_events will all be broadcast events from a Master channel.
            if (response.responseID == (byte)ANT_ReferenceLibrary.ANTMessageID.RESPONSE_EVENT_0x40 &&
                (ANT_ReferenceLibrary.ANTEventID)response.messageContents[2] ==
                ANT_ReferenceLibrary.ANTEventID.EVENT_TX_0x03)
            {
                SetNextBroadcastMessage();
            }
            else if (response.responseID == (byte)ANT_ReferenceLibrary.ANTMessageID.ACKNOWLEDGED_DATA_0x4F)
            {
                var controlPage = response.messageContents[1];

                if (controlPage == 0x31) // Target power
                {
                    var lsb = response.messageContents[7];
                    var msb = response.messageContents[8];
                    var power = (msb * 256 + lsb) / 4.0;
                    model.UserTargetPower = (int)Math.Round(power);
                    stringToPrint.AppendLine($"Target power set to {power} W.");
                }
                else if (controlPage == 0x33) // Track resistance
                {
                    var lsb = response.messageContents[6];
                    var msb = response.messageContents[7];
                    var gradient = (msb * 256 + lsb) / 100.0 - 200.0;
                    if (msb == 0xFF && lsb == 0xFF) gradient = 0.0;
                    model.Gradient = gradient;
                    stringToPrint.AppendLine($"Gradient set to {gradient} %.");
                }
                else
                {
                    throw new NotImplementedException($"Control page {controlPage}.");
                }
            }
            else if (response.responseID == (byte)ANT_ReferenceLibrary.ANTMessageID.RESPONSE_EVENT_0x40)
                stringToPrint.AppendLine(((ANT_ReferenceLibrary.ANTEventID)response.messageContents[2]).ToString());
            else   //This is a receive event, so display the ID
                stringToPrint.AppendLine("Received " + ((ANT_ReferenceLibrary.ANTMessageID)response.responseID));

            return stringToPrint.ToString();
        }

        private void setupAndOpen(ANT_Device deviceToSetup, ANT_ReferenceLibrary.ChannelType channelType)
        {
            //We try-catch and forward exceptions to the calling function to handle and pass the errors to the user
            try
            {
                //To access an ANTChannel on a paticular device we need to get the channel from the device
                //Once again, this ensures you have a valid object associated with a real-world ANTChannel
                //ie: You can only get channels that actually exist
                ANT_Channel channel0 = deviceToSetup.getChannel(0);

                //Almost all functions in the library have two overloads, one with a response wait time and one without
                //If you give a wait time, you can check the return value for success or failure of the command, however
                //the wait time version is blocking. 500ms is usually a safe value to ensure you wait long enough for any response.
                //But with no wait time, the command is simply sent and you have to monitor the device response for success or failure.

                //To setup channels for communication there are three mandatory operations assign, setID, and Open
                //Various other settings such as message period and network key affect communication
                //between two channels as well, see the documentation for further details on these functions.

                //So, first we assign the channel, we have already been passed the channelType which is an enum that has various flags
                //If we were doing something more advanced we could use a bitwise or ie:base|adv1|adv2 here too
                //We also use net 0 which has the public network key by default
                if (channel0.assignChannel(channelType, 0, 500))
                    threadSafePrintLine("Ch assigned to " + channelType + " on net 0.");
                else
                    throw new Exception("Channel assignment operation failed.");

                //Next we have to set the channel id. Slaves will only communicate with a master device that
                //has the same id unless one or more of the id parameters are set to a wild card 0. If wild cards are included
                //the slave will search until it finds a broadcast that matches all the non-wild card parameters in the id.
                //For now we pick an arbitrary id so that we can ensure we match between the two devices.
                //The pairing bit ensures on a search that you only pair with devices that also are requesting
                //pairing, but we don't need it here so we set it to false
                if (channel0.setChannelID(12345, false, 17, 5, 500))
                    threadSafePrintLine("Set channel ID to 12345, 17, 5");
                else
                    throw new Exception("Set Channel ID operation failed.");

                //Setting the channel period isn't mandatory, but we set it slower than the default period so messages aren't coming so fast
                //The period parameter is divided by 32768 to set the period of a message in seconds. So here, 16384/32768 = 1/2 sec/msg = 2Hz
                if (channel0.setChannelPeriod(8192, 500))
                    threadSafePrintLine("Message Period set to 8192/32768 seconds per message");

                if (channel0.setChannelFreq(57, 500))
                    threadSafePrintLine("Frequency set to 57");

                //Now we open the channel
                if (channel0.openChannel(500))
                    threadSafePrintLine("Opened Channel" + Environment.NewLine);
                else
                    throw new Exception("Channel Open operation failed.");
            }
            catch (Exception ex)
            {
                throw new Exception("Setup and Open Failed. " + ex.Message + Environment.NewLine);
            }
        }
    }
}