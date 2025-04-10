using System;
using System.Threading.Tasks;

namespace ArduinoSerialRead
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: NamedPipelineServer <portName> <baudRate>");
                return;
            }

            string portName = args[0];
            if (!int.TryParse(args[1], out int baudRate))
            {
                Console.WriteLine("Invalid baud rate.");
                return;
            }

            var serialPortManager = new SerialPortManager(portName, baudRate);
            var namedPipeManager1 = new NamedPipeManager("pipe1");
            var namedPipeManager2 = new NamedPipeManager("pipe2");
            var messageProcessor = new MessageProcessor();

            serialPortManager.DataReceived += async (sender, message) =>
            {
                if (messageProcessor.ProcessMessage(message, out string part1, out string part2))
                {
                    await namedPipeManager1.WriteToPipeAsync(part1);
                    await namedPipeManager2.WriteToPipeAsync(part2);
                }
            };

            try
            {
                await Task.WhenAll(namedPipeManager1.WaitForConnectionAsync(), namedPipeManager2.WaitForConnectionAsync());
                serialPortManager.Open();

                Console.WriteLine("Reading from Arduino... Press any key to exit.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                serialPortManager.Close();
                namedPipeManager1.Dispose();
                namedPipeManager2.Dispose();
            }
        }
    }
}
