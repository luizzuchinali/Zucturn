// Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// Licensed under the MIT License.

namespace Zucturn.Protocol;

/// <summary>
/// Represents a STUN (Session Traversal Utilities for NAT) message.
/// </summary>
public struct StunMessage
{
    public StunMessageHeader MessageHeader { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StunMessage"/> struct with the specified message header.
    /// </summary>
    /// <param name="messageHeader">The message header for the STUN message.</param>
    public StunMessage(StunMessageHeader messageHeader)
    {
        MessageHeader = messageHeader;
    }

    /// <summary>
    /// Parses a byte array to construct a <see cref="StunMessage"/> from its binary representation.
    /// </summary>
    /// <param name="buffer">The byte array containing the binary representation of the STUN message.</param>
    /// <returns>A <see cref="StunMessage"/> representing the parsed STUN message.</returns>
    /// <exception cref="EmptyBufferException">Thrown when the provided buffer is empty.</exception>
    public static StunMessage FromByteArray(ReadOnlySpan<byte> buffer)
    {
        if (buffer.IsEmpty)
            throw new EmptyBufferException("STUN message buffer can't be empty");

        var header = StunMessageHeader.FromByteArray(buffer);

        return new StunMessage(header);
    }
}