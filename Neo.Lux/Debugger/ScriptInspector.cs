using Neo.Lux.Cryptography;
using Neo.Lux.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Lux.Debugger
{
    public class ScriptCall
    {
        public UInt160 contractHash;
        public string operation;
        public List<object> arguments = new List<object>();
    }

    public class ScriptInspector
    {
        private List<ScriptCall> _calls = new List<ScriptCall>();
        public IEnumerable<ScriptCall> Calls => _calls;

        public ScriptInspector(byte[] script) : this(script, x=>true)
        {

        }

        public ScriptInspector(byte[] script, UInt160 targetContract) : this (script, x => x.Equals(targetContract))
        {

        }

        public ScriptInspector(byte[] script, Func<UInt160, bool> filter)
        {
            var instructions = NeoTools.Disassemble(script);

            for (int i = 1; i < instructions.Count; i++)
            {
                var op = instructions[i];

                // opcode data must contain the script hash to the Bluzelle contract, otherwise ignore it
                if (op.opcode == OpCode.APPCALL && op.data != null && op.data.Length == 20)
                {
                    var scriptHash = new UInt160(op.data);

                    if (filter != null && !filter(scriptHash))
                    {
                        continue;
                    }

                    var call = new ScriptCall();
                    call.contractHash = scriptHash;
                    call.operation = Encoding.ASCII.GetString(instructions[i - 1].data);


                    int index = i - 3;
                    var argCount = 1 + ((byte)instructions[index].opcode - (byte)OpCode.PUSH1);

                    while (argCount > 0)
                    {
                        index--;
                        if (instructions[index].opcode >= OpCode.PUSHBYTES1 && instructions[index].opcode <= OpCode.PUSHBYTES75)
                        {
                            call.arguments.Add(instructions[index].data);
                        }
                        else
                        if (instructions[index].opcode >= OpCode.PUSH1 && instructions[index].opcode <= OpCode.PUSH16)
                        {
                            var n = new BigInteger(1 + (instructions[index].opcode - OpCode.PUSH1));
                            call.arguments.Add(n);
                        }
                        else
                        if (instructions[index].opcode == OpCode.PUSH0)
                        {
                            call.arguments.Add(new BigInteger(0));
                        }
                        else
                        if (instructions[index].opcode == OpCode.PUSHM1)
                        {
                            call.arguments.Add(new BigInteger(-1));
                        }
                        else
                        {
                            throw new Exception("Invalid arg type");
                        }

                        argCount--;
                    }
                }
            }
        }
    }
}
