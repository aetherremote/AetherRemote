using AetherRemoteServer.Managers;

namespace AetherRemoteServer.Domain.Interfaces;

public interface IPossessionManager
{
    public void TryAddSession(string ghostFriendCode, string hostFriendCode, Session session);

    public Session? TryGetSession(string friendCode);

    public void TryRemoveSession(Session session);
}