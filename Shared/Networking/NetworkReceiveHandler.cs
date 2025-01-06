using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Shared.Networking;

public sealed class NetworkReceiveHandler
{
    private const byte HEADER_SIZE = 4;
    private const int MAX_BUFFER_SIZE = 131072;
    private const int MAX_BODY_BUFFER_SIZE = short.MaxValue;

    private readonly TCPSocketBase _network;

    private byte[] _receiveBuffer = new byte[MAX_BUFFER_SIZE];
    private SocketAsyncEventArgs _recvSAEA = new SocketAsyncEventArgs();

    public readonly ConcurrentQueue<byte[]> BufferQueue = new ConcurrentQueue<byte[]>();

    public NetworkReceiveHandler(TCPSocketBase session, Socket socket)
    {
        _network = session;

        _recvSAEA.AcceptSocket = socket;
        _recvSAEA.SetBuffer(_receiveBuffer, 0, _receiveBuffer.Length);
        _recvSAEA.Completed += OnReceiveCompleted;

        _ = _recvSAEA.AcceptSocket.ReceiveAsync(_recvSAEA);
    }

    private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
    {
        try
        {
            if (_network.Disconnected)
                return;

            var socket = e.AcceptSocket;
            if (!socket.Connected)
            {
                _network.Disconnect("OnReceiveCompleted: !Socket.Connected");
                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                _network.Disconnect($"OnReceiveCompleted: e.SocketError != SocketError.Success");
                return;
            }

            if (e.BytesTransferred == 0)
            {
                _network.Disconnect($"OnReceiveCompleted: e.BytesTransferred == 0");
                return;
            }

            var memoryBuffer = e.MemoryBuffer.Span;

            var bytesRead = 0;

            var bytesTransferred = e.BytesTransferred;
            var remainingBytes = e.Offset + bytesTransferred - bytesRead;

            while (remainingBytes > 0)
            {
                if (remainingBytes < HEADER_SIZE)
                    break;

                var packetSize = BinaryPrimitives.ReadInt32LittleEndian(memoryBuffer.Slice(bytesRead));
                if (packetSize <= 0 || packetSize > MAX_BODY_BUFFER_SIZE)
                {
                    _network.Disconnect($"Invalid packet size detected: {packetSize}");
                    return;
                }

                if (remainingBytes < packetSize)
                    break;

                var buffer = new byte[packetSize];
                memoryBuffer.Slice(bytesRead, packetSize).CopyTo(buffer);

                BufferQueue.Enqueue(buffer);

                bytesRead += packetSize;
                remainingBytes -= packetSize;
            }

            if (remainingBytes > 0)
            {
                memoryBuffer.Slice(bytesRead, remainingBytes).CopyTo(memoryBuffer);
                e.SetBuffer(remainingBytes, memoryBuffer.Length - remainingBytes);
            }
            else
                e.SetBuffer(0, memoryBuffer.Length);

            if (!socket.ReceiveAsync(e))
                OnReceiveCompleted(this, e);
        }
        catch (Exception ex)
        {
            _network.Disconnect($"OnReceiveCompleted: {ex.StackTrace} {ex.Message}");
        }
    }
}
