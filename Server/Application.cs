using Server.Network;
using System;
using System.Diagnostics;

namespace Server;

public sealed class Application : IDisposable
{
    public TCPSocketServer TCPSocketServer = new TCPSocketServer(2050, 0xFF);

    public Application(string[] args)
    {
    }

    public void Initialize()
    {
        _ = TCPSocketServer.StartAsync();
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

            TCPSocketServer.ProcessSessionBuffers();

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
        // any application logic here
    }

    public void Dispose()
    {
        TCPSocketServer.DisconnectSessions();
    }
}
