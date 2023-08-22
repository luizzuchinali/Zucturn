// Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// Licensed under the MIT License.

using System.Net;
using System.Buffers.Binary;

namespace Zucturn.Protocol;

/// <summary>
/// Represents the STUN message class.
/// </summary>
public enum StunClass
{
    Request = 0b0000_0000,
    Indication = 0b0000_0100,
    SuccessResponse = 0b0000_1000,
    ErrorResponse = 0b0000_1100
}

/// <summary>
/// Represents the STUN method.
/// </summary>
public enum StunMethod
{
    Binding = 0b0000_0001
}

/// <summary>
/// Represents the STUN message header.
/// </summary>
public struct StunMessageHeader
{
    public const int MagicCookieValue = 0x2112A442;
    public const int MessageHeaderByteSize = 20;
    public static readonly int AttributeHeaderByteSize = 4;
    public const int TransactionIdByteSize = 12;

    public StunClass Class { get; set; }
    public StunMethod Method { get; set; }
    public ushort MessageLength { get; set; }
    public int MagicCookie { get; set; }
    public TransactionIdentifier TransactionId { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StunMessageHeader"/> struct.
    /// </summary>
    /// <param name="class">The STUN message class.</param>
    /// <param name="method">The STUN method.</param>
    /// <param name="messageLength">The length of the STUN message.</param>
    public StunMessageHeader(StunClass @class, StunMethod method, ushort messageLength)
    {
        Class = @class;
        Method = method;
        MessageLength = messageLength;
        MagicCookie = MagicCookieValue;
        TransactionId = TransactionIdentifier.NewIdentifier();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StunMessageHeader"/> struct.
    /// </summary>
    /// <param name="class">The STUN message class.</param>
    /// <param name="method">The STUN method.</param>
    /// <param name="messageLength">The length of the STUN message.</param>
    /// <param name="magicCookie">The STUN Magic Cookie.</param>
    /// <param name="transactionId">The STUN transaction identifier.</param>
    public StunMessageHeader(StunClass @class, StunMethod method, ushort messageLength, int magicCookie,
        TransactionIdentifier transactionId)
    {
        Class = @class;
        Method = method;
        MessageLength = messageLength;
        MagicCookie = magicCookie;
        TransactionId = transactionId;
    }

    /// <summary>
    /// Converts the <see cref="StunMessageHeader"/> to a byte array in big-endian format.
    /// </summary>
    /// <returns>A byte array representing the <see cref="StunMessageHeader"/> in big-endian format.</returns>
    public byte[] ToByteArray()
    {
        var buffer = new byte[MessageHeaderByteSize];

        buffer[0] = (byte)((byte)Class & 0b0000_1111);
        buffer[1] = (byte)((byte)Method & 0b0000_1111);

        var lengthBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)MessageLength));
        Buffer.BlockCopy(lengthBytes, 0, buffer, 2, 2);

        var magicCookieBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(MagicCookie));
        Buffer.BlockCopy(magicCookieBytes, 0, buffer, 4, 4);

        var transactionIdBytes = TransactionId.ToByteArray();
        Buffer.BlockCopy(transactionIdBytes, 0, buffer, 8, TransactionIdByteSize);

        return buffer;
    }

    /// <summary>
    /// Parses a byte array in big-endian format to construct a <see cref="StunMessageHeader"/> from its binary representation.
    /// </summary>
    /// <param name="buffer">The byte array containing the STUN message header in big-endian format.</param>
    /// <returns>A <see cref="StunMessageHeader"/> representing the parsed STUN message header.</returns>
    /// <exception cref="MalformatteHeaderException">
    /// Thrown when the provided buffer does not contain a valid STUN header or when the Magic Cookie is invalid.
    /// </exception>
    public static StunMessageHeader FromByteArray(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < MessageHeaderByteSize)
            throw new MalformatteHeaderException("The header must be 20 byte sized");

        var messageTypeByte = buffer[0];
        if ((messageTypeByte & 0b1100_0000) != 0)
            throw new MalformatteHeaderException("The first byte should have 00 as most significant bits");

        var (@class, method) = GetMessageType(buffer[..2]);
        var length = GetLength(buffer[2..4]);
        var magicCookie = GetMagicCookie(buffer[4..8]);
        if (magicCookie != MagicCookieValue)
            throw new MalformatteHeaderException("Invalid Magic Cookie");

        var transactionId = GetTransactionId(buffer[8..MessageHeaderByteSize]);

        return new StunMessageHeader(@class, method, length, magicCookie, transactionId);
    }

    /// <summary>
    /// Parses a byte array in Big Endian format and returns a <see cref="ValueTuple{StunClass, StunMethod}"/>.
    /// </summary>
    /// <param name="buffer">The byte array to parse in Big Endian format.</param>
    /// <returns>A tuple containing <see cref="StunClass"/> and <see cref="StunMethod"/>.</returns>
    /// <exception cref="InvalidDataException">Thrown when invalid data is encountered.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTuple<StunClass, StunMethod> GetMessageType(ReadOnlySpan<byte> buffer)
    {
        var @class = (buffer[0] & 0b1111) switch
        {
            0b0000_0000 => StunClass.Request,
            0b0000_0100 => StunClass.Indication,
            0b0000_1000 => StunClass.SuccessResponse,
            0b0000_1100 => StunClass.ErrorResponse,
            _ => throw new InvalidDataException("Invalid class specified")
        };

        var method = (buffer[1] & 0b1111_1111) switch
        {
            0b0000_0001 => StunMethod.Binding,
            _ => throw new InvalidDataException("Invalid method specified")
        };

        return (@class, method);
    }

    /// <summary>
    /// Retrieves the length value from a buffer in Big Endian format.
    /// </summary>
    /// <param name="buffer">The buffer containing the length value in Big Endian format.</param>
    /// <returns>The length value as a ushort.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort GetLength(ReadOnlySpan<byte> buffer)
    {
        return BinaryPrimitives.ReadUInt16BigEndian(buffer);
    }

    /// <summary>
    /// Retrieves the magic cookie value from a buffer in Big Endian format.
    /// </summary>
    /// <param name="buffer">The buffer containing the magic cookie value in Big Endian format.</param>
    /// <returns>The magic cookie value as a uint.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetMagicCookie(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < sizeof(uint))
            throw new ArgumentException("Buffer is too short to retrieve the magic cookie.");

        return BinaryPrimitives.ReadInt32BigEndian(buffer);
    }

    /// <summary>
    /// Parses a byte array in big-endian format to construct a <see cref="TransactionIdentifier"/> from its binary representation.
    /// </summary>
    /// <param name="buffer">The byte array containing the transaction ID in big-endian format.</param>
    /// <returns>A <see cref="TransactionIdentifier"/> representing the parsed transaction ID.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TransactionIdentifier GetTransactionId(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length != TransactionIdByteSize)
            throw new MalformatteHeaderException("Invalid transaction ID size");

        return new TransactionIdentifier(new Memory<byte>(buffer.ToArray()));
    }
}