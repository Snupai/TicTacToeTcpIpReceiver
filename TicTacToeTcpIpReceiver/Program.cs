using CommandLine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Console;

namespace TicTacToeTcpIpReceiver
{
    internal class Program
    {
        /// <summary>
        /// Represents the options for the program.
        /// </summary>
        class Options
        {
            /// <summary>
            /// Gets or sets the port to listen on.
            /// </summary>
            [Option('p', "port", Required = false, Default = 50000, HelpText = "Port to listen on")]
            public int Port { get; set; }
        }

        /// <summary>
        /// Main entry point of the program
        /// </summary>
        /// <param name="args">Command line arguments</param>
        static async Task Main(string[] args)
        {
            StartServer(args);
            await Task.Delay(-1);
        }

        /// <summary>
        /// Starts the server with the given command line arguments.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
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

            /// <summary>
            /// Initializes a new instance of the Server class with the specified port.
            /// </summary>
            /// <param name="port">The port number to listen on.</param>
            public Server(int port)
            {
                iPEndPoint = new IPEndPoint(IPAddress.Any, port);
                listener = new TcpListener(iPEndPoint);
                StartAsync();
                WriteLine($"Server started on port {port}");
            }

            private async Task StartAsync()
            {
                try
                {
                    // Start listening for incoming connections
                    listener.Start();

                    while (true)
                    {
                        // Create a 3x3 board for the game
                        char[,] board = new char[3, 3];
                        // Accept an incoming TCP client connection
                        using TcpClient handler = await listener.AcceptTcpClientAsync();

                        WriteLine($"Client connected {handler.Client.RemoteEndPoint}");
                        // Get the network stream for reading and writing
                        await using NetworkStream stream = handler.GetStream();

                        // Read the incoming message from the client
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

                        // Display the game board
                        ShowOnDisplay.DrawBitmap(board);

                        // Get the current time and send it back to the client
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
