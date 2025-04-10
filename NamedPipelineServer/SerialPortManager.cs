using System;
using System.IO.Ports;

namespace ArduinoSerialRead
{
    public class SerialPortManager
    {
        private readonly SerialPort _serialPort;

        public event EventHandler<string> DataReceived;

        public SerialPortManager(string portName, int baudRate)
        {
            _serialPort = new SerialPort(portName, baudRate);
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        public void Open()
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
            }
        }

        public void Close()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string message = _serialPort.ReadLine();
            DataReceived?.Invoke(this, message);
        }
    }
}
