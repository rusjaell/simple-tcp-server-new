using Shared.Networking;
using System;
using System.Net.Sockets;

namespace Server.Network;

public sealed class TCPSession : TCPSocketBase
{
    public int Id { get; private set; }

    public TCPSession(int id, Socket socket)
        : base(socket)
    {
        Id = id;
    }

    public void ProcessBufferQueue()
    {
        while (_networkReceiveHandler.BufferQueue.TryDequeue(out var data))
        {
            var reader = new BufferReader(data);

            var type = reader.ReadByte();

            switch (type)
            {
                case 0:
                    {
                        var a = reader.ReadInt16();
                        var b = reader.ReadString();

                        if (reader.Error)
                        {
                            Console.WriteLine("Case 0 Errored");
                            break;
                        }

                        Console.WriteLine(a + " " + b);
                    }
                    break;

                case 1:
                    {
                        var a = reader.ReadBoolean();
                        var b = reader.ReadInt32();

                        if (reader.Error)
                        {
                            Console.WriteLine("Case 1 Errored");
                            break;
                        }

                        Console.WriteLine(a + " " + b);
                    }
                    break;
            }
        }
    }

    public override void OnDisconnect()
    {

    }
}