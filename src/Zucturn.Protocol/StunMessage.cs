// Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// Licensed under the MIT License.

namespace Zucturn.Protocol;

public struct StunMessage
{
    public StunMessageHeader MessageHeader { get; set; }

    public StunMessage(StunMessageHeader messageHeader)
    {
        MessageHeader = messageHeader;
    }

    public static StunMessage FromByteArray(ReadOnlySpan<byte> buffer)
    {
        if (buffer.IsEmpty)
            throw new EmptyBufferException("STUN message bufffer can't be empty");

        var header = StunMessageHeader.FromByteArray(buffer);

        return new StunMessage(header);
    }
}