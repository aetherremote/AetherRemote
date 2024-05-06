namespace AetherRemoteClient.Domain;

public class AsyncResult(bool success, string message = "")
{
    public bool Success = success;
    public string Message = message;
}

public class ResultWithOnlineStatus(bool success, string message = "", bool online = false)
{
    public bool Success = success;
    public string Message = message;
    public bool Online = online;
}
