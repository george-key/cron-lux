﻿namespace Cron.SmartContract.Framework.Services.Cron
{
    public class Block : Header
    {
        public int GetTransactionCount() { return 0; }

        public Transaction[] GetTransactions() { return null; }

        public Transaction GetTransaction(int index) { return null; }
    }
}
