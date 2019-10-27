namespace Cron.SmartContract.Framework.Services.Cron
{
    public class TransactionAttribute : IApiInterface
    {
        public byte Usage
        {
            get;
        }

        public byte[] Data
        {
            get;
        }
    }
}
