// // Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// // Licensed under the MIT License.

using System.Text;
using System.Security.Cryptography;

namespace Zucturn.Protocol;

/// <summary>
/// Represents a STUN Transaction Identifier, a 96-bit identifier used to uniquely identify STUN transactions.
/// </summary>
public readonly struct TransactionIdentifier
{
    private readonly ReadOnlyMemory<byte> _bytes;

    /// <summary>
    /// Gets the length of the Transaction Identifier in bytes (12 bytes).
    /// </summary>
    public int Length => 12;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionIdentifier"/> struct.
    /// </summary>
    /// <param name="bytes">The 12-byte Transaction ID.</param>
    /// <exception cref="ArgumentException">Thrown if the provided byte array is not 12 bytes long.</exception>
    public TransactionIdentifier(ReadOnlyMemory<byte> bytes)
    {
        if (bytes.Length != Length)
            throw new ArgumentException($"Transaction ID must be a {Length}-byte array.", nameof(bytes));

        _bytes = bytes;
    }

    /// <summary>
    /// Generates a new <see cref="TransactionIdentifier"/>.
    /// </summary>
    /// <remarks>
    /// The generated identifier is cryptographically random and is suitable for use in STUN transactions.
    /// </remarks>
    /// <returns>A new <see cref="TransactionIdentifier"/>.</returns>
    public static TransactionIdentifier NewIdentifier()
    {
        var transactionId = new byte[12];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(transactionId);
        return new TransactionIdentifier(new ReadOnlyMemory<byte>(
            BitConverter.IsLittleEndian
                ? transactionId.Reverse().ToArray()
                : transactionId)
        );
    }

    /// <summary>
    /// Converts the Transaction Identifier to a <see cref="ReadOnlySpan{byte}"/>.
    /// </summary>
    /// <returns>A read-only span representing the Transaction Identifier.</returns>
    public ReadOnlySpan<byte> ToReadOnlySpan()
    {
        return _bytes.Span;
    }

    /// <summary>
    /// Determines whether this <see cref="TransactionIdentifier"/> is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns>True if the objects are equal; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is TransactionIdentifier other)
            return ToReadOnlySpan().SequenceEqual(other.ToReadOnlySpan());

        return false;
    }

    /// <summary>
    /// Computes the hash code for this <see cref="TransactionIdentifier"/>.
    /// </summary>
    /// <returns>The computed hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            HashCode.Combine(_bytes.Span[0], _bytes.Span[1], _bytes.Span[2], _bytes.Span[3], _bytes.Span[4],
                _bytes.Span[5]),
            HashCode.Combine(_bytes.Span[6], _bytes.Span[7], _bytes.Span[8], _bytes.Span[9], _bytes.Span[10],
                _bytes.Span[11])
        );
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
    /// </summary>
    /// <returns>A hexadecimal string.</returns>
    public override string ToString()
    {
        var builder = new StringBuilder(24);

        foreach (var b in _bytes.Span)
            builder.Append($"{b:x2}");

        return builder.ToString();
    }
}