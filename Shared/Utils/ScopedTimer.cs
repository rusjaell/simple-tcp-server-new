using System;
using System.Diagnostics;

namespace Shared.Utils;

public readonly struct ScopedTimer : IDisposable
{
    private readonly string _message;
    private readonly long _startTime;

    public ScopedTimer(string message)
    {
        _message = message;
        _startTime = Stopwatch.GetTimestamp();
    }

    public readonly void Dispose() => Console.WriteLine($"{_message} took: {Stopwatch.GetElapsedTime(_startTime).TotalMilliseconds}ms");
}