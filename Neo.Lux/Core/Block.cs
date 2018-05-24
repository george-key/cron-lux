﻿using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using System;
using System.IO;

namespace Neo.Lux.Core
{
    public class Block
    {
        public uint Version;
        public uint Height;
        public byte[] MerkleRoot;
        public UInt256 PreviousHash;
        public DateTime Timestamp;
        public long ConsensusData;
        public UInt160 Validator;
        public Transaction[] transactions;
        public Witness witness;

        public override string ToString()
        {
            return Hash.ToString();
        }

        private UInt256 _hash;
        public UInt256 Hash
        {
            get
            {
                if (_hash == null)
                {
                    var data = this.Serialize();
                    _hash = new UInt256(CryptoUtils.Hash256(data));
                }

                return _hash;
            }
        }

        public static Block Unserialize(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    return Unserialize(reader);
                }
            }
        }

        public byte[] Serialize()
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(Version);
                    writer.Write(PreviousHash != null ? PreviousHash.ToArray() : new byte[32]);
                    writer.Write(MerkleRoot != null ? MerkleRoot : new byte[32]);
                    writer.Write((uint)Timestamp.ToTimestamp());
                    writer.Write(Height);
                    writer.Write(ConsensusData);
                    writer.Write(Validator!=null ? Validator.ToArray(): new byte[20]);
                    return stream.ToArray();
                }
            }
        }

        public static Block Unserialize(BinaryReader reader)
        {
            var block = new Block();

            block.Version = reader.ReadUInt32();
            block.PreviousHash = new UInt256(reader.ReadBytes(32));
            block.MerkleRoot = reader.ReadBytes(32);
            block.Timestamp = reader.ReadUInt32().ToDateTime();
            block.Height = reader.ReadUInt32();
            block.ConsensusData = reader.ReadInt64();

            var nextConsensus = reader.ReadBytes(20);
            block.Validator = new UInt160(nextConsensus);

            var pad = reader.ReadByte(); // should be 1
            block.witness = Witness.Unserialize(reader);

            var txCount = (int)reader.ReadVarInt();
            block.transactions = new Transaction[txCount];
            for (int i = 0; i < txCount; i++)
            {
                block.transactions[i] = Transaction.Unserialize(reader);
            }

            var lastPos = reader.BaseStream.Position;

            return block;
        }
    }
}
