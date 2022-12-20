using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class Server
{
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }
    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public static TcpListener tcpListener;
    public static UdpClient udpClient;

    public delegate void PacketHandler(int fromClien, Packet packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    public static void Start(int newMaxPlayers, int newPort)
    {
        MaxPlayers = newMaxPlayers;
        Port = newPort;

        InitServerData();
        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPCallBack), null);

        udpClient = new UdpClient(Port);
        udpClient.BeginReceive(UDPCallBack, null);

        Debug.Log($"Server Stareted on {Port}");
    }

    private static void TCPCallBack(IAsyncResult result)
    {
        TcpClient client = tcpListener.EndAcceptTcpClient(result);
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPCallBack), null);


        Debug.Log($"Incoming connection form {client.Client.RemoteEndPoint}");
        for (int i = 1; i <= MaxPlayers; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(client);
                return;
            }
        }
    }

    private static void UDPCallBack(IAsyncResult result)
    {
        try
        {
            IPEndPoint clientEndPont = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpClient.EndReceive(result, ref clientEndPont);
            udpClient.BeginReceive(UDPCallBack, null);
            if (data.Length < 4)
            {
                return;
            }

            using (Packet packet = new Packet(data))
            {
                int clientId = packet.ReadInt();

                if (clients[clientId].udp.endPoint == null)
                {
                    clients[clientId].udp.Connect(clientEndPont);
                    return;
                }

                if (clients[clientId].udp.endPoint.ToString() == clientEndPont.ToString())
                {
                    clients[clientId].udp.HandleData(packet);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log("Error receiving UDP data");
        }
    }

    public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
    {
        try
        {
            if (clientEndPoint != null)
            {
                udpClient.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("Error Sending data");
        }
    }

    private static void InitServerData()
    {
        for (int i = 1; i <= MaxPlayers; i++)
        {
            clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ClientPackets.welcomeReceived, ServerHandler.WelcomeReceived },
            { (int)ClientPackets.playerMove, ServerHandler.PlayerMovement }
        };
        Debug.Log("Initialised packets");
    }
}

