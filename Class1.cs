using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PRI_lab5_Server
{
    public class Program
    {
        static HashSet<TcpClient> TcpClients = new HashSet<TcpClient>();

        static void Main(string[] args)
        {
            IPAddress localAddr = IPAddress.Parse("192.168.43.11");
            TcpListener server = new TcpListener(localAddr, 8888);
            server.Start();
            Console.WriteLine("Сервер запущен. Ожидание подключений... ");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                TcpClients.Add(client);
                new Thread(() =>  ProcessClient(client)).Start();

            }
            async void ProcessClient(TcpClient tcpClient)
            {
                try
                {
                    NetworkStream streamClient = tcpClient.GetStream();
                    while (true)
                    {
                        byte[] bytes = new byte[4];
                        await streamClient.ReadAsync(bytes, 0, 4);
                        int messageLength = BitConverter.ToInt32(bytes, 0);

                        byte[] buffer = new byte[messageLength];
                        int bytes2 = streamClient.Read(buffer, 0, messageLength);
                        var message = Encoding.UTF8.GetString(buffer, 0, messageLength);
                        message = tcpClient.Client.RemoteEndPoint + ": " + message;
                        SendEverybody(message);
                    }
                }catch(Exception ex)
                {
                    TcpClients.Remove(tcpClient);
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(tcpClient.Client.RemoteEndPoint + " - удален из списка клиентов");
                }
            }
            void SendEverybody(string message)
            {
                foreach (var client in TcpClients)
                {
                    NetworkStream stream = client.GetStream();

                    stream.Write(BitConverter.GetBytes(message.Length), 0, 4);
                    stream.Write(Encoding.UTF8.GetBytes(message),0, message.Length);
             
                    Console.WriteLine("Передано:" + client.Client.RemoteEndPoint + " " + message);
                }
            }
        }
    }
}
