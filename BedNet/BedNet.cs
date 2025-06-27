using BedNetProtocol.Enums;
using BedNetProtocol.Types;
using BedNetProtocol.Packets;
using System.Net;
using BedNetProtocol;
using RakNetAgain.Types;
using System.IO.Compression;

namespace BedNet;

class ServerConnection {
    private RakNetAgain.RakServerConnection _rakConnection;
    private Server _server;
    private bool compressPackets = false;

    public int ProtocolVersion { get; init; }

    public ServerConnection(RakNetAgain.RakServerConnection rakConn, Server server) {
        _rakConnection = rakConn;
        _server = server;

        _rakConnection.OnTick += Tick;
        _rakConnection.OnGamePacket += HandlePacketBatch;
    }

    // public void SendPacketBatch(Packet[] packets, RakNetAgain.Types.Frame.FramePriority priority = RakNetAgain.Types.Frame.FramePriority.Normal) =>
    //     _rakConnection.SendGamePacket(Packet.WriteBatch(packets), priority);

    public void SendPacketBatch(Packet[] packets, Frame.FramePriority priority = Frame.FramePriority.Normal) {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);

        if (compressPackets) {
            // TODO
        } else {
            writer.WritePacketBatch(packets);
        }

        _rakConnection.SendGamePacket(stream.ToArray(), priority);
    }

    public void Tick() { }

    public void HandlePacketBatch(byte[] data) {
        var packets = Packet.ReadBatch(data);
        foreach (byte[] pak in packets) HandlePacket(pak);
    }

    public void HandlePacket(byte[] data) {
        switch (Packet.GetId(data)) {
            case PacketID.RequestNetworkSettings:
                HandleRequestNetworkSettings(new(data));
                break;
            default:
                break;
        }
    }

    private void HandleRequestNetworkSettings(RequestNetworkSettings packet) {
        if (packet.ProtocolVersion != ProtocolVersion) {
            // TODO: Disconnect packet
            _rakConnection.Disconnect();
        }

        NetworkSettings reply = new() {
            CompressionThreshold = 0,
            CompressionAlgorithm = CompressionAlgorithm.Deflate,
            ClientThrottleEnabled = false,
            ClientThrottleThreshold = 0,
            ClientThrottleScalar = 0,
        };

        SendPacketBatch([reply], Frame.FramePriority.Immediate);

        // Compress packets from now on
        compressPackets = true;
    }
}

class Server {
    private readonly RakNetAgain.RakServer rak;

    public required string GameVersion { get; init; }
    public required ushort Port { get; init; }
    public short CompressionThreshold { get; init; } = 512;

    public readonly Dictionary<IPEndPoint, ServerConnection> Connections = [];

    public Server() {
        rak = new(Port);
        rak.OnDiscovery += Discovery;
        rak.OnConnect += NewConnection;
    }

    private string Discovery(IPEndPoint endpoint) {
        ServerMessage reply = new() {
            GameVersion = GameVersion,
            Port = Port,
            PortV6 = (ushort)(Port + 1), // TODO: ?
            ProtocolVersion = 818, // TODO: get from GameVersion
            ServerGuid = rak.Guid,
        };

        return reply.ToString();
    }

    private void NewConnection(RakNetAgain.RakServerConnection rakConn) {
        ServerConnection connection = new(rakConn, server: this) {
            ProtocolVersion = 818, // TODO
        };

        Connections[rakConn.Endpoint] = connection;
        rakConn.OnDisconnect += () => Connections.Remove(rakConn.Endpoint);
    }

    private CancellationTokenSource? _cts;
    public async Task Start(CancellationToken? cancellationToken = null) {
        _cts = new();
        var token = cancellationToken ?? _cts.Token;
        await rak.Start(token);
    }

    public void Stop() {
        _cts?.Cancel();
    }
}
