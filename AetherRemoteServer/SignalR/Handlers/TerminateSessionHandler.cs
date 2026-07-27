using AetherRemoteServer.Services;

namespace AetherRemoteServer.SignalR.Handlers;

public class TerminateSessionHandler(SessionService sessionService)
{
    public bool Terminate(string senderFriendCode)
    {
        return sessionService.EndSession(senderFriendCode);
    }
}