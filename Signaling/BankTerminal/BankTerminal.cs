using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Signaling.BankTerminal
{
    public class BankTerminal
    {
        private readonly Protocol _protocol;
        private readonly AutoResetEvent _operationSignal = new AutoResetEvent(false);

        public BankTerminal(IPEndPoint endPoint)
        {
            _protocol = new Protocol(endPoint);
            _protocol.OnMessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(object sender, ProtocolMessage e)
        {
            if (e.Status == OperationStatus.Finished)
            {
                Console.WriteLine("Signaling!");
                _operationSignal.Set();
            }
        }

        public Task Purchase(decimal amount)
        {
            return Task.Run(() =>
            {
                const int purchaseOpCode = 1;
                _protocol.Send(purchaseOpCode, amount);

                //_operationSignal.Reset();
                Console.WriteLine("Waiting for signal.");
                _operationSignal.WaitOne();
            });
        }
    }
}