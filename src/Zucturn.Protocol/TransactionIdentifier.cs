// // Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// // Licensed under the MIT License.

using System.Runtime.InteropServices;
using System.Text;
using System.Security.Cryptography;

namespace Zucturn.Protocol;

/// <summary>
/// Represents a STUN Transaction Identifier, a 96-bit or 128-bit (for backward compatibility) identifier used to uniquely identify STUN transactions.
/// For more information, refer to RFC 8489: https://datatracker.ietf.org/doc/html/rfc8489#autoid-5
/// </summary>
public readonly struct TransactionIdentifier
{
    private readonly ReadOnlyMemory<byte> _bytes;

    /// <summary>
    /// Gets a value indicating whether this <see cref="TransactionIdentifier"/> represents an RFC 3849 Transaction ID.
    /// RFC 3849 defines a 128-bit format for Transaction IDs.
    /// </summary>
    public bool IsRfc3849 => _bytes.Length == Rfc3849Size;

    /// <summary>
    /// Gets the length of the Transaction Identifier in bytes (12 bytes).
    /// </summary>
    public static int Size => 12;

    /// <summary>
    /// Gets the length of the Transaction Identifier in bytes (16 bytes).
    /// </summary>
    public static int Rfc3849Size => 16;

    /// <summary>
    /// Generates a new 96-bit <see cref="TransactionIdentifier"/>.
    /// </summary>
    /// <remarks>
    /// The generated identifier is cryptographically random and is suitable for use in STUN transactions.
    /// </remarks>
    /// <returns>A new <see cref="TransactionIdentifier"/>.</returns>
    public TransactionIdentifier()
    {
        var transactionId = new byte[12];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(transactionId);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(transactionId);

        _bytes = new ReadOnlyMemory<byte>(transactionId);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionIdentifier"/> struct.
    /// </summary>
    /// <param name="bytes">The 12-byte Transaction ID.</param>
    /// <exception cref="ArgumentException">Thrown if the provided byte array is not 12 bytes long.</exception>
    public TransactionIdentifier(ReadOnlyMemory<byte> bytes)
    {
        if (bytes.Length != Size && bytes.Length != Rfc3849Size)
            throw new ArgumentException($"Transaction ID must be a {Size}-byte or {Rfc3849Size}-byte array.",
                nameof(bytes));

        _bytes = bytes;
    }

    /// <summary>
    /// Converts the Transaction Identifier to a <see cref="ReadOnlySpan{byte}"/>.
    /// </summary>
    /// <returns>A read-only span representing the Transaction Identifier.</returns>
    public byte[] ToByteArray()
    {
        return _bytes.Span.ToArray();
    }

    /// <summary>
    /// Determines whether this <see cref="TransactionIdentifier"/> is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns>True if the objects are equal; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is TransactionIdentifier other)
            return ToByteArray().SequenceEqual(other.ToByteArray());

        return false;
    }

    /// <summary>
    /// Computes the hash code for this <see cref="TransactionIdentifier"/>.
    /// </summary>
    /// <returns>The computed hash code.</returns>
    public override int GetHashCode()
    {
        var hashCode = 17;
        foreach (var b in _bytes.Span)
            hashCode = hashCode * 31 + b.GetHashCode();

        return hashCode;
    }


    /// <summary>
    /// Determines whether two <see cref="TransactionIdentifier"/> objects are equal.
    /// </summary>
    /// <param name="left">The first <see cref="TransactionIdentifier"/>.</param>
    /// <param name="right">The second <see cref="TransactionIdentifier"/>.</param>
    /// <returns>True if the objects are equal; otherwise, false.</returns>
    public static bool operator ==(TransactionIdentifier left, TransactionIdentifier right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="TransactionIdentifier"/> objects are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="TransactionIdentifier"/>.</param>
    /// <param name="right">The second <see cref="TransactionIdentifier"/>.</param>
    /// <returns>True if the objects are not equal; otherwise, false.</returns>
    public static bool operator !=(TransactionIdentifier left, TransactionIdentifier right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Returns a hexadecimal string representation of the <see cref="TransactionIdentifier"/>.
    /// If this identifier is an RFC 3849 Transaction ID, the resulting string will be 32 characters long,
    /// otherwise, it will be 24 characters long.
    /// </summary>
    /// <returns>A hexadecimal string.</returns>
    public override string ToString()
    {
        var builder = new StringBuilder(IsRfc3849 ? 32 : 24);

        foreach (var b in _bytes.Span)
            builder.Append($"{b:x2}");

        return builder.ToString();
    }
}