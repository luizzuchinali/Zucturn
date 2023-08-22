// // Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// // Licensed under the MIT License.

namespace Zucturn.Protocol.Tests;

public class StunMessageTests
{
    [Fact]
    public void ToByteArray_ShouldConvertToBigEndianByteArray()
    {
        // Arrange
        var message = new StunMessage(new StunMessageHeader
        {
            Class = StunClass.Request,
            Method = StunMethod.Binding,
            MessageLength = 5123,
            TransactionId = new TransactionIdentifier(new byte[]
            {
                0x10, 0x11, 0x12, 0x13, 0x14, 0x03,
                0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
                0x0A, 0x0B, 0x0C, 0x19
            })
        });

        // Act
        var byteArray = message.ToByteArray();

        // Assert
        byteArray.Should().NotBeNull();
        byteArray.Length.Should().Be(StunMessageHeader.MessageHeaderByteSize);

        byteArray[0].Should().Be((byte)StunClass.Request);
        byteArray[1].Should().Be((byte)StunMethod.Binding);
        byteArray[2..4].Should().BeEquivalentTo(new byte[] { 0x14, 0x03 });
        byteArray[4..20].Should().BeEquivalentTo(message.MessageHeader.TransactionId.ToByteArray());
    }

    [Fact]
    public void FromByteArray_ShouldCreateStunMessage_WhenValidHeader()
    {
        // Arrange
        var headerBytes = new byte[]
        {
            0x00, 0x01, 0x00, 0x08, 0x21, 0x12, 0xA4, 0x42,
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0A, 0x0B, 0x0C
        };

        // Act
        var stunMessage = StunMessage.FromByteArray(headerBytes);

        // Assert
        stunMessage.MessageHeader.Class.Should().Be(StunClass.Request);
        stunMessage.MessageHeader.Method.Should().Be(StunMethod.Binding);
        stunMessage.MessageHeader.MagicCookie.Should().Be(StunMessageHeader.MagicCookieValue);
        stunMessage.MessageHeader.MessageLength.Should().Be(8);
        stunMessage.MessageHeader.TransactionId.ToString().Should().Be("0102030405060708090a0b0c");
    }

    [Fact]
    public void FromByteArray_ShouldThrowEmptyBufferException_WhenEmptyBuffer()
    {
        // Arrange
        var emptyBuffer = Array.Empty<byte>();

        // Act & Assert
        Action action = () => StunMessage.FromByteArray(emptyBuffer);

        action.Should().Throw<EmptyBufferException>()
            .WithMessage("STUN message buffer can't be empty");
    }
}