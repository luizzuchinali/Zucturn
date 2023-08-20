// // Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// // Licensed under the MIT License.

namespace Zucturn.Protocol.Exceptions;

public class MalformatteHeaderException : Exception
{
    public MalformatteHeaderException(string message) : base(message)
    {
    }
}