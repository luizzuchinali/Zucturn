// // Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// // Licensed under the MIT License.

using System.Buffers.Binary;
using System.Net;

namespace Zucturn.Protocol.Tests;

public class StunMessageHeaderTests
{
    private readonly byte[] _validBuffer = new byte[StunMessageHeader.MessageHeaderByteSize];

    public StunMessageHeaderTests()
    {
        _validBuffer[0] = 0b0000;
        _validBuffer[1] = 0b0001;

        var lengthBytes = BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder(512));
        lengthBytes.CopyTo(_validBuffer, 2);

        var magicCookieBytes =
            BitConverter.GetBytes((uint)IPAddress.HostToNetworkOrder(StunMessageHeader.MagicCookieValue));
        magicCookieBytes.CopyTo(_validBuffer, 4);

        var randomBytes = new byte[StunMessageHeader.TransactionIdByteSize];
        new Random().NextBytes(randomBytes);
        randomBytes.CopyTo(_validBuffer, 8);
    }

    [Fact]
    public void StunMessageHeader_Properties_ShouldWorkCorrectly()
    {
        // Arrange
        var header = new StunMessageHeader();
        const StunClass expectedClass = StunClass.Request;
        const StunMethod expectedMethod = StunMethod.Binding;
        const ushort expectedMessageLength = (ushort)1234;
        const int expectedMagicCookie = StunMessageHeader.MagicCookieValue;
        var expectedTransactionId = TransactionIdentifier.NewIdentifier();

        // Act
        header.Class = expectedClass;
        header.Method = expectedMethod;
        header.MessageLength = expectedMessageLength;
        header.MagicCookie = expectedMagicCookie;
        header.TransactionId = expectedTransactionId;

        // Assert
        header.Class.Should().Be(expectedClass);
        header.Method.Should().Be(expectedMethod);
        header.MessageLength.Should().Be(expectedMessageLength);
        header.MagicCookie.Should().Be(expectedMagicCookie);
        header.TransactionId.Should().Be(expectedTransactionId);
    }

    [Fact]
    public void FromByteArray_ShouldThrowMalformatteHeaderException_WhenBufferTooShort()
    {
        // Arrange
        var buffer = new byte[StunMessageHeader.MessageHeaderByteSize - 1];

        // Act
        Action act = () => StunMessageHeader.FromByteArray(buffer);

        // Assert
        act.Should().Throw<MalformatteHeaderException>();
    }

    [Fact]
    public void FromByteArray_ShouldThrowMalformatteHeaderException_WhenBitsNot00()
    {
        // Arrange
        var buffer = new byte[StunMessageHeader.MessageHeaderByteSize];
        buffer[0] = 0b1100_0000;

        // Act
        Action act = () => StunMessageHeader.FromByteArray(buffer);

        // Assert
        act.Should().Throw<MalformatteHeaderException>();
    }

    [Fact]
    public void FromByteArray_ShouldNotThrowMalformatteHeaderException_WhenBits00()
    {
        // Arrange & Act
        Action act = () => StunMessageHeader.FromByteArray(_validBuffer);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void FromByteArray_ShouldThrowMalformatteHeaderException_WhenMagicCookieInvalid()
    {
        // Arrange
        var buffer = new byte[StunMessageHeader.MessageHeaderByteSize];
        buffer[0] = 0b0011_0000;
        buffer[1] = 0b0000_0001;
        var magicCookie = BitConverter.GetBytes(0x12345678);
        magicCookie.CopyTo(buffer, 4);

        // Act
        Action act = () => StunMessageHeader.FromByteArray(buffer);

        // Assert
        act.Should().Throw<MalformatteHeaderException>().WithMessage("Invalid Magic Cookie");
    }

    [Fact]
    public void FromByteArray_ShouldNotThrowMalformatteHeaderException_WhenMagicCookieValid()
    {
        // Arrange & Act
        Action act = () => StunMessageHeader.FromByteArray(_validBuffer);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(0b0000_0000, 0b0000_0001, StunClass.Request, StunMethod.Binding)]
    [InlineData(0b0000_0100, 0b0000_0001, StunClass.Indication, StunMethod.Binding)]
    [InlineData(0b0000_1000, 0b0000_0001, StunClass.SuccessResponse, StunMethod.Binding)]
    [InlineData(0b0000_1100, 0b0000_0001, StunClass.ErrorResponse, StunMethod.Binding)]
    public void GetMessageType_ShouldReturnValidClassAndMethod_WhenBufferIsCorrect(
        byte classBits, byte methodBits, StunClass expectedClass, StunMethod expectedMethod)
    {
        // Arrange
        var buffer = new byte[2];
        buffer[0] = classBits;
        buffer[1] = methodBits;

        // Act
        var (stunClass, stunMethod) = StunMessageHeader.GetMessageType(buffer);

        // Assert
        stunClass.Should().Be(expectedClass);
        stunMethod.Should().Be(expectedMethod);
    }

    [Fact]
    public void GetMessageType_ShouldThrowInvalidDataException_WhenInvalidClassSpecified()
    {
        // Arrange
        var buffer = new byte[2];
        buffer[0] = 0b0000_0011; // Invalid Class

        // Act
        Action act = () => StunMessageHeader.GetMessageType(buffer);

        // Assert
        act.Should().Throw<InvalidDataException>().WithMessage("Invalid class specified");
    }

    [Fact]
    public void GetMessageType_ShouldThrowInvalidDataException_WhenInvalidMethodSpecified()
    {
        // Arrange
        var buffer = new byte[2];
        buffer[0] = 0b0000_0000; // Class: Request
        buffer[1] = 0b0000_0010; // Invalid Method

        // Act
        Action act = () => StunMessageHeader.GetMessageType(buffer);

        // Assert
        act.Should().Throw<InvalidDataException>().WithMessage("Invalid method specified");
    }

    [Theory]
    [InlineData(1523)]
    [InlineData(512)]
    [InlineData(9984)]
    [InlineData(63459)]
    public void GetLength_ShouldReturnCorrectValue_WhenBufferIsCorrect(ushort expectedLength)
    {
        // Arrange
        var lengthBytes = BitConverter.GetBytes(BitConverter.IsLittleEndian
            ? (ushort)((expectedLength >> 8) | (expectedLength << 8))
            : expectedLength);
        //Act
        var length = StunMessageHeader.GetLength(lengthBytes);

        // Assert
        length.Should().Be(expectedLength);
    }

    [Fact]
    public void GetLength_ShouldThrowArgumentOutOfRangeException_WhenBufferIsTooShort()
    {
        // Arrange
        var buffer = new byte[] { 0b0000_0010 }; // Tamanho do buffer excessivo

        // Act
        Action act = () => StunMessageHeader.GetLength(buffer);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetMagicCookie_ShouldReturnMagicCookie_WhenBufferIsValid()
    {
        // Arrange
        byte[] buffer = { 0x21, 0x12, 0xA4, 0x42 };

        // Act
        var magicCookie = StunMessageHeader.GetMagicCookie(buffer);

        // Assert
        const int expectedMagicCookie = 0x2112A442;
        magicCookie.Should().Be(expectedMagicCookie);
    }

    [Fact]
    public void GetMagicCookie_ShouldThrowArgumentException_WhenBufferIsTooShort()
    {
        // Arrange
        byte[] buffer = { 0x21, 0x12, 0xA4 }; // Incomplete buffer

        // Act and Assert
        Assert.Throws<ArgumentException>(() => StunMessageHeader.GetMagicCookie(buffer));
    }

    [Fact]
    public void GetTransactionId_ShouldThrowMalformatteHeaderException_WhenInvalidSize()
    {
        var buffer = new byte[StunMessageHeader.TransactionIdByteSize + 1];

        // Act & Assert
        Action act = () => StunMessageHeader.GetTransactionId(buffer);
        act.Should().Throw<MalformatteHeaderException>();
    }
}