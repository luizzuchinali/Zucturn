// Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// Licensed under the MIT License.

namespace Zucturn.Protocol;

public enum StunClass
{
    Request = 0b0000,
    Indication = 0b0100,
    SuccessResponse = 0b1000,
    ErrorResponse = 0b1100
}

public enum StunMethod
{
    Binding = 0b000000000001
}

public struct StunMessageHeader
{
    public const int MagicCookie = 0x2112A442;
    public const int MessageHeaderByteSize = 20;
    public const int AttributeHeaderByteSize = 4;
    public const int TransactionIdByteSize = 12;

    public StunClass Class { get; set; }
    public StunMethod Method { get; set; }
    public uint Magic { get; set; }

    public StunMessageHeader(StunClass @class, StunMethod method, uint magic)
    {
        Class = @class;
        Method = method;
        Magic = magic;
    }

    public static StunMessageHeader FromByteArray(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < MessageHeaderByteSize)
            throw new MalformatteHeaderException("The header must be 20 byte sized");

        var messageTypeByte = buffer[0];
        if ((messageTypeByte & 0b1100_0000) != 0)
            throw new MalformatteHeaderException("The first byte should have 00 as most significant bits");

        return new StunMessageHeader(StunClass.Request, StunMethod.Binding, MagicCookie);
    }
}