﻿using Neo.Lux.Utils;
using System.IO;

namespace Neo.Lux.Core
{
    public enum ContractPropertyState : byte
    {
        NoProperty = 0,

        HasStorage = 1 << 0,
        HasDynamicInvoke = 1 << 1,
        Payable = 1 << 2
    }


    public class Contract
    {
        public byte[] script;
        public byte[] parameterList;
        public byte returnType;
        public ContractPropertyState properties;
        public string name;

        public string version;
        public string author;
        public string email;
        public string description;

        public void Serialize(BinaryWriter writer, int version)
        {
            writer.WriteVarBytes(this.script);
            writer.WriteVarBytes(this.parameterList);
            writer.Write((byte)this.returnType);
            if (version >= 1)
            {
                writer.Write((byte)properties);
            }

            writer.WriteVarString(this.name);
            writer.WriteVarString(this.version);
            writer.WriteVarString(this.author);
            writer.WriteVarString(this.email);
            writer.WriteVarString(this.description);
        }

        public static Contract Unserialize(BinaryReader reader, int version)
        {
            var reg = new Contract();
            reg.script = reader.ReadVarBytes();
            reg.parameterList = reader.ReadVarBytes();
            reg.returnType = reader.ReadByte();
            reg.properties = (version >= 1) ? ((ContractPropertyState) reader.ReadByte()) : ContractPropertyState.NoProperty;
            reg.name = reader.ReadVarString();
            reg.version = reader.ReadVarString();
            reg.author = reader.ReadVarString();
            reg.email = reader.ReadVarString();
            reg.description = reader.ReadVarString();
            return reg;
        }
    }
}
