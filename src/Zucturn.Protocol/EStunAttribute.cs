// // Copyright (c) 2023 Luiz Antonio Anacleto Zuchinali and Contributors
// // Licensed under the MIT License.

using System.Runtime.Serialization;

namespace Zucturn.Protocol;

// @formatter:off
public enum EStunAttribute : ushort
{
    #region Mandatory
    [EnumMember(Value = "MAPPED-ADDRESS")]
    MappedAddress =         0x0001,

    [EnumMember(Value = "RESPONSE-ADDRESS")]
    ResponseAddress =       0x0002,

    [EnumMember(Value = "CHANGE-REQUEST")]
    ChangeRequest =         0x0003,

    [EnumMember(Value = "SOURCE-ADDRESS")]
    SourceAddress =         0x0004,

    [EnumMember(Value = "CHANGED-ADDRESS")]
    ChangedAddress =        0x0005,

    [EnumMember(Value = "USERNAME")]
    Username =              0x0006,

    [EnumMember(Value = "PASSWORD")]
    Password =              0x0007,

    [EnumMember(Value = "MESSAGE-INTEGRITY")]
    MessageIntegrity =      0x0008,

    [EnumMember(Value = "ERROR-CODE")]
    ErrorCode =             0x0009,

    [EnumMember(Value = "UNKNOWN-ATTRIBUTES")]
    UnknownAttributes =     0x000a,

    [EnumMember(Value = "REFLECTED-FROM")]
    ReflectedFrom =         0x000b
    #endregion
}
// @formatter:on