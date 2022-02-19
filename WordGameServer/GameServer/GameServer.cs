using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using WordGameServer.Common;

namespace WordGameServer.GameServer
{
    public class GameServer
    {
        private readonly int                 _port;
        private readonly IPAddress           _ipAddress;
        private          TcpListener         _tcpListener;
        private          GameLogic.GameLogic _gameLogic;
        private          Regex               _clientRequestRegex;

        public GameServer(int port, string ipAddress)
        {
            _port      = port;
            _gameLogic = new GameLogic.GameLogic();
            _ipAddress = IPAddress.Parse(ipAddress);
            _clientRequestRegex =
                new Regex(
                    "Request ID: (?<requestId>\\d+) Request Command: (?<requestCommand>\\d+) Player Identifier: (?<playerId>\\w+) Payload: (?<payload>\\w+)");
        }


        public void StartServer()
        {
            // TcpListener server = new TcpListener(port);
            _tcpListener = new TcpListener(_ipAddress, _port);

            // Start listening for client requests.
            _tcpListener.Start();

            // Enter the listening loop.
            while (true)
            {
                Console.WriteLine(
                    $"THREAD ID: {Thread.CurrentThread.ManagedThreadId} - Server is ready to receive a connection At: {_ipAddress}:{_port}");

                // Perform a blocking call to accept requests.
                // You could also use server.AcceptSocket() here.
                TcpClient client = _tcpListener.AcceptTcpClient();
                Console.WriteLine($"THREAD ID: {Thread.CurrentThread.ManagedThreadId} - Connected!");
                ThreadPool.QueueUserWorkItem(state => HandleClientConnection(client));
            }
        }


        private void HandleClientConnection(TcpClient client)
        {
            Console.WriteLine(
                $"Starting Communication with new client in thread: {Thread.CurrentThread.ManagedThreadId}");
            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data  = null;

            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();

            int i;

            // Loop to receive all the data sent by the client.
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                // Translate data bytes to a ASCII string.
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                Console.WriteLine($"THREAD ID: {Thread.CurrentThread.ManagedThreadId} Received: {data}");

                var response = HandleClientRequest(data);
                // // Process the data sent by the client.
                // data =
                //     $"Hello from server! Got it! The user guess is: {data.Replace("Hello from Unity! Player guess is ", "")}";

                byte[] msg = Encoding.ASCII.GetBytes(response);

                // Send back a response.
                stream.Write(msg, 0, msg.Length);
                Console.WriteLine("Sent: {0}", response);
            }

            // Shutdown and end connection
            client.Close();
        }

        private string HandleClientRequest(string clientRequestString)
        {
            var threadId = $"THREAD ID: {Thread.CurrentThread.ManagedThreadId}";

            var requestStruct = new CommunicationStruct();
            var replyStruct   = new CommunicationStruct();
            
            requestStruct.FromString(clientRequestString);
            var reply = "";


            switch (requestStruct.RequestCommand)
            {
                //command to choose a new word to guess
                case 0:
                    var newWordToGuess = _gameLogic.PickWordToGuess(requestStruct.PlayerIdentifier);
                    Console.WriteLine(
                        $"{threadId} - Chose a new word for player: {requestStruct.PlayerIdentifier}. Word is: {newWordToGuess}");
                    replyStruct.RequestId        = requestStruct.RequestId;
                    replyStruct.RequestCommand   = 0; //0 means success
                    replyStruct.Payload          = newWordToGuess;
                    replyStruct.PlayerIdentifier = requestStruct.PlayerIdentifier;


                    reply = replyStruct.ToString();

                    break;
                //command to evaluate guess
                case 1:
                    Console.WriteLine(
                        $"{threadId} - Evaluation Guess: {requestStruct.Payload}. For Player: {requestStruct.PlayerIdentifier}");
                    var evalResults =
                        _gameLogic.EvaluateGuess(requestStruct.PlayerIdentifier, requestStruct.Payload);

                    replyStruct.RequestId        = requestStruct.RequestId;
                    replyStruct.RequestCommand   = 0; //0 means success
                    replyStruct.Payload          = evalResults;
                    replyStruct.PlayerIdentifier = requestStruct.PlayerIdentifier;

                    reply = replyStruct.ToString();

                    break;
                //command to check if the word exist in the dictionary
                case 2:
                    Console.WriteLine(
                        $"{threadId} - Checking if word {requestStruct.Payload} exist in the dictionary for player {requestStruct.PlayerIdentifier}");
                    var result = _gameLogic.CheckIsViableWord(requestStruct.Payload);

                    replyStruct.RequestId        = requestStruct.RequestId;
                    replyStruct.RequestCommand   = 0; //0 means success
                    replyStruct.Payload          = result.ToString();
                    replyStruct.PlayerIdentifier = requestStruct.PlayerIdentifier;

                    reply = replyStruct.ToString();
                    break;


                default:
                    Console.WriteLine(
                        $"{threadId} - Command {requestStruct.RequestCommand} is an unknown command! ");
                    break;
            }

            return reply;
        }
    }
}