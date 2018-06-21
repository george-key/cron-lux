using System.IO;

namespace Neo.Lux.Cryptography
{
    public interface ISerializable
    {
        int Size { get; }

        void Serialize(BinaryWriter writer);

        void Deserialize(BinaryReader reader);
    }
}
