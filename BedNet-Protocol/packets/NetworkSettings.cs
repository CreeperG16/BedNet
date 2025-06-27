using BedNetProtocol.Types;
using System.Diagnostics.CodeAnalysis;

namespace BedNetProtocol.Packets;

public class NetworkSettings() : Packet {
    public override Enums.PacketID Id => Enums.PacketID.NetworkSettings;
    public override int Size => 0;

    // Determines the smallest size of raw network payload to compress.
    // NOTE: 0 is disable compression, 1 is compress everything 1 byte or larger (so everything)
    public required ushort CompressionThreshold;
    public required Enums.CompressionAlgorithm CompressionAlgorithm; // TODO: enum
    public required bool ClientThrottleEnabled;
    public required byte ClientThrottleThreshold;
    public required float ClientThrottleScalar;

    [SetsRequiredMembers]
    public NetworkSettings(byte[] data) : this() {
        using MemoryStream stream = new(data);
        using BinaryReader reader = new(stream);
        Read(reader);
    }

    public override void Read(BinaryReader reader) {
        base.Read(reader);
        CompressionThreshold = reader.ReadUInt16();
        CompressionAlgorithm = (Enums.CompressionAlgorithm)reader.ReadUInt16();
        ClientThrottleEnabled = reader.ReadBoolean();
        ClientThrottleThreshold = reader.ReadByte();
        ClientThrottleScalar = reader.ReadSingle();
    }

    public override void Write(BinaryWriter writer) {
        base.Write(writer);
        writer.Write(CompressionThreshold);
        writer.Write((ushort)CompressionAlgorithm);
        writer.Write(ClientThrottleEnabled);
        writer.Write(ClientThrottleThreshold);
        writer.Write(ClientThrottleScalar);
    }
}
