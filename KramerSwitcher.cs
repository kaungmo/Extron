using System;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Text;

namespace Kramer
{
    class KramerSwitcher
    {
        private SerialPort serialPort = null;

        #region Constructors

        ~KramerSwitcher()
        {
            if (serialPort != null && serialPort.IsOpen)
                serialPort.Close();
        }

        public KramerSwitcher()
        {
            serialPort = new SerialPort();
        }

        public KramerSwitcher(string portName)
        {
            serialPort = new SerialPort(portName);
        }

        public KramerSwitcher(string portName, int baudRate)
        {
            serialPort = new SerialPort(portName, baudRate);
        }

        public KramerSwitcher(string portName, int baudRate, Parity parity)
        {
            serialPort = new SerialPort(portName, baudRate, parity);
        }

        public KramerSwitcher(string portName, int baudRate, Parity parity, int dataBits)
        {
            serialPort = new SerialPort(portName, baudRate, parity, dataBits);
        }

        public KramerSwitcher(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
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

        #region Switcher Functions

        public int Tie(int input, int output)
        {
            try
            {
                // Command Format 
                // 0x01 IN OUT 0x81
                byte[] data = new byte[] { 0x01, 0x80, 0x80, 0x81 };
                data[1] = Convert.ToByte(0x80 | input);
                data[2] = Convert.ToByte(0x80 | output);

                serialPort.Write(data, 0, data.Length);
                serialPort.BaseStream.Flush();
                Thread.Sleep(10);

                byte[] retdata = new byte[] { 0, 0, 0, 0 };
                serialPort.Read(retdata, 0, 4);
                if (retdata[0] == 0x41 &&
                    data[1] == retdata[1] &&
                    data[2] == retdata[2] &&
                    data[3] == retdata[3])
                    return 0;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
            return -1;
        }

        #endregion
    }
}
