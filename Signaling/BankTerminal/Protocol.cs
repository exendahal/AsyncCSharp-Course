using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Signaling.BankTerminal
{
    public class Protocol
    {
        private readonly IPEndPoint _endPoint;

        public Protocol(IPEndPoint endPoint)
        {
            _endPoint = endPoint;
        }

        public void Send(int opCode, object parameters)
        {
            Task.Run(() =>
            {
                //emulating interoperatinon with a bank terminal device
                Console.WriteLine("Operation is in action.");
                Thread.Sleep(3000);

                OnMessageReceived?.Invoke(this, new ProtocolMessage(OperationStatus.Finished));

            });
        }

        public event EventHandler<ProtocolMessage> OnMessageReceived;
    }
}