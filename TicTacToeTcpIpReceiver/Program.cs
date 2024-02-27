﻿using CommandLine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Console;

namespace TicTacToeTcpIpReceiver
{
    internal class Program
    {
        class Options
        {
            [Option('p', "port", Required = false, Default = 50000, HelpText = "Port to listen on")]
            public int Port { get; set; }
        }

        static async Task Main(string[] args)
        {
            StartServer(args);
            await Task.Delay(-1);
        }

        static async void StartServer(string[] args)
        {
            var options = new Options();
            Parser.Default.ParseArguments<Options>(args)
                  .WithParsed<Options>(o => options = o)
                  .WithNotParsed<Options>(e => Environment.Exit(42));

            Server server = new(options.Port);

            await Task.Delay(-1);
        }

        public class Server
        {
            readonly IPEndPoint iPEndPoint;
            readonly TcpListener listener;

            public Server(int port)
            {
                iPEndPoint = new IPEndPoint(IPAddress.Any, port);
                listener = new(iPEndPoint);
                StartAsync();
                WriteLine($"Server started on port {port}");
            }

            private async Task StartAsync()
            {
                try
                {
                    listener.Start();

                    while (true)
                    {
                        char[,] board = new char[3, 3];
                        using TcpClient handler = await listener.AcceptTcpClientAsync();

                        WriteLine($"Client connected {handler.Client.RemoteEndPoint}");
                        await using NetworkStream stream = handler.GetStream();

                        byte[] byteBuffer = new byte[1024];
                        int anzBytesEmpfangen = await stream.ReadAsync(byteBuffer);
                        string empfangeneNachricht = Encoding.UTF8.GetString(byteBuffer);
                        WriteLine($"{anzBytesEmpfangen} received: {empfangeneNachricht}");
                        string[] empfangenes = empfangeneNachricht.Split('\n');
                        for (int i = 0; i < empfangenes.Length; i++)
                        {
                            string[] empfangeneChars = empfangenes[i].Split(';');
                            for (int j = 0; j < empfangeneChars.Length; j++)
                            {
                                board[i, j] = empfangeneChars[j][0];
                            }
                        }

                        ShowOnDisplay.DrawBitmap(board);

                        string currentTime = $"{DateTime.Now}";
                        byte[] currentTimeBytes = Encoding.UTF8.GetBytes(currentTime);
                        await stream.WriteAsync(currentTimeBytes);

                        WriteLine($"Sent: \"{currentTime}\"");
                    }
                }
                catch (Exception e)
                {
                    WriteLine($"Error: {e.Message}");
                }
            }
        }
    }

}
