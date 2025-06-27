using BedNetProtocol.Enums;
using BedNetProtocol.Types;
using System.Diagnostics.CodeAnalysis;

namespace BedNetProtocol.Packets;

public class RequestNetworkSettings() : Packet {
    public override PacketID Id => PacketID.RequestNetworkSettings;
    public override int Size => 0;

    
    public required int ProtocolVersion;

    [SetsRequiredMembers]
    public RequestNetworkSettings(byte[] data) : this() {
        using MemoryStream stream = new(data);
        using BinaryReader reader = new(stream);
        Read(reader);
    }

    public override void Read(BinaryReader reader) {
        base.Read(reader);
        ProtocolVersion = reader.ReadInt32BE();
    }

    public override void Write(BinaryWriter writer) {
        base.Write(writer);
        writer.WriteBE(ProtocolVersion);
    }
}
