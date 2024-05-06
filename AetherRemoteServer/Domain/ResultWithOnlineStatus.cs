namespace AetherRemoteServer.Domain;

public class ResultWithOnlineStatus
{
    public bool Success;
    public string Message;
    public bool Online;

    public ResultWithOnlineStatus(bool success, string message = "", bool online = false)
    {
        Success = success;
        Message = message;
        Online = online;
    }
}
