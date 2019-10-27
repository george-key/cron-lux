namespace Cron.SmartContract.Framework.Services.Cron
{
    public class TransactionOutput : IApiInterface
    {
        public byte[] AssetId
        {
            get;
        }

        public long Value
        {
            get;
        }

        public byte[] ScriptHash
        {
            get;
        }
    }
}
