// Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// Licensed under the MIT License.

using Serilog;
using System.Net;
using System.Net.Sockets;
using Zucturn.Protocol;

var address = IPEndPoint.Parse("127.0.0.1:3478");
var socket = new UdpClient(AddressFamily.InterNetwork);

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var message = new StunMessage(new StunHeader());
// socket.Send(message.ToByteArray());
socket.Send("Hello"u8, address);