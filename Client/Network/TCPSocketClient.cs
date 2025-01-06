using Shared.Networking;
using System.Net;

namespace Client.Network;

public sealed class TCPSocketClient : TCPSocketBase
{
    public TCPSocketClient(string host, int port)
        : base(new IPEndPoint(IPAddress.Parse(host), port))
    {
    }

    public void Connect() => _socket.Connect(_ipEndPoint);
}