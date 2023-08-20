// // Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// // Licensed under the MIT License.

namespace Zucturn.Protocol.Tests;

public class StunMessageHeaderTests
{
    [Fact]
    public void FromByteArray_ShouldThrowMalformatteHeaderException_WhenBufferTooShort()
    {
        var buffer = new byte[StunMessageHeader.MessageHeaderByteSize - 1];

        var act = () => StunMessageHeader.FromByteArray(buffer);
        act.Should().Throw<MalformatteHeaderException>();
    }

    [Fact]
    public void FromByteArray_ShouldThrowMalformatteHeaderException_WhenBitsNot00()
    {
        var buffer = new byte[StunMessageHeader.MessageHeaderByteSize];

        buffer[0] = 0b1100_0000;

        var act = () => StunMessageHeader.FromByteArray(buffer);
        act.Should().Throw<MalformatteHeaderException>();
    }

    [Fact]
    public void FromByteArray_ShouldNotThrowMalformatteHeaderException_WhenBits00()
    {
        var buffer = new byte[StunMessageHeader.MessageHeaderByteSize];

        buffer[0] = 0b0011_0000;

        var act = () => StunMessageHeader.FromByteArray(buffer);
        act.Should().NotThrow();
    }
}