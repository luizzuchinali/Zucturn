// // Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// // Licensed under the MIT License.

namespace Zucturn.Protocol.Tests;

public class TransactionIdentifierTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance_WhenValidBytes()
    {
        // Arrange
        var validBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        const string expectedString = "0102030405060708090a0b0c";

        // Act & Assert
        var transactionId = new TransactionIdentifier(validBytes);
        transactionId.ToString().Should().Be(expectedString);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenInvalidBytes()
    {
        // Arrange
        var invalidBytes = new byte[11];

        // Act & Assert
        var action = () => { _ = new TransactionIdentifier(invalidBytes); };
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NewIdentifier_ShouldReturnDifferentIdentifiers_WhenCalledTwice()
    {
        // Act
        var identifier1 = TransactionIdentifier.NewIdentifier();
        var identifier2 = TransactionIdentifier.NewIdentifier();

        // Assert
        identifier1.Should().NotBe(identifier2);
    }

    [Fact]
    public void NewIdentifier_ShouldReturnIdentifierWithCorrectLength()
    {
        // Act
        var identifier = TransactionIdentifier.NewIdentifier();

        // Assert
        identifier.Length.Should().Be(12);
    }

    [Fact]
    public void NewIdentifier_ShouldReturnIdentifierWithRandomBytes()
    {
        // Act
        var identifier1 = TransactionIdentifier.NewIdentifier();
        var identifier2 = TransactionIdentifier.NewIdentifier();

        // Assert
        identifier1.ToReadOnlySpan().ToArray().Should().NotBeEquivalentTo(identifier2.ToReadOnlySpan().ToArray());
    }

    [Fact]
    public void ToReadOnlySpan_ShouldReturnSpan()
    {
        // Arrange
        var bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var transactionId = new TransactionIdentifier(bytes);

        // Act
        var span = transactionId.ToReadOnlySpan();

        // Assert
        span.ToArray().Should().BeEquivalentTo(bytes);
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenEqualObjects()
    {
        // Arrange
        var bytes1 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var bytes2 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var transactionId1 = new TransactionIdentifier(bytes1);
        var transactionId2 = new TransactionIdentifier(bytes2);

        // Act & Assert
        transactionId1.Should().Be(transactionId2);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenDifferentObjects()
    {
        // Arrange
        var bytes1 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var bytes2 = new byte[] { 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
        var transactionId1 = new TransactionIdentifier(bytes1);
        var transactionId2 = new TransactionIdentifier(bytes2);

        // Act & Assert
        transactionId1.Should().NotBe(transactionId2);
    }

    [Fact]
    public void EqualsMethod_ShouldReturnFalse_WhenComparingWithDifferentType()
    {
        // Arrange
        var bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var transactionId = new TransactionIdentifier(bytes);

        // Act
        // ReSharper disable once SuspiciousTypeConversion.Global
        var result = transactionId.Equals("Not a TransactionIdentifier");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_ShouldReturnSameHashCode_WhenEqualObjects()
    {
        // Arrange
        var bytes1 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var bytes2 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var transactionId1 = new TransactionIdentifier(bytes1);
        var transactionId2 = new TransactionIdentifier(bytes2);

        // Act & Assert
        transactionId1.GetHashCode().Should().Be(transactionId2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ShouldReturnDifferentHashCode_WhenDifferentObjects()
    {
        // Arrange
        var bytes1 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var bytes2 = new byte[] { 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
        var transactionId1 = new TransactionIdentifier(bytes1);
        var transactionId2 = new TransactionIdentifier(bytes2);

        // Act & Assert
        transactionId1.GetHashCode().Should().NotBe(transactionId2.GetHashCode());
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenEqualObjects()
    {
        // Arrange
        var bytes1 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var bytes2 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var transactionId1 = new TransactionIdentifier(bytes1);
        var transactionId2 = new TransactionIdentifier(bytes2);

        // Act & Assert
        (transactionId1 == transactionId2).Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_WhenDifferentObjects()
    {
        // Arrange
        var bytes1 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var bytes2 = new byte[] { 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
        var transactionId1 = new TransactionIdentifier(bytes1);
        var transactionId2 = new TransactionIdentifier(bytes2);

        // Act & Assert
        (transactionId1 == transactionId2).Should().BeFalse();
        (transactionId1 != transactionId2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnHexadecimalString()
    {
        // Arrange
        var bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var transactionId = new TransactionIdentifier(bytes);
        const string expectedString = "0102030405060708090a0b0c";

        // Act
        var result = transactionId.ToString();

        // Assert
        result.Should().Be(expectedString);
    }
}