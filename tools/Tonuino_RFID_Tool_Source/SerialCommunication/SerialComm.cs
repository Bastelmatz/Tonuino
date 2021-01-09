﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tonuino_RFID_Creator
{
    public class SerialComm
    {
        // *****************************
        // Port Handling
        // ***************************** 
        #region Port Handling

        public static List<string> FoundPorts = new List<string>();

        public static SerialPort ConnectedPort = null;

        public static bool DoListening = false;
        public static bool IsListening = false;
        public static Task listeningTask = null;

        public static void CheckComPorts()
        {
            string[] portNames = SerialPort.GetPortNames();

            FoundPorts.Clear();
            foreach (string portName in portNames)
            {
                FoundPorts.Add(portName);
            }
        }

        public static void OpenPort(string name)
        {
            ClosePort();

            SerialPort port = new SerialPort(name);

            port.BaudRate = 115200;
            port.Parity = Parity.None;
            port.StopBits = StopBits.One;
            port.DataBits = 8;
            port.Handshake = Handshake.None;
            port.Encoding = Encoding.UTF8;

            port.Open();
            ConnectedPort = port;

            DoListening = true;
            listenPort();
        }

        public static void ClosePort()
        {
            DoListening = false;
            ConnectedPort?.Close();

            DateTime startTime = DateTime.Now;
            // wait for listening task to finish - continue after 5s anyway
            while (IsListening && DateTime.Now - startTime < TimeSpan.FromSeconds(5))
            {
                Thread.Sleep(100);
            }
        }

        public static void WriteLine(string text)
        {
            ConnectedPort?.WriteLine(text);
        }

        private static void listenPort()
        {
            listeningTask = new Task(listenPortAction);

            listeningTask.Start();
        }

        private static void listenPortAction()
        {
            IsListening = true;
            while (ConnectedPort.IsOpen)
            {
                try
                {
                    string receivedData = ConnectedPort.ReadLine();
                    collectForCardData(receivedData);
                    MainForm.Instance.LogText(receivedData);
                }
                catch (Exception e)
                {
                    if (DoListening) // Don't show error if port is regularly closed
                    {
                        MainForm.Instance.LogText(e.Message);
                    }
                }
            }
            IsListening = false;
        }

        #endregion

        // *****************************
        // Card Data Handling
        // ***************************** 
        #region Card Data Handling

        public static CardData CurrentCardRead { get; set; } = new CardData();

        private static bool collectCardData = false;
        private static List<string> collectedData = new List<string>();

        private static string KEYRead_TriggerBEGIN { get; } = "Tonuino_RFID_Tool_BEGIN";
        private static string KEYRead_TriggerEND { get; } = "Tonuino_RFID_Tool_END";
        private static string KEYRead_CardData { get; } = "CardData";
        private static char Read_KeyValueSeparator { get; } = ':';

        private static void collectForCardData(string receivedText)
        {
            if (receivedText.Contains(KEYRead_TriggerEND))
            {
                // get section
                string section = "";
                if (collectedData.Count > 0)
                {
                    section = collectedData[0];
                    // remove section part
                    collectedData.RemoveAt(0);
                }

                // evaluate received data
                if (section.Contains(KEYRead_CardData))
                {
                    CurrentCardRead = getCardData(collectedData);
                    MainForm.Instance.UpdateCardData();
                }
                collectCardData = false;
            }
            if (collectCardData)
            {
                collectedData.Add(receivedText);
            }
            if (receivedText.Contains(KEYRead_TriggerBEGIN))
            {
                collectedData.Clear();
                collectCardData = true;
            }
        }

        private static CardData getCardData(List<string> textLines)
        {
            CardData data = new CardData();
            if (textLines.Count == 6)
            {
                // Defined order - has to be in sync with Tonuino sketch!!
                long rfid = getLong(textLines[0]);
                int cookie = getInt(textLines[1]);
                byte folder = getByte(textLines[2]);
                if (folder == 0)
                {
                    EModiType modiType = ModiType.FromNumber(getByte(textLines[3]));
                    TimeSpan sleepTime = TimeSpan.FromMinutes(getByte(textLines[4]));
                    data = new ModiCardData(rfid, cookie, modiType, sleepTime);
                }
                else
                {
                    EMusicMode mode = MusicMode.FromNumber(getByte(textLines[3]));
                    byte start = getByte(textLines[4]);
                    byte end = getByte(textLines[5]);
                    data = new MusicCardData(rfid, cookie, mode, folder, start, end);
                }
            }
            return data;
        }

        private static byte getByte(string text)
        {
            return (byte)getLong(text);
        }

        private static int getInt(string text)
        {
            return (int)getLong(text);
        }

        private static long getLong(string text)
        {
            text = text.Replace("\r", "");
            text = text.Replace(" ", "");
            string[] splitText = text.Split(Read_KeyValueSeparator);
            if (splitText.Length > 1)
            {
                long number;
                string sValue = splitText[1];
                if (long.TryParse(sValue, out number))
                {
                    return number;
                }
                if (long.TryParse(sValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out number))
                {
                    return number;
                }
            }
            return 0;
        }

        public static void Write(ICardData cardData)
        {
            string formattedLineText = toString(cardData);
            WriteLine(formattedLineText);
        }

        private const string SEPARATOR = ";";

        private static string toString(ICardData data)
        {
            // Defined order - has to be in sync with Tonuino sketch - don't change!!
            List<byte> items = new List<byte>()
            {
                data.Raw_Folder,
                data.Raw_Mode,
                data.Raw_Special,
                data.Raw_Special2
            };
            return string.Join(SEPARATOR, items);
        }

        #endregion
    }
}