// Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// Licensed under the MIT License.

using System.Net;
using System.Buffers.Binary;

namespace Zucturn.Protocol;

/// <summary>
/// Represents the STUN message class.
/// </summary>
public enum EStunClass
{
    Request = 0b0000_0000,
    Indication = 0b0000_0100,
    SuccessResponse = 0b0000_1000,
    ErrorResponse = 0b0000_1100
}

/// <summary>
/// Represents the STUN method.
/// </summary>
public enum EStunMethod
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

    public EStunClass Class { get; set; }
    public EStunMethod Method { get; set; }
    public ushort MessageLength { get; set; }
    public int MagicCookie { get; set; }
    public TransactionIdentifier TransactionId { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StunMessageHeader"/> struct.
    /// </summary>
    /// <param name="class">The STUN message class.</param>
    /// <param name="method">The STUN method.</param>
    /// <param name="messageLength">The length of the STUN message.</param>
    public StunMessageHeader(EStunClass @class, EStunMethod method, ushort messageLength)
    {
        Class = @class;
        Method = method;
        MessageLength = messageLength;
        MagicCookie = MagicCookieValue;
        TransactionId = new TransactionIdentifier();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StunMessageHeader"/> struct.
    /// </summary>
    /// <param name="class">The STUN message class.</param>
    /// <param name="method">The STUN method.</param>
    /// <param name="messageLength">The length of the STUN message.</param>
    /// <param name="magicCookie">The STUN Magic Cookie.</param>
    /// <param name="transactionId">The STUN transaction identifier.</param>
    public StunMessageHeader(EStunClass @class, EStunMethod method, ushort messageLength, int magicCookie,
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

        if (TransactionId.IsRfc3849)
        {
            Buffer.BlockCopy(TransactionId.ToByteArray(), 0, buffer, 4, TransactionIdentifier.Rfc3849Size);
            return buffer;
        }

        var magicCookieBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(MagicCookie));
        Buffer.BlockCopy(magicCookieBytes, 0, buffer, 4, 4);

        var transactionIdBytes = TransactionId.ToByteArray();
        Buffer.BlockCopy(transactionIdBytes, 0, buffer, 8, TransactionIdentifier.Size);

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

        // Verify if is a RFC 3489 message
        var transactionId = GetTransactionId(magicCookie != MagicCookieValue
            ? buffer[4..MessageHeaderByteSize]
            : buffer[8..MessageHeaderByteSize]
        );

        return new StunMessageHeader(@class, method, length, magicCookie, transactionId);
    }

    /// <summary>
    /// Parses a byte array in Big Endian format and returns a <see cref="ValueTuple{StunClass, StunMethod}"/>.
    /// </summary>
    /// <param name="buffer">The byte array to parse in Big Endian format.</param>
    /// <returns>A tuple containing <see cref="EStunClass"/> and <see cref="EStunMethod"/>.</returns>
    /// <exception cref="InvalidDataException">Thrown when invalid data is encountered.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTuple<EStunClass, EStunMethod> GetMessageType(ReadOnlySpan<byte> buffer)
    {
        var @class = (buffer[0] & 0b1111) switch
        {
            0b0000_0000 => EStunClass.Request,
            0b0000_0100 => EStunClass.Indication,
            0b0000_1000 => EStunClass.SuccessResponse,
            0b0000_1100 => EStunClass.ErrorResponse,
            _ => throw new InvalidDataException("Invalid class specified")
        };

        var method = (buffer[1] & 0b1111_1111) switch
        {
            0b0000_0001 => EStunMethod.Binding,
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
    /// <returns>
    /// The magic cookie value as an integer.
    /// If the magic cookie value matches the predefined MagicCookieValue, it returns the magic cookie value as is; otherwise, it returns 0.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetMagicCookie(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < sizeof(uint))
            throw new ArgumentException("Buffer is too short to retrieve the magic cookie.");

        var messageCookie = BinaryPrimitives.ReadInt32BigEndian(buffer);
        return messageCookie != MagicCookieValue ? 0 : messageCookie;
    }

    /// <summary>
    /// Parses a byte array in big-endian format to construct a <see cref="TransactionIdentifier"/> from its binary representation.
    /// </summary>
    /// <param name="buffer">The byte array containing the transaction ID in big-endian format.</param>
    /// <returns>A <see cref="TransactionIdentifier"/> representing the parsed transaction ID.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TransactionIdentifier GetTransactionId(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length != TransactionIdentifier.Size && buffer.Length != TransactionIdentifier.Rfc3849Size)
            throw new MalformatteHeaderException("Invalid transaction ID size");

        return new TransactionIdentifier(buffer.ToArray());
    }
}