/*
 * Title: AddressListener
 * Author: Sergio Flores
 * Description: This demo shows how to listen for incoming transactions on a specific address on test net.
 * This can be used for example for implementing a payment system.
 */

using Cron.Lux.Core;
using Cron.Lux.Cryptography;
using Cron.Lux.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NeoBlocks
{
    class Program
    {
        static void Main(string[] args)
        {
            string address;

            do
            {
                Console.WriteLine("Write an CRON address to listen:");
                address = Console.ReadLine();
            } while (!address.IsValidAddress());

            int  cronPort = 0;
            string  cronURL = null;


            var api = new RemoteRPCNode(cronPort, cronURL, CronNodesKind.CRON_GLOBAL);

            var oldBlockCount = api.GetBlockHeight();

            var targetScriptHash = new UInt160(address.AddressToScriptHash());

            Console.WriteLine("Now listening for transactions...");

            do
            {
                // wait for block generation
                Thread.Sleep(10000);

                var newBlockCount = api.GetBlockHeight();

                if (newBlockCount != oldBlockCount)
                {
                    Console.WriteLine($"Fetching block {newBlockCount}");

                    // retrieve latest block
                    var block = api.GetBlock(newBlockCount);

                    if (block == null)
                    {
                        Console.WriteLine($"Failed...");
                        continue;
                    }

                    oldBlockCount = newBlockCount;

                    // inspect each tx in the block for inputs sent to the target address
                    foreach (var tx in block.transactions)
                    {
                        var amounts = new Dictionary<string, decimal>();

                        foreach (var output in tx.outputs)
                        {
                            if (output.scriptHash.Equals(targetScriptHash))
                            {
                                var asset = CronAPI.SymbolFromAssetID(output.assetID);

                                if (amounts.ContainsKey(asset))
                                {
                                    amounts[asset] += output.value;
                                }
                                else
                                {
                                    amounts[asset] = output.value;
                                }
                                
                            }
                        }

                        if (amounts.Count > 0)
                        {
                            foreach (var entry in amounts)
                            {
                                Console.WriteLine($"{entry.Value} {entry.Key} was sent to {address}, in tx {tx.Hash}");
                            }
                        }
                    }
                }

            } while (true);
        }
    }
}
