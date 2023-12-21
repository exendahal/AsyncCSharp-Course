namespace Signaling.BankTerminal
{
    public class ProtocolMessage
    {
        public OperationStatus Status { get; }

        public ProtocolMessage(OperationStatus status)
        {
            this.Status = status;
        }
    }
}