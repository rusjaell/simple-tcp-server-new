using Client.Network;
using System;
using System.Diagnostics;

namespace Client;

public sealed class Application : IDisposable
{
    public TCPSocketClient TCPClient;

    public Application(string[] args)
    {
        TCPClient = new TCPSocketClient("127.0.0.1", 2050);
    }

    public void Initialize()
    {
        TCPClient.Connect();
    }

    public void Run(int updateRate = 30)
    {
        var frameTime = 1.0 / updateRate;

        var updates = 0;

        var sw = Stopwatch.StartNew();

        var totalTime = 0.0;
        var lastFrameTime = 0.0;

        var accumulator = 0.0;

        while (true)
        {
            var currentTime = sw.Elapsed.TotalSeconds;
            var deltaTime = currentTime - lastFrameTime;
            lastFrameTime = currentTime;

            totalTime += deltaTime;
            if (totalTime >= 1.0)
            {
                Console.WriteLine("UPS: " + updates.ToString("N0"));
                totalTime = 0.0;
                updates = 0;
            }

            TCPClient.ProcessBufferQueue();

            accumulator += deltaTime;
            while (accumulator >= frameTime)
            {
                updates++;
                Update(accumulator);
                accumulator -= frameTime;
            }
        }
    }

    public void Update(double dt)
    {
    }

    public void Dispose()
    {

    }
}
