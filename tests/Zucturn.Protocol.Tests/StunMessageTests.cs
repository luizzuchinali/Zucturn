// // Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// // Licensed under the MIT License.

namespace Zucturn.Protocol.Tests;

public class StunMessageTests
{
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