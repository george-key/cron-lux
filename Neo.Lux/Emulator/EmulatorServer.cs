using LunarParser;
using LunarParser.JSON;
using Neo.Lux.Utils;
using Neo.Lux.VM;
using System;
using System.IO;
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
            Byte[] bytes = new Byte[1024*64];

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
                    NetworkStream netStream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    var sb = new StringBuilder();
                    do
                    {
                        var len = netStream.Read(bytes, 0, bytes.Length);
                        if (len == 0)
                        {
                            break;
                        }

                        var str = Encoding.ASCII.GetString(bytes, 0, len);
                        sb.Append(str);

                        if (len < bytes.Length)
                        {
                            break;
                        }
                    } while (true);

                    var temp = sb.ToString();
                    Console.WriteLine(temp);

                    var split = temp.Split('/');
                    var method = split[0];
                    var val = split[1];
                    val = val.Substring(0, val.Length - 1);
                    
                    var result = DataNode.CreateObject("response");

                    switch (method)
                    {
                        case "GetChainHeight":
                            {
                                result.AddField("height", this.emulator.GetBlockHeight());
                                break;
                            }

                        case "InvokeScript":
                            {
                                var script = val.HexToBytes();

                                var obj = emulator.InvokeScript(script);

                                using (var wstream = new MemoryStream())
                                {
                                    using (var writer = new BinaryWriter(wstream))
                                    {
                                        Serialization.SerializeStackItem(obj.result, writer);

                                        var hex = wstream.ToArray().ByteToHex();

                                        result.AddField("state", obj.state);
                                        result.AddField("gas", obj.gasSpent);
                                        result.AddField("stack", hex);
                                    }
                                }

                                break;
                            }

                        default:
                            {
                                result.AddField("error", "invalid method");
                                break;
                            }
                    }

                    var json = JSONWriter.WriteToString(result);

                    Console.WriteLine(json);

                    var output = Encoding.UTF8.GetBytes(json);
                    netStream.Write(output, 0, output.Length);
                    
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
