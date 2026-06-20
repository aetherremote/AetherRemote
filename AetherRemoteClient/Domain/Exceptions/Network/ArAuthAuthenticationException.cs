using System;
using AetherRemoteClient.Domain.Enums;

namespace AetherRemoteClient.Domain.Exceptions.Network;

public class ArAuthAuthenticationException(ArAuthAuthenticationErrorCode errorCode) : Exception
{
    public ArAuthAuthenticationErrorCode ErrorCode { get; } = errorCode;
}