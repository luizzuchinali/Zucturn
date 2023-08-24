// Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// Licensed under the MIT License.

using System.Net;

namespace Zucturn.Protocol;

/// <summary>
/// Represents a STUN (Session Traversal Utilities for NAT) message.
/// </summary>
public struct StunMessage
{
    public StunMessageHeader MessageHeader { get; set; }
    public IDictionary<EStunAttribute, ValueTuple<ushort, byte[]>> Attributes { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StunMessage"/> struct with the specified message header and attributes.
    /// </summary>
    /// <param name="messageHeader">The message header for the STUN message.</param>
    /// <param name="attributes">
    /// A dictionary containing the STUN attributes associated with the message.
    /// The dictionary keys are byte arrays representing attribute types, and the values are tuples
    /// containing the attribute length as a ushort and the attribute value as a byte array.
    /// </param>
    public StunMessage(StunMessageHeader messageHeader,
        IDictionary<EStunAttribute, ValueTuple<ushort, byte[]>> attributes)
    {
        MessageHeader = messageHeader;
        Attributes = attributes;
    }

    /// <summary>
    /// Converts the <see cref="StunMessage"/> to a byte array in big-endian format.
    /// </summary>
    /// <returns>A byte array representing the <see cref="StunMessage"/> in big-endian format.</returns>
    public byte[] ToByteArray()
    {
        var headerBytes = MessageHeader.ToByteArray();

        return headerBytes;
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
        var attributes = new Dictionary<EStunAttribute, ValueTuple<ushort, byte[]>>();
        for (var x = StunMessageHeader.MessageHeaderByteSize; x < header.MessageLength; x++)
        {
            if ((buffer[x] & 0b1100_0000) != 0)
                throw new MalformattedAttributeException("Attributes should has 00 as most significant bits");

            var attribute = GetAttribute(buffer[x..(x + 2)]);
            x += 2;
            var length = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToUInt16(buffer[x..(x + 2)]));
            x += 2;

            var value = new ValueTuple<ushort, byte[]>(length, buffer[x..(x + length)].ToArray());

            attributes.Add(attribute, value);
        }

        return new StunMessage(header, attributes);
    }

    public static EStunAttribute GetAttribute(ReadOnlySpan<byte> byteArray)
    {
        if (byteArray.Length != 2)
            throw new ArgumentException("The input byte array must have a length of 2 bytes.");

        var value = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToUInt16(byteArray));

        if (!Enum.IsDefined(typeof(EStunAttribute), value))
            throw new ArgumentException(
                $"No matching attribute found for the value {value:X4} in the EStunAttribute enum.");


        return (EStunAttribute)value;
    }
}