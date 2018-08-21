using System;
using System.Text;
using Neo.Lux.Utils;

namespace Neo.SmartContract.Framework.Services.Neo
{
    public static class Runtime
    {
        public static TriggerType Trigger => TriggerType.Application;

        public static uint Time => (uint)(DateTime.UtcNow.Ticks/1000);

        public static bool CheckWitness(byte[] hashOrPubkey) { return true; }

        public static void Notify(params object[] state) {
            var sb = new StringBuilder();
            foreach (var obj in state)
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }

                if (obj is byte[])
                {
                    sb.Append(((byte[])obj).ByteToHex());
                }
                else
                {
                    sb.Append(obj.ToString());
                }
            }
            Console.WriteLine("NOTIFY: " + sb);
        }

        public static void Log(string message) {
            Console.WriteLine("LOG: " + message);
        }

        public static Func<string, object[], object> CallHandler = null;

        public static object Call(string operation, object[] args)
        {
            var temp = System.ExecutionEngine.CallingScriptHash;
            System.ExecutionEngine.CallingScriptHash = System.ExecutionEngine.ExecutingScriptHash;
            var result = CallHandler(operation, args);
            System.ExecutionEngine.CallingScriptHash = temp;
            return result;
        }
    }
}
