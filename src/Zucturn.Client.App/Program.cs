// Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// Licensed under the MIT License.

namespace Zucturn.Client.App;

[ExcludeFromCodeCoverage]
internal class Program
{
    public static void Main()
    {
        var address = IPEndPoint.Parse("127.0.0.1:3478");
        using var socket = new UdpClient(AddressFamily.InterNetwork);

        var header = new StunMessageHeader(EStunClass.Request, EStunMethod.Binding, 5123);
        var message = new StunMessage(header, new Dictionary<EStunAttribute, (ushort, byte[])>
        {
            { EStunAttribute.Username, new(10, new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0x2 }) }
        });
        socket.Send(message.ToByteArray(), address);
    }
}