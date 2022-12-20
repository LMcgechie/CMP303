using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class Client
{
    public int id;
    public TCP tcp;
    public UDP udp;

    public Player player;

    public Client(int newId)
    {
        id = newId;
        tcp = new TCP(id);
        udp = new UDP(id);
    }

    public class TCP
    {
        public TcpClient socket;
        private readonly int id;
        private NetworkStream stream;
        private byte[] receiveBuffer;
        private Packet receivedData;

        public TCP(int newId)
        {
            id = newId;
        }

        public void Connect(TcpClient newSocket)
        {
            socket = newSocket;
            socket.ReceiveBufferSize = 4096;
            socket.SendBufferSize = 4096;

            stream = socket.GetStream();

            receivedData = new Packet();
            receiveBuffer = new byte[4096];
            stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallBack, null);

            PacketSender.Welcome(id, "Connected!");
        }

        public void SendData(Packet packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"Error Sending data to player {id} via TCP: {ex}");
            }
        }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;

            receivedData.SetBytes(data);

            if (receivedData.UnreadLength() >= 4)
            {
                packetLength = receivedData.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }

            while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
            {
                byte[] packetBytes = receivedData.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        Server.packetHandlers[packetId](id, packet);
                    }
                });

                packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        private void ReceiveCallBack(IAsyncResult newResult)
        {
            try
            {
                int byteLength = stream.EndRead(newResult);
                if (byteLength <= 0)
                {
                    Server.clients[id].Disconnect();
                    return;
                }
                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                receivedData.Reset(HandleData(data));
                stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallBack, null);
            }
            catch (Exception ex)
            {
                Debug.Log($"Error receiving data: {ex}");
                Server.clients[id].Disconnect();
            }
        }


        public void Disconnect()
        {
            socket.Close();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public IPEndPoint endPoint;
        private int id;
        public UDP(int newId)
        {
            id = newId;
        }

        public void Connect(IPEndPoint newEndPoint)
        {
            endPoint = newEndPoint;
        }

        public void SendData(Packet packet)
        {
            Server.SendUDPData(endPoint, packet);
        }

        public void HandleData(Packet packetData)
        {
            int packetLength = packetData.ReadInt();
            byte[] packetbytes = packetData.ReadBytes(packetLength);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet packet = new Packet(packetbytes))
                {
                    int packetId = packet.ReadInt();
                    Server.packetHandlers[packetId](id, packet);
                }
            });
        }

        public void Disconnect()
        {
            endPoint = null;
        }
    }

    public void SendIntoGame(string playerName)
    {
        player = NetworkManager.instance.InstantiatePlayer();
        player.Initialize(id, playerName, Vector3.zero);

        foreach (Client clients in Server.clients.Values)
        {
            if (clients.player != null)
            {
                if (clients.id != null)
                {
                    if (clients.id != id)
                    {
                        PacketSender.SpawnPlayer(id, clients.player);
                    }
                }
            }
        }
        foreach (Client clients in Server.clients.Values)
        {
            if (clients.player != null)
            {
                PacketSender.SpawnPlayer(clients.id, player);
            }
        }
    }

    private void Disconnect()
    {
        UnityEngine.Object.Destroy(player.gameObject);
        tcp.Disconnect();
        udp.Disconnect();
    }
}
