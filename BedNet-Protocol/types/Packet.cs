using BedNetProtocol.Enums;

namespace BedNetProtocol.Types;

public abstract class Packet {
    public abstract PacketID Id { get; }
    public abstract int Size { get; }

    public byte SenderSubClientId = 0;
    public byte TargetSubClientId = 0;

    public virtual void Read(BinaryReader reader) {
        // The first 10 value bits are the packet id, the next 2 value bits are the Sender SubClientID, and the next 2 value bits are the Target SubClientID
        var header = reader.ReadVarUInt32();
        var id = header & 0x3ff;
        if (id != (int)Id) throw new InvalidDataException($"Unexpected packet ID '0x{id:X2}' when attempting to read {this}.");

        SenderSubClientId = (byte)((header >> 10) & 3);
        TargetSubClientId = (byte)((header >> 12) & 3);
    }

    public virtual void Write(BinaryWriter writer) {
        var header = (int)Id | ((SenderSubClientId & 3) << 10) | ((TargetSubClientId & 3) << 12);
        writer.WriteVarInt((uint)header);
    }

    public static PacketID GetId(byte[] data) {
        using MemoryStream stream = new(data);
        using BinaryReader reader = new(stream);
        var header = reader.ReadVarUInt32();
        return (PacketID)(header & 0x3ff);
    }

    public static byte[] WriteBatch(Packet[] packets) {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);

        foreach (var packet in packets) {
            writer.WriteVarInt((uint)packet.Size);
            writer.Write(packet);
        }

        return stream.ToArray();
    }

    public static byte[][] ReadBatch(byte[] data) {
        using MemoryStream stream = new();
        using BinaryReader reader = new(stream);
        
        List<byte[]> packets = [];

        while (reader.BaseStream.Position < reader.BaseStream.Length) {
            var size = reader.ReadVarUInt32();
            packets.Add(reader.ReadBytes((int)size));
        }

        return [.. packets];
    }
}
