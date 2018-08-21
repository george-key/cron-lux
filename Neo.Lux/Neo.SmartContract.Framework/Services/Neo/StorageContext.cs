using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Neo.SmartContract.Framework.Services.Neo
{
    public class StorageContext
    {
        private static Dictionary<byte[], StorageContext> _contexts = new Dictionary<byte[], StorageContext>(new ByteArrayComparer());

        public readonly Dictionary<byte[], byte[]> Entries = new Dictionary<byte[], byte[]>();

        public static StorageContext Find(byte[] hash)
        {
            if (_contexts.ContainsKey(hash))
            {
                return _contexts[hash];
            }

            var context = new StorageContext();
            _contexts[hash] = context;
            return context;
        }

        public void Clear()
        {
            Entries.Clear();
        }

        private void Log(string s)
        {
            var temp = global::System.Console.ForegroundColor;
            global::System.Console.ForegroundColor = global::System.ConsoleColor.Yellow;
            global::System.Console.WriteLine(s);
            global::System.Console.ForegroundColor = temp;
        }

        public static bool IsASCII(byte[] key)
        {
            for (int i=0; i<key.Length; i++)
            {
                if (key[i]<32 || key[i] >= 127)
                {
                    return false;
                }
            }

            return true;
        }

        public static byte[] FromHumanKey(string key, bool forceSep = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return new byte[0];
            }

            if (key.Contains("."))
            {
                var temp = key.Split('.');
                byte[] result = new byte[0];

                foreach (var entry in temp)
                {
                    var sub = FromHumanKey(entry, true);
                    result = result.Concat(sub);
                }
            }

            if (key.IsValidAddress())
            {
                return key.AddressToScriptHash();
            }

            if (key.StartsWith("0x"))
            {
                return key.Substring(0).HexToBytes();
            }

            {
                var result = global::System.Text.Encoding.ASCII.GetBytes(key);
                if (forceSep)
                {
                    result = new byte[] { (byte)'{' }.Concat(result).Concat(new byte[] { (byte)'}' });
                }
                return result;
            }
        }

        public static string ToHumanValue(byte[] key, byte[] value)
        {
            if (key.Length == 20)
            {
                return new global::System.Numerics.BigInteger(value).ToString();
            }

            return "0x"+value.ByteToHex();
        }

        private static string DecodeKey(byte[] key)
        {
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] == (byte)'}')
                {
                    int index = i + 1;
                    var first = key.Take(index).ToArray();

                    if (IsASCII(first))
                    {
                        var name = global::System.Text.Encoding.ASCII.GetString(first);
                        if (name.StartsWith("{") && name.EndsWith("}"))
                        {
                            name = name.Substring(1, name.Length - 2);

                            if (i == key.Length - 1)
                            {
                                return name;
                            }

                            var temp = key.Skip(index).ToArray();

                            var rest = DecodeKey(temp);

                            if (rest == null)
                            {
                                return null;
                            }

                            return $"{name}.{rest}";
                        }
                    }

                    return null;                    
                }
                /*else
                if (key[0] == (byte)'<' && key[i] == (byte)'>')
                {
                    int index = i + 1;
                    var first = key.Take(index).Skip(1).ToArray();
                    var num = new BigInteger(first);
                    if (i == key.Length - 1)
                    {
                        return $"[]";
                    }
                }*/
            }

            return null;
        }

        public static string ToHumanKey(byte[] key)
        {
            if (key.Length == 20)
            {
                return new UInt160(key).ToAddress();
            }

            if (key.Length > 20)
            {
                var address = key.Take(20);
                var temp = key.Skip(20).ToArray();

                var rest = DecodeKey(temp);
                if (rest != null)
                {
                    return $"{ToHumanKey(address)}.{rest}";
                }

                if (IsASCII(key))
                {
                    return global::System.Text.Encoding.ASCII.GetString(key);
                }
            }

            return "0x" + key.ByteToHex();
        }

        public byte[] Get(byte[] key)
        {
            var value = Entries.ContainsKey(key) ? Entries[key] : new byte[0];

            Log($"GET: {ToHumanKey(key)} => {ToHumanValue(key, value)}");

            return value;
        }

        public void Put(byte[] key, byte[] value)
        {

            Log($"PUT: {ToHumanKey(key)} => {ToHumanValue(key, value)}");

            if (value == null) value = new byte[0]; Entries[key] = value;
        }

        public void Delete(byte[] key)
        {

            Log($"DELETE: {ToHumanKey(key)}");

            if (Entries.ContainsKey(key)) Entries.Remove(key);
        }

    }
}
