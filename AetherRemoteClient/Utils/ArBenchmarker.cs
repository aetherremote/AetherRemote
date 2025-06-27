using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace AetherRemoteClient.Utils;

public class ArBenchmarker(string benchmarkName, int sample)
{
    private readonly List<int> _times = [];
    private readonly Stopwatch _stopwatch = new();

    public void Start()
    {
        _stopwatch.Start();
    }

    public void Stop()
    {
        _stopwatch.Stop();
        _times.Add(_stopwatch.Elapsed.Nanoseconds);
        if (_times.Count < sample)
            return;
        
        Plugin.Log.Info($"[{benchmarkName}] Average: " + _times.Average().ToString(CultureInfo.InvariantCulture));
        _times.Clear();
    }
}