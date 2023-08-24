// // Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// // Licensed under the MIT License.

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

        var randomBytes = new byte[TransactionIdentifier.Size];
        new Random().NextBytes(randomBytes);
        randomBytes.CopyTo(_validBuffer, 8);
    }

    [Fact]
    public void StunMessageHeader_ShouldInitializeHeaderProperties()
    {
        // Arrange
        const EStunClass expectedClass = EStunClass.Request;
        const EStunMethod expectedMethod = EStunMethod.Binding;
        const ushort expectedMessageLength = (ushort)5123;

        // Act
        var header = new StunMessageHeader(expectedClass, expectedMethod, expectedMessageLength);

        // Assert
        header.Class.Should().Be(expectedClass);
        header.Method.Should().Be(expectedMethod);
        header.MessageLength.Should().Be(expectedMessageLength);
        header.MagicCookie.Should().Be(StunMessageHeader.MagicCookieValue);
        header.TransactionId.Should().NotBeNull();
        header.TransactionId.ToByteArray().Length.Should().Be(TransactionIdentifier.Size);
    }

    [Fact]
    public void StunMessageHeader_Properties_ShouldWorkCorrectly()
    {
        // Arrange
        var header = new StunMessageHeader();
        const EStunClass expectedClass = EStunClass.Request;
        const EStunMethod expectedMethod = EStunMethod.Binding;
        const ushort expectedMessageLength = 1234;
        const int expectedMagicCookie = StunMessageHeader.MagicCookieValue;
        var expectedTransactionId = new TransactionIdentifier();

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
    public void ToByteArray_ShouldConvertToBigEndianByteArray()
    {
        // Arrange
        var header = new StunMessageHeader
        {
            Class = EStunClass.Request,
            Method = EStunMethod.Binding,
            MessageLength = 5123,
            MagicCookie = 0x2112A442,
            TransactionId = new TransactionIdentifier(new byte[]
            {
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06,
                0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C
            })
        };

        // Act
        var byteArray = header.ToByteArray();

        // Assert
        byteArray.Should().NotBeNull();
        byteArray.Length.Should().Be(StunMessageHeader.MessageHeaderByteSize);

        // Verify the byte order of fields.
        byteArray[0].Should().Be((byte)EStunClass.Request);
        byteArray[1].Should().Be((byte)EStunMethod.Binding);

        //MessageLength
        byteArray[2].Should().Be(0x14);
        byteArray[3].Should().Be(0x03);

        // MagicCookie
        byteArray[4].Should().Be(0x21);
        byteArray[5].Should().Be(0x12);
        byteArray[6].Should().Be(0xA4);
        byteArray[7].Should().Be(0x42);

        // TransactionId
        byteArray[8..20].Should().BeEquivalentTo(header.TransactionId.ToByteArray());
    }

    [Fact]
    public void ToByteArray_ShouldConvertToBigEndianByteArray_ForOldRFC()
    {
        // Arrange
        var header = new StunMessageHeader
        {
            Class = EStunClass.Request,
            Method = EStunMethod.Binding,
            MessageLength = 5123,
            TransactionId = new TransactionIdentifier(new byte[]
            {
                0x10, 0x11, 0x12, 0x13, 0x14, 0x03,
                0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
                0x0A, 0x0B, 0x0C, 0x19
            })
        };

        // Act
        var byteArray = header.ToByteArray();

        // Assert
        byteArray.Should().NotBeNull();
        byteArray.Length.Should().Be(StunMessageHeader.MessageHeaderByteSize);

        // Verify the byte order of fields for the old RFC scenario.
        byteArray[0].Should().Be((byte)EStunClass.Request);
        byteArray[1].Should().Be((byte)EStunMethod.Binding);

        // MessageLength
        byteArray[2].Should().Be(0x14);
        byteArray[3].Should().Be(0x03);

        // TransactionId
        byteArray[4..20].Should().BeEquivalentTo(header.TransactionId.ToByteArray());
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
    public void FromByteArray_ShouldParseRFC3489Header_WhenMagicCookieIsNotMatching()
    {
        // Arrange
        var buffer = new byte[]
        {
            0b0000_0000, 0b0000_0001, // Message Type (Binding Request)
            0, 0, // Message Length
            13, 14, 15, 16, // Invalid Magic Cookie
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 // Transaction ID
        };

        // Act
        var header = StunMessageHeader.FromByteArray(buffer);

        // Assert
        header.Class.Should().Be(EStunClass.Request);
        header.Method.Should().Be(EStunMethod.Binding);
        header.MessageLength.Should().Be(0);
        header.MagicCookie.Should().Be(0);
        header.TransactionId.ToString().Should().Be("0d0e0f100102030405060708090a0b0c");
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
    [InlineData(0b0000_0000, 0b0000_0001, EStunClass.Request, EStunMethod.Binding)]
    [InlineData(0b0000_0100, 0b0000_0001, EStunClass.Indication, EStunMethod.Binding)]
    [InlineData(0b0000_1000, 0b0000_0001, EStunClass.SuccessResponse, EStunMethod.Binding)]
    [InlineData(0b0000_1100, 0b0000_0001, EStunClass.ErrorResponse, EStunMethod.Binding)]
    public void GetMessageType_ShouldReturnValidClassAndMethod_WhenBufferIsCorrect(
        byte classBits, byte methodBits, EStunClass expectedClass, EStunMethod expectedMethod)
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
        var buffer = new byte[TransactionIdentifier.Size + 1];

        // Act & Assert
        Action act = () => StunMessageHeader.GetTransactionId(buffer);
        act.Should().Throw<MalformatteHeaderException>();
    }
}