using Shared.Networking;
using Shared.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Network;

public sealed class TCPSocketServer : TCPSocketBase
{
    private int _nextSessionId;
    
    private readonly Lock _sessionLock = new Lock();
    private readonly Dictionary<int, TCPSession> _sessions = new Dictionary<int, TCPSession>();

    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public TCPSocketServer(int port, int backlog)
        : base(new IPEndPoint(IPAddress.Any, port))
    {
        _socket.Bind(_ipEndPoint);
        _socket.Listen(backlog);
    }

    public async Task StartAsync()
    {
        try
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var socket = await _socket.AcceptAsync();

                Console.WriteLine("New Session Socket");

                using (_sessionLock.EnterScope())
                {
                    var session = new TCPSession(_nextSessionId++, socket);

                    _sessions.Add(session.Id, session);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public void ProcessSessionBuffers()
    {
        using var t = new ScopedTimer("ProcessSessionBuffers");

        var handled = 0;

        using (_sessionLock.EnterScope())
        {
            foreach (var session in _sessions.Values)
            {
                if (session.Disconnected)
                {
                    _ = _sessions.Remove(session.Id);
                    continue;
                }

                session.HandleBufferQueue(ref handled);
            }
        }

        if(handled > 0)
            Console.WriteLine($"Handled: {handled}");
    }

    public void DisconnectSessions()
    {
        using (_sessionLock.EnterScope())
        {
            foreach (var session in _sessions.Values)
                session.Disconnect("Disconnected Sessions");
        }
    }
}