using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoSerialRead
{
    public class NamedPipeManager : IDisposable
    {
        private readonly NamedPipeServerStream _pipeServer;

        public NamedPipeManager(string pipeName)
        {
            _pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        }

        public async Task WaitForConnectionAsync()
        {
            await _pipeServer.WaitForConnectionAsync();
        }

        public async Task WriteToPipeAsync(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await _pipeServer.WriteAsync(buffer, 0, buffer.Length);
            await _pipeServer.FlushAsync();
        }

        public void Dispose()
        {
            _pipeServer?.Dispose();
        }
    }
}
