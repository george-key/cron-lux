namespace Cron.SmartContract.Framework.Services.Cron
{
    public class TransactionInput : IApiInterface
    {
        public byte[] PrevHash
        {
            get;
        }

        public ushort PrevIndex
        {
            get;
        }
    }
}
