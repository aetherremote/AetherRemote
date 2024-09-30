using AetherRemoteCommon;
using System;
using System.Timers;

namespace AetherRemoteClient.Domain;

public class CommandLockoutManager : IDisposable
{
    public bool IsLocked { get; private set; } = false;

    private readonly Timer commandLockoutTimer;

    public CommandLockoutManager()
    {
        commandLockoutTimer = new Timer();
        commandLockoutTimer.AutoReset = false;
        commandLockoutTimer.Elapsed += LockoutComplete;
    }

    public void Lock(uint cooldownInSeconds = Constraints.GameCommandCooldownInSeconds)
    {
        IsLocked = true;
        commandLockoutTimer.Stop();
        commandLockoutTimer.Interval = cooldownInSeconds * 1000;
        commandLockoutTimer.Start();
    }

    public void Unlock() => IsLocked = false;

    private void LockoutComplete(object? sender, ElapsedEventArgs e) => IsLocked = false;

    public void Dispose()
    {
        commandLockoutTimer.Elapsed -= LockoutComplete;
        commandLockoutTimer.Dispose();
        GC.SuppressFinalize(this);
    }
}
