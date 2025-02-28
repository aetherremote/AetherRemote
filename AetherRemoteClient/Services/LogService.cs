using System;
using System.Collections.Generic;
using AetherRemoteClient.Domain;

namespace AetherRemoteClient.Services;

/// <summary>
///     Class responsible for maintaining the logs of internal operations
/// </summary>
public class LogService
{
    public readonly List<InternalLog> Logs = [];

    /// <summary>
    ///     Logs a command rejection due to safe mode 
    /// </summary>
    public void SafeMode(string action, string sender)
    {
        var log = new InternalLog
        {
            TimeStamp = DateTime.Now,
            Message = $"Rejected {action} action from {sender} because you are in safe mod"
        };
        
        Logs.Add(log);
    }

    /// <summary>
    ///     Logs a command rejection due to not being friended
    /// </summary>
    public void NotFriends(string action, string sender)
    {
        var log = new InternalLog
        {
            TimeStamp = DateTime.Now,
            Message = $"Rejected {action} action from {sender} because you are not friends"
        };
        
        Logs.Add(log);
    }

    /// <summary>
    ///     Logs a command rejection due to overriding it locally
    /// </summary>
    public void Override(string action, string sender)
    {
        var log = new InternalLog
        {
            TimeStamp = DateTime.Now,
            Message = $"Rejected {action} action from {sender} because you are overriding it locally"
        };
        
        Logs.Add(log);
    }

    /// <summary>
    ///     Logs a command rejection due to lacking permissions
    /// </summary>
    public void LackingPermissions(string action, string sender)
    {
        var log = new InternalLog
        {
            TimeStamp = DateTime.Now,
            Message = $"Rejected {action} action from {sender} because they are lacking permissions"
        };
        
        Logs.Add(log);
    }

    /// <summary>
    ///     Logs a command rejection due to invalid data
    /// </summary>
    public void InvalidData(string action, string sender)
    {
        var log = new InternalLog
        {
            TimeStamp = DateTime.Now,
            Message = $"Rejected {action} action from {sender} because they send invalid data"
        };
        
        Logs.Add(log);
    }
    
    /// <summary>
    ///     Logs a command rejection due to invalid data
    /// </summary>
    public void MissingLocalBody(string action, string sender)
    {
        var log = new InternalLog
        {
            TimeStamp = DateTime.Now,
            Message = $"Unable to process {action} action from {sender} because you lack a local body"
        };
        
        Logs.Add(log);
    }

    /// <summary>
    ///     Logs a custom message
    /// </summary>
    public void Custom(string message)
    {
        var log = new InternalLog
        {
            TimeStamp = DateTime.Now,
            Message = message
        };
        
        Logs.Add(log);
    }
}