using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using LunarLabs.Parser;
using LunarLabs.Parser.JSON;
using Neo.Lux.Core;
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using Neo.Lux.VM;

namespace Neo.Lux.Emulator
{
    public class RemoteEmulator : NeoAPI
    {
        private string host;
        private int port;

        private Action<string> logger;

        public RemoteEmulator(string host, int port, Action<string> logger)
        {
            this.host = host;
            this.port = port;
            this.logger = logger;
        }

        private DataNode DoRequest(string method, string arg)
        {
            try
            {
                logger($"Request: {method}/{arg}");
                var client = new TcpClient(host, port);
                client.NoDelay = true;

                var msg = Encoding.UTF8.GetBytes($"{method}/{arg}\r\n");

                NetworkStream stream = client.GetStream();

                stream.Write(msg, 0, msg.Length);

                var data = new byte[1024*64];

                var bytes = stream.Read(data, 0, data.Length);

                var responseData = Encoding.UTF8.GetString(data, 0, bytes);

                stream.Close();
                client.Close();

                var root = JSONReader.ReadFromString(responseData);
                return root["response"];
            }
            catch (Exception e)
            {
                Logger("exception: " + e);
                return null;
            }
        }

        public override Dictionary<string, decimal> GetAssetBalancesOf(UInt160 hash)
        {
            var response = DoRequest("GetAssetBalancesOf", hash.ToAddress());
            var result = new Dictionary<string, decimal>();
            foreach (var node in response.Children)
            {
                result[node.GetString("symbol")] = node.GetDecimal("value");
            }
            return result;
        }

        public override Block GetBlock(UInt256 hash)
        {
            var response = DoRequest("GetBlockByHash", hash.ToString());
            var bytes = response.GetString("rawblock").HexToBytes();
            return Block.Unserialize(bytes);
        }

        public override Block GetBlock(uint height)
        {
            var response = DoRequest("GetBlockByHeight", height.ToString());
            var bytes = response.GetString("rawblock").HexToBytes();
            return Block.Unserialize(bytes);
        }

        public override uint GetBlockHeight()
        {
            var response = DoRequest("GetChainHeight", "");
            return response.GetUInt32("height");
        }

        public override List<UnspentEntry> GetClaimable(UInt160 hash, out decimal amount)
        {
            var response = DoRequest("GetClaimable", hash.ToAddress());
            var result = new List<UnspentEntry>();
            amount = 0;
            foreach (var node in response.Children)
            {
                var entry = new UnspentEntry()
                {
                    hash = new UInt256(node.GetString("hash").HexToBytes()),
                    index = node.GetUInt32("index"),
                    value = node.GetDecimal("value")
                };
                amount += entry.value;
                result.Add(entry);
            }
            return result;
        }

        public override byte[] GetStorage(string scriptHash, byte[] key)
        {
            var response = DoRequest("GetStorage", key.ByteToHex());
            return response.GetString("value").HexToBytes();
        }

        public override Transaction GetTransaction(UInt256 hash)
        {
            var response = DoRequest("GetTransaction", hash.ToString());
            var bytes = response.GetString("rawtx").HexToBytes();
            return Transaction.Unserialize(bytes);
        }

        public override Dictionary<string, List<UnspentEntry>> GetUnspent(UInt160 hash)
        {
            var response = DoRequest("GetUnspent", hash.ToAddress());
            var result = new Dictionary<string, List<UnspentEntry>>();

            foreach (var node in response.Children)
            {
                var asset = node.GetString("asset");
                var list = new List<UnspentEntry>();
                foreach (var child in node.Children)
                {
                    var entry = new UnspentEntry()
                    {
                        hash = new UInt256(child.GetString("hash").HexToBytes()),
                        index = child.GetUInt32("index"),
                        value = child.GetDecimal("value")
                    };
                    list.Add(entry);
                }

                result[asset] = list;
            }
            return result;
        }

        public override InvokeResult InvokeScript(byte[] script)
        {
            var response = DoRequest("InvokeScript", script.ByteToHex());

            var bytes = response.GetString("stack").HexToBytes();
            using (var stream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    var result = new InvokeResult()
                    {
                        result = Serialization.DeserializeStackItem(reader),
                        state = (VMState)Enum.Parse(typeof(VMState), response.GetString("state"), true),
                        gasSpent = response.GetDecimal("gas"),
                    };

                    return result;
                }
            }
        }

        public UInt160 GetContractHash(string name)
        {
            var response = DoRequest("GetContractHash", name);
            var address = response.GetString("address");
            return new UInt160(address.AddressToScriptHash());
        }

        protected override bool SendTransaction(Transaction tx)
        {
            var hextx = tx.Serialize(true).ByteToHex();
            var response = DoRequest("SendTransaction", hextx);
            return response.GetBool("result");
        }

        protected override string GetRPCEndpoint()
        {
            throw new NotImplementedException();
        }
    }
}
