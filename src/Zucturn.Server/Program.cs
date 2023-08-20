// Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// Licensed under the MIT License.

using Serilog;
using System.Net;
using System.Text;
using System.Net.Sockets;
using Zucturn.Protocol;

var ip = args[0];
var port = args[1];

var address = IPEndPoint.Parse($"{ip}:{port}");
var socket = new UdpClient(address);

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

logger.Information($"Listening on {ip}:{port}");
while (socket.Client.IsBound)
{
    var result = await socket.ReceiveAsync();
    logger.Information(Encoding.UTF8.GetString(result.Buffer));

    var message = StunMessage.FromByteArray(result.Buffer);
}