// Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// Licensed under the MIT License.

namespace Zucturn.Protocol;

public struct StunHeader
{
    public const int MagicCookie = 0x2112A442;
    public const int MessageHeaderByteSize = 20;
    public const int AttributeHeaderByteSize = 4;
    public const int TransactionIdByteSize = 12;

    public uint Magic { get; set; }

    public StunHeader(uint magic)
    {
        Magic = magic;
    }

    public ReadOnlySpan<byte> ToByteArray()
    {
        return new byte[]
        {
            0x00
        };
    }

    public static StunHeader FromByteArray(ReadOnlySpan<byte> buffer)
    {
        throw new NotImplementedException();
    }
}

[Flags]
public enum EStunClass
{
    Request = 0x00,
    Indication = 0x01,
    SuccessResponse = 0x10,
    ErrorResponse = 0x11
}