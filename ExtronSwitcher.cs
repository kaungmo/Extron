using System;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace Extron
{
    class ExtronSwitcher
    {
        //This is V2 Branch
        private SerialPort  serialPort = null;

        private int inputSize = 0;
        private int outputSize = 0;
        private int _scanIndex = 0;
        private ManualResetEvent syncDone = new ManualResetEvent(false);
        private StringBuilder sb = new StringBuilder();

        public string CurrentTies
        {
            get { return sb.ToString(); }
        }
        
        #region Constructors
        
        ~ExtronSwitcher()
        {
            if (serialPort != null && serialPort.IsOpen)
                serialPort.Close();
        }

        public ExtronSwitcher()
        {
            serialPort = new SerialPort();
        }

        public ExtronSwitcher(string portName)
        {
            serialPort = new SerialPort(portName);
        }

        public ExtronSwitcher(string portName, int baudRate)
        {
            serialPort = new SerialPort(portName, baudRate);
        }

        public ExtronSwitcher(string portName, int baudRate, Parity parity)
        {
            serialPort = new SerialPort(portName, baudRate, parity);
        }

        public ExtronSwitcher(string portName, int baudRate, Parity parity, int dataBits)
        {
            serialPort = new SerialPort(portName, baudRate, parity, dataBits);
        }

        public ExtronSwitcher(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
        }
        #endregion

        #region Port Related Functions
        public bool Open()
        {
            try
            {
                serialPort.Open();
                serialPort.ReadTimeout = 500;
                serialPort.Handshake = Handshake.None;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }

        public void Close()
        {
            serialPort.Close();
        }
        #endregion

        #region Helper Functions
        private string Write(string input)
        {
            try
            {
                serialPort.Write(input);
                Thread.Sleep(100);
                return serialPort.ReadLine().ToUpper();
            }
            catch (TimeoutException)
            {
                return "timeout";
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
            return "";
        }

        private bool CheckError(string input)
        {
            if (input.Contains("E01") || input.Contains("E11") || input.Contains("E12") ||
                input.Contains("E13") || input.Contains("E14") || input.Contains("TIMEOUT"))
                return true;
            return false;
        }
        #endregion

        #region Switcher Functions


        private void GetMatrixSize()
        {
            string responseString = Write( string.Format("I", Environment.NewLine));
            ProcessSwitching(responseString);
        }

        public void Synchronize()
        {
            if (inputSize > 0 && outputSize > 0)
            {
                sb = new StringBuilder();
                _scanIndex = 1;
                while (_scanIndex < outputSize)
                {
                    string commandString = string.Format("{0}0*{1}*1VC{2}", Convert.ToChar(0x1b), _scanIndex, Environment.NewLine);
                    ProcessSwitching(Write(commandString));
                    //syncDone.WaitOne();

                    _scanIndex += 16;
                    sb.Append(" ");
                }
                int i = 10;
            }
            else
            {
                GetMatrixSize();
            }
        }
       
        public int ReadTie(int output)
        {
            string retString = Write(string.Format("{0}%", output));
            string[] array = null;

            if (CheckError(retString)) 
                return -1;

            array = retString.Split(new string[] { " ", "\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
            if (array != null && array.Length == 1)
                return Convert.ToInt32(array[0]);
            return -1;
        }

        public int Tie(int input, int output)
        {
            string retString = Write(string.Format("{0}*{1}!", input, output));
            string[] array = null;

            if (CheckError(retString))
                return -1;

            array = retString.Split(new string[] { " ", "\r", "\n", "OUT", "IN", "ALL"}, StringSplitOptions.RemoveEmptyEntries);
            if (array != null && array.Length == 2)
                return Convert.ToInt32(array[1]);
            return -1;
        }

        private void ProcessSwitching(string responseString)
        {
            if (CheckError(responseString)) return;
            responseString = responseString.ToLower();

            if (responseString.Contains("in") || responseString.Contains("out"))
            {
                //Single Switch
                //string[] array = null;
                //array = responseString.Split(new string[] { " ", "\r", "\n", "out", "in", "all" }, StringSplitOptions.RemoveEmptyEntries);
                //if (array != null && array.Length == 2)
                //{
                //    List<SwitchingStatus> list = new List<SwitchingStatus>();
                //    var outputDeviceID = outputPoints.Where(arg => arg.MATRIX_NAME == array[0]).Select(arg => arg.DEVICE_ID).FirstOrDefault();
                //    var inputDeviceID = inputPoints.Where(arg => arg.MATRIX_NAME == array[1]).Select(arg => arg.DEVICE_ID).FirstOrDefault();

                //    list.Add(new SwitchingStatus { InputDeviceId = inputDeviceID, OutputDeviceId = outputDeviceID });

                //    UpdateSwitchingStatus(list);
                //    RaiseSwitchingStatusChangedEvent(list);
                //}
            }
            else if (responseString.Contains("vid"))
            {
                //Synchronize
                string[] array = null;
                array = responseString.Split(new string[] { " ", "\r", "\n", "out", "in", "-", "all", "vid" }, StringSplitOptions.RemoveEmptyEntries);
                if (array != null)
                {sb.Append(string.Join(" ", array));
                    //List<SwitchingStatus> list = new List<SwitchingStatus>();
                    for (int i = 1; i <= array.Length; i++)
                    {
                        int idx = (i + _scanIndex - 1);

                        

                       // var output = outputPoints.FirstOrDefault(arg => arg.MATRIX_NAME == idx.ToString("00"));
                        //var input = inputPoints.FirstOrDefault(arg => arg.MATRIX_NAME == array[i - 1]);
                        //if (output != null && input != null)
                        //    list.Add(new SwitchingStatus { InputDeviceId = input.DEVICE_ID, OutputDeviceId = output.DEVICE_ID });
                    }
                    //UpdateSwitchingStatus(list);
                    //syncDone.Set();
                }
            }
            else if (responseString.Contains(" a") && responseString.Contains("x") && responseString.Contains("v"))
            {
                string[] array = null;
                array = responseString.Split(new string[] { " ", "\r", "\n", "v", "a", "x" }, StringSplitOptions.RemoveEmptyEntries);
                if (array != null && array.Length >= 4)
                {
                    inputSize = Convert.ToInt32(array[0]);
                    outputSize = Convert.ToInt32(array[1]);
                }
                Synchronize();
            }
            return;
        }

        #endregion
    }
}
