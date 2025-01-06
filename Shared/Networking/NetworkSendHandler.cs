using System;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Shared.Networking;

public sealed class NetworkSendHandler
{
    public const int BUFFER_SIZE = 131072;
    public const int QUEUE_BUFFER_SIZE = BUFFER_SIZE * 2;

    enum SendState
    {
        Awaiting,
        Ready,
        Sending
    }

    private readonly TCPSocketBase _socketBase;
    private readonly Socket _socket;

    private readonly SocketAsyncEventArgs _saea = new SocketAsyncEventArgs();
    private readonly object _sendLock = new object();
    private SendState _sendState = SendState.Awaiting;
    private readonly ConcurrentQueue<byte[]> _pendingData = new ConcurrentQueue<byte[]>();

    private int _bytesWritten;
    private int _bytesTransfered;
    private byte[] _sendBuffer = new byte[BUFFER_SIZE];
    private byte[] _sendQueueBuffer = new byte[QUEUE_BUFFER_SIZE]; 

    public NetworkSendHandler(TCPSocketBase socketBase, Socket socket)
    {
        _socketBase = socketBase;
        _socket = socket;

        _saea.SetBuffer(_sendBuffer, 0, _sendBuffer.Length);
        _saea.Completed += OnCompleted;
    }

    public void Send(ref NetworkBufferWriter writer)
    {
        if (_socketBase.Disconnected)
            return;

        var b = writer.ToArray();
        for (var i = 0; i < ushort.MaxValue; i++)
            _pendingData.Enqueue(b);

        TrySend();
    }

    private void SendBatch()
    {
        if (_socketBase.Disconnected)
            return;

        // send no more than the buffer size 
        // will start batching
        var bytesToSend = _bytesWritten > BUFFER_SIZE ? BUFFER_SIZE : _bytesWritten;
        
        _saea.SetBuffer(0, bytesToSend);
        Buffer.BlockCopy(_sendQueueBuffer, _bytesTransfered, _sendBuffer, 0, bytesToSend);

        _sendState = SendState.Sending;

        var willRaiseEvent = _socket.SendAsync(_saea);
        if (!willRaiseEvent)
            OnCompleted(null, _saea);
    }

    private void OnCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (_socketBase.Disconnected)
        {
            _bytesWritten = 0;
            _bytesTransfered = 0;
            return;
        }

        if (e.SocketError != SocketError.Success)
        {
            _socketBase.Disconnect($"Send SocketError = {e.SocketError}");
            return;
        }

        _bytesTransfered += e.BytesTransferred;
        _bytesWritten -= _bytesTransfered;

        if (_bytesWritten <= 0)
        {
            _bytesWritten = 0;
            _bytesTransfered = 0;

            _sendState = SendState.Awaiting;
            TrySend();
            return;
        }

        SendBatch();
    }

    private void TrySend()
    {
        if (!PrepareBatch())
            return;
        
        SendBatch();
    }

    private bool PrepareBatch()
    {
        lock (_sendLock)
        {
            if (_sendState != SendState.Awaiting)
                return false;

            _bytesWritten = 0;
            _bytesTransfered = 0;

            // preserve send order with peek
            while (_pendingData.TryPeek(out var buffer))
            {
                var bufferLength = buffer.Length;

                if (bufferLength > _sendQueueBuffer.Length - _bytesWritten)
                {
                    _sendState = SendState.Ready;
                    return true;
                }

                // pop it
                _pendingData.TryDequeue(out _);
                
                Buffer.BlockCopy(buffer, 0, _sendQueueBuffer, _bytesWritten, bufferLength);

                _bytesWritten += bufferLength;
            }

            if (_bytesWritten <= 0)
                return false;

            _sendState = SendState.Ready;
            return true;
        }
    }
}