using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using Neo.Lux.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Neo.Lux.Core
{
    public interface IBlockchainProvider
    {
        Transaction GetTransaction(UInt256 hash);
        Block GetBlock(UInt256 hash);
        Block GetBlock(uint height);
        uint GetBlockHeight();
    }

    public static class VMAPI {

        public static T GetInteropFromStack<T>(this ExecutionEngine engine) where T : class, IInteropInterface
        {
            if (engine.EvaluationStack.Count == 0)
            {
                return default(T);
            }

            var obj = engine.EvaluationStack.Pop() as VM.Types.InteropInterface;
            if (obj == null)
            {
                return default(T);

            }

            return obj.GetInterface<T>();
        }

        public static void RegisterAPI(IBlockchainProvider provider, InteropService target) {
            target.Register("Neo.Output.GetScriptHash", engine => { var output = GetInteropFromStack<Transaction.Output>(engine); if (output == null) return false; engine.EvaluationStack.Push(output.scriptHash.ToArray()); return true; }, InteropService.defaultGasCost);
            target.Register("Neo.Output.GetValue", engine => { var output = GetInteropFromStack<Transaction.Output>(engine); if (output == null) return false; engine.EvaluationStack.Push(output.value.ToBigInteger()); return true; }, InteropService.defaultGasCost);
            target.Register("Neo.Output.GetAssetId", engine => { var output = GetInteropFromStack<Transaction.Output>(engine); if (output == null) return false; engine.EvaluationStack.Push(output.assetID); return true; }, InteropService.defaultGasCost);

            target.Register("Neo.Transaction.GetReferences",  engine=> Transaction_GetReferences(provider, engine), 0.2m);
            target.Register("Neo.Transaction.GetOutputs", Transaction_GetOutputs, InteropService.defaultGasCost);
            target.Register("Neo.Transaction.GetInputs", Transaction_GetInputs, InteropService.defaultGasCost);
            target.Register("Neo.Transaction.GetHash", engine => { var tx = GetInteropFromStack<Transaction>(engine); if (tx == null) return false; engine.EvaluationStack.Push(tx.Hash.ToArray()); return true; }, InteropService.defaultGasCost);


            target.Register("Neo.Blockchain.GetHeight", engine => { engine.EvaluationStack.Push((new VM.Types.Integer(provider.GetBlockHeight()))); return true; }, InteropService.defaultGasCost);
            target.Register("Neo.Blockchain.GetHeader", engine => {
                Block block;
                var bytes = engine.EvaluationStack.Pop().GetByteArray();
                if (bytes.Length == 32)
                {
                    var hash = new UInt256(bytes);
                    block = provider.GetBlock(hash);
                }
                else
                {
                    var height = new BigInteger(bytes);
                    block = provider.GetBlock((uint)height);
                }
                engine.EvaluationStack.Push((new VM.Types.InteropInterface(block)));
                return true;
            }, InteropService.defaultGasCost);

            target.Register("Neo.Header.GetConsensusData", engine => { var output = GetInteropFromStack<Block>(engine); if (output == null) return false; engine.EvaluationStack.Push(output.ConsensusData); return true; }, InteropService.defaultGasCost);
        }

        private static bool Transaction_GetReferences(IBlockchainProvider provider, ExecutionEngine engine)
        {
            var tx = GetInteropFromStack<Transaction>(engine);

            if (tx == null)
            {
                return false;
            }

            var items = new List<StackItem>();

            var references = new List<Transaction.Output>();
            foreach (var input in tx.inputs)
            {
                var other_tx = provider.GetTransaction(input.prevHash);
                references.Add(other_tx.outputs[input.prevIndex]);
            }

            foreach (var reference in references)
            {
                items.Add(new VM.Types.InteropInterface(reference));
            }

            var result = new VM.Types.Array(items.ToArray<StackItem>());
            engine.EvaluationStack.Push(result);

            return true;
        }

        private static bool Transaction_GetOutputs(ExecutionEngine engine)
        {
            var tx = GetInteropFromStack<Transaction>(engine);

            if (tx == null)
            {
                return false;
            }

            var items = new List<StackItem>();

            foreach (var output in tx.outputs)
            {
                items.Add(new VM.Types.InteropInterface(output));
            }

            var result = new VM.Types.Array(items.ToArray<StackItem>());
            engine.EvaluationStack.Push(result);

            return true;
        }

        private static bool Transaction_GetInputs(ExecutionEngine engine)
        {
            var tx = GetInteropFromStack<Transaction>(engine);

            if (tx == null)
            {
                return false;
            }

            var items = new List<StackItem>();

            foreach (var input in tx.inputs)
            {
                items.Add(new VM.Types.InteropInterface(input));
            }

            var result = new VM.Types.Array(items.ToArray<StackItem>());
            engine.EvaluationStack.Push(result);

            return true;
        }


    }
}
