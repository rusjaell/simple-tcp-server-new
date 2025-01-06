using System;
using System.Net;
using System.Net.Sockets;

namespace Shared.Networking;

public abstract class TCPSocketBase
{
    protected readonly IPEndPoint _ipEndPoint;
    protected readonly Socket _socket;

    protected NetworkReceiveHandler _networkReceiveHandler;
    protected NetworkSendHandler _networkSendHandler;

    public bool Disconnected { get; private set; }

    public TCPSocketBase(Socket socket)
    {
        _ipEndPoint = (IPEndPoint)socket.RemoteEndPoint;
        _socket = socket;
        _socket.NoDelay = true;

        _networkReceiveHandler = new NetworkReceiveHandler(this, _socket);
        _networkSendHandler = new NetworkSendHandler(this, _socket);
    }

    public TCPSocketBase(IPEndPoint ipEndPoint, bool initializeWithHandlers = true)
    {
        _ipEndPoint = ipEndPoint;
        _socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _socket.NoDelay = true;

        if (initializeWithHandlers)
        {
            _networkReceiveHandler = new NetworkReceiveHandler(this, _socket);
            _networkSendHandler = new NetworkSendHandler(this, _socket);
        }
    }

    public void Send(ref NetworkBufferWriter writer) => _networkSendHandler.Send(ref writer);

    public virtual void OnDisconnect()
    {
    }

    public void Disconnect() => Disconnect(string.Empty);
    public void Disconnect(string reason)
    {
        if (Disconnected)
            return;
        Disconnected = true;

        if(!string.IsNullOrWhiteSpace(reason) && reason.Length > 0)
            Console.WriteLine("Disconnected: " + reason);

        OnDisconnect();

        try
        {
            _socket.Close();
        }
        catch
        {
        }
    }
} 
