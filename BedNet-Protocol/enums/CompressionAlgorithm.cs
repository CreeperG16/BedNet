namespace BedNetProtocol.Enums;

public enum CompressionAlgorithm : ushort {
    Deflate = 0,
    Snappy = 1,
    None = 255,
}
