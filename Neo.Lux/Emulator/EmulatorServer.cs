using LunarParser.JSON;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Neo.Lux.Emulator
{
    public class EmulatorServer
    {
        private Emulator emulator;
        private TcpListener server;

        public EmulatorServer(Emulator emulator, int port)
        {
            this.emulator = emulator;

            // TcpListener server = new TcpListener(port);
            server = new TcpListener(IPAddress.Any, port);
        }

        public void Start() { 

            // Start listening for client requests.
            server.Start();

            // Buffer for reading data
            Byte[] bytes = new Byte[256];

            // Enter the listening loop.
            while (true)
            {
                try
                {

                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    var sb = new StringBuilder();
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var str = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        sb.Append(str);
                    }

                    var json = sb.ToString();
                    var root = JSONReader.ReadFromString(json);

                    var method = root.GetString("method");

                    string result = null;

                    switch (method)
                    {
                        case "getaccountstate":
                            {
                                break;
                            }

                        case "getstorage":
                            {
                                break;
                            }

                        case "sendrawtransaction":
                            {
                                break;
                            }

                        case "invokescript":
                            {
                                break;
                            }

                        case "getrawtransaction":
                            {
                                break;
                            }

                        case "getblockcount":
                            {
                                break;
                            }

                        case "getblock":
                            {
                                break;
                            }
                    }

                    var output = Encoding.ASCII.GetBytes(result);
                    stream.Write(output, 0, output.Length);

                    client.Close();
                }
                catch (SocketException e)
                {
                    Console.WriteLine("SocketException: {0}", e);
                }
                
            }
           // Stop listening for new clients.
           server.Stop();
        }
    }
}
