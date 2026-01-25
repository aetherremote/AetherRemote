using System.Diagnostics;

namespace AetherRemoteServer.Domain;

/// <summary>
///     A bucket for managing tokens for accessing the network
/// </summary>
public class TokenBucket(int maxTokens)
{
    /// <summary>
    ///     The maximum number of tokens this bucket can have 
    /// </summary>
    public float Capacity = maxTokens;
    
    /// <summary>
    ///     The current number of tokens this bucket has 
    /// </summary>
    public float Tokens = maxTokens;
    
    /// <summary>
    ///     The last time in ticks this bucket was refilled 
    /// </summary>
    public long LastRefillTicks = Stopwatch.GetTimestamp();

    /// <summary>
    ///     Attempts to consume a token for any given request
    /// </summary>
    /// <returns>True if token was consumed, false if not</returns>
    public bool TryConsumeToken()
    {
        var now = Stopwatch.GetTimestamp();
        var seconds = (now - LastRefillTicks) / (float)Stopwatch.Frequency;

        if (seconds > 0)
        {
            // Refill
            Tokens = Math.Min(Capacity, Tokens + Capacity * seconds);
            LastRefillTicks = now;
        }

        if (Tokens < 1.0f)
            return false;

        Tokens--;
        return true;
    }
}