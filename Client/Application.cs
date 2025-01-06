using Client.Network;
using Shared.Networking;
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
        var writer = new NetworkBufferWriter();
        writer.WriteByte(0);
        writer.WriteInt16((short)Random.Shared.Next(0, short.MaxValue));
        writer.WriteString("Test String");

        TCPClient.Send(ref writer);

        writer = new NetworkBufferWriter();
        writer.WriteByte(1);
        writer.WriteBoolean(Random.Shared.NextDouble() > 0.5);
        writer.WriteInt32(Random.Shared.Next());
        writer.WriteString("Another String");

        TCPClient.Send(ref writer);
    }

    public void Dispose()
    {

    }
}
