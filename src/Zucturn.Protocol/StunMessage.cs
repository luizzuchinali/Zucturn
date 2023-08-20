namespace Zucturn.Protocol;

public struct StunMessage
{
    public StunHeader Header { get; set; }

    public StunMessage(StunHeader header)
    {
        Header = header;
    }

    public ReadOnlySpan<byte> ToByteArray()
    {
        return new byte[]
        {
            0x00
        };
    }

    public static StunMessage FromByteArray(ReadOnlySpan<byte> buffer)
    {
        throw new NotImplementedException();
    }
}