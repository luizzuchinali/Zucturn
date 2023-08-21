// Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// Licensed under the MIT License.

using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using Zucturn.Protocol;

var address = IPEndPoint.Parse("127.0.0.1:3478");
var socket = new UdpClient(AddressFamily.InterNetwork);

// var message = new StunMessage(new StunMessageHeader());
// socket.Send(message.ToByteArray());

ushort type = 0b0000_0100_0000_0001;
ushort length = 0b0000_0010_0000_0000;
uint magicCookie = 0x2112A442;

if (BitConverter.IsLittleEndian)
{
    type = (ushort)((type >> 8) | (type << 8));
    length = (ushort)((length >> 8) | (length << 8));
    magicCookie = ((magicCookie >> 24) & 0xFF) |
                  ((magicCookie >> 8) & 0xFF00) |
                  ((magicCookie << 8) & 0xFF0000) |
                  ((magicCookie << 24) & 0xFF000000);
}

using var stream = new MemoryStream();
using var writer = new BinaryWriter(stream);

writer.Write(BitConverter.GetBytes(type));
writer.Write(BitConverter.GetBytes(length));
writer.Write(BitConverter.GetBytes(magicCookie));
writer.Write(TransactionIdentifier.NewIdentifier().ToReadOnlySpan());

socket.Send(stream.GetBuffer(), address);