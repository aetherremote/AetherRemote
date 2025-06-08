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
    public void SafeMode(string operation, string sender)
    {
        var log = new InternalLog
        {
            TimeStamp = DateTime.Now,
            Message = $"Rejected {operation} action from {sender} because you are in safe mode"
        };
        
        Logs.Add(log);
    }

    /// <summary>
    ///     Logs a command rejection due to not being friended
    /// </summary>
    public void NotFriends(string operation, string sender)
    {
        var log = new InternalLog
        {
            TimeStamp = DateTime.Now,
            Message = $"Rejected {operation} action from {sender} because you are not friends"
        };
        
        Logs.Add(log);
    }

    /// <summary>
    ///     Logs a command rejection due to overriding it locally
    /// </summary>
    public void Override(string operation, string sender)
    {
        var log = new InternalLog
        {
            TimeStamp = DateTime.Now,
            Message = $"Rejected {operation} action from {sender} because you are overriding it locally"
        };
        
        Logs.Add(log);
    }

    /// <summary>
    ///     Logs a command rejection due to lacking permissions
    /// </summary>
    public void LackingPermissions(string operation, string sender)
    {
        var log = new InternalLog
        {
            TimeStamp = DateTime.Now,
            Message = $"Rejected {operation} action from {sender} because they are lacking permissions"
        };
        
        Logs.Add(log);
    }

    /// <summary>
    ///     Logs a command rejection due to invalid data
    /// </summary>
    public void InvalidData(string operation, string sender)
    {
        var log = new InternalLog
        {
            TimeStamp = DateTime.Now,
            Message = $"Rejected {operation} action from {sender} because they send invalid data"
        };
        
        Logs.Add(log);
    }
    
    /// <summary>
    ///     Logs a command rejection due to invalid data
    /// </summary>
    public void MissingLocalBody(string operation, string sender)
    {
        var log = new InternalLog
        {
            TimeStamp = DateTime.Now,
            Message = $"Unable to process {operation} action from {sender} because you lack a local body"
        };
        
        Logs.Add(log);
    }

    public void FriendPaused(string operation, string sender)
    {
        var log = new InternalLog
        {
            TimeStamp = DateTime.Now,
            Message = $"Rejected {operation} action from {sender} because you have them paused"
        };
        
        Logs.Add(log);
    }
    
    public void FeaturePaused(string operation, string sender)
    {
        var log = new InternalLog
        {
            TimeStamp = DateTime.Now,
            Message = $"Rejected {operation} action from {sender} because you have that feature paused"
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