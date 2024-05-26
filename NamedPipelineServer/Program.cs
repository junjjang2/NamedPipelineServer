using System;
using System.IO.Ports;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace ArduinoSerialRead
{
    class Program
    {
        private static SerialPort serialPort;
        private static NamedPipeServerStream pipeServer1;
        private static NamedPipeServerStream pipeServer2;

        static async Task Main(string[] args)
        {
            string portName = "COM4";  // 사용 중인 시리얼 포트 이름
            int baudRate = 9600;       // 아두이노와 일치하는 보드 레이트 설정

            // 두 개의 NamedPipeServerStream 생성
            pipeServer1 = new NamedPipeServerStream("pipe1", PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            pipeServer2 = new NamedPipeServerStream("pipe2", PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

            // 파이프 서버 비동기 연결 대기
            var pipeTask1 = WaitForConnectionAsync(pipeServer1);
            var pipeTask2 = WaitForConnectionAsync(pipeServer2);

            try
            {
                serialPort = new SerialPort(portName, baudRate)
                {
                    NewLine = "\n",
                    ReadTimeout = 1000
                };
                serialPort.DataReceived += SerialPort_DataReceived;
                serialPort.Open();  // 시리얼 포트 열기

                Console.WriteLine("Reading from Arduino... Press any key to exit.");
                await Task.WhenAll(pipeTask1, pipeTask2);  // 파이프 연결 대기
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                serialPort?.Close();
                pipeServer1?.Dispose();
                pipeServer2?.Dispose();
            }
        }

        private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string message = serialPort.ReadLine();
                if (ProcessMessage(message, out string part1, out string part2))
                {
                    // 유효한 메시지를 파이프에 전송
                    _ = WriteToPipeAsync(pipeServer1, part1);
                    _ = WriteToPipeAsync(pipeServer2, part2);
                }
            }
            catch (TimeoutException) { }
        }

        private static async Task WaitForConnectionAsync(NamedPipeServerStream pipeServer)
        {
            await Task.Factory.FromAsync(pipeServer.BeginWaitForConnection, pipeServer.EndWaitForConnection, null);
        }

        private static async Task WriteToPipeAsync(NamedPipeServerStream pipe, string message)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);

            if (pipe.IsConnected)
            {
                await pipe.WriteAsync(buffer, 0, buffer.Length);
                await pipe.FlushAsync();
            }
        }

        private static bool ProcessMessage(string message, out string part1, out string part2)
        {
            string[] parts = message.Split(',');

            // 숫자 4개가 아닌 경우 무시
            if (parts.Length != 4)
            {
                part1 = part2 = null;
                return false;
            }

            // 각 부분이 숫자인지 확인
            foreach (string part in parts)
            {
                if (!int.TryParse(part, out _))
                {
                    part1 = part2 = null;
                    return false;
                }
            }

            // 첫 두 숫자와 나머지 두 숫자 분리
            part1 = $"{parts[0]},{parts[1]}";
            part2 = $"{parts[2]},{parts[3]}";

            Console.WriteLine($"Valid message received: {message}");
            return true;
        }
    }
}
