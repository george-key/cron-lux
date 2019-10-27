namespace Cron.SmartContract.Framework.Services.Cron
{
    public class Header : IScriptContainer
    {
        public byte[] Hash;

        public uint Version;

        public byte[] PrevHash;

        public byte[] MerkleRoot;

        public uint Timestamp;

        public uint Index;

        public ulong ConsensusData;

        public byte[] NextConsensus;
    }
}
