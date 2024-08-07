using System;

namespace AetherRemoteClient.Domain.Logger;

public class AetherRemoteInternalLog
{
    public string Message;
    public DateTime Timestamp;

    public AetherRemoteInternalLog(string message, DateTime timestamp)
    {
        Message = message;
        Timestamp = timestamp;
    }
}
