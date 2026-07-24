using System.Diagnostics;

namespace AetherRemoteServer.Domain;

/// <summary>
///     A bucket for managing tokens for accessing the network
/// </summary>
public class TokenBucket
{
    /// <summary>
    ///     The maximum number of tokens this bucket can have 
    /// </summary>
    public int Capacity { get; }

    /// <summary>
    ///     Refill rate per second
    /// </summary>
    public double RefillRate { get; }

    /// <summary>
    ///     The current number of tokens this bucket has 
    /// </summary>
    public double Tokens { get; private set; }
    
    /// <summary>
    ///     The last time in ticks this bucket was refilled 
    /// </summary>
    private long _lastRefillTicks = Stopwatch.GetTimestamp();

    /// <inheritdoc cref="TokenBucket"/>
    /// <param name="capacity"> How many tokens this bucket can hold </param>
    /// <param name="refillRate"> How many tokens this bucket restores per second </param>
    public TokenBucket(int capacity, double refillRate)
    {
        Capacity = capacity;
        Tokens = capacity;
        RefillRate = refillRate;
    }
    
    private readonly Lock _lock = new();
    
    /// <summary>
    ///     Attempts to consume a token for any given request
    /// </summary>
    /// <returns>True if token was consumed, false if not</returns>
    public bool TryConsumeToken()
    {
        lock (_lock)
        {
            Refill();
            
            if (Tokens < 1.0)
                return false;

            Tokens--;
            return true;
        }

        // I will continue to try these local functions...
        void Refill()
        {
            var now = Stopwatch.GetTimestamp();
            var elapsedSeconds = (now - _lastRefillTicks) / (double)Stopwatch.Frequency;

            Tokens = Math.Min(Capacity, Tokens + elapsedSeconds * RefillRate);
            _lastRefillTicks = now;
        }
    }
}