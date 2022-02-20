using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using WordGameServer.Common;
using WordGameServer.Common.NetworkCommandCodes;

namespace WordGameServer.GameServer
{
    /// <summary>
    /// Class that houses the game server.
    /// </summary>
    public class GameServer
    {
        private readonly int                 _port;
        private readonly IPAddress           _ipAddress;
        private          TcpListener         _tcpListener;
        private readonly GameLogic.GameLogic _gameLogic;

        public GameServer(int port, string ipAddress)
        {
            _port      = port;
            _gameLogic = new GameLogic.GameLogic();
            _ipAddress = IPAddress.Parse(ipAddress);
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

                // Establishing connection with the client and passing that connection to a thread pool.
                // This solution isn't scalable but it should be fine for a small number of concurrent clients.
                var client = _tcpListener.AcceptTcpClient();
                Console.WriteLine($"THREAD ID: {Thread.CurrentThread.ManagedThreadId} - Connected!");
                ThreadPool.QueueUserWorkItem(state => HandleClientConnection(client));
            }
        }


        private void HandleClientConnection(TcpClient client)
        {
            Console.WriteLine(
                $"Starting Communication with new client in thread: {Thread.CurrentThread.ManagedThreadId}");
            // Buffer for reading data
            var bytes = new Byte[256];

            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();

            int i;

            // Loop to receive all the data sent by the client.
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                // Translate data bytes to a ASCII string.
                var data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                Console.WriteLine($"THREAD ID: {Thread.CurrentThread.ManagedThreadId} Received: {data}");

                var requestStruct = new CommunicationStruct();


                //todo: implement a failure path where parsing from string fails.
                //parse request struct from received string.
                requestStruct.FromString(data);
                var response = HandleClientRequest(requestStruct);


                //serialize reply struct to string
                var replyString = response.ToString();

                byte[] msg = Encoding.ASCII.GetBytes(replyString);

                // Send back a response.
                stream.Write(msg, 0, msg.Length);
                Console.WriteLine("Sent: {0}", response);

                if (requestStruct.RequestCommand == NetworkClientRequestCommandCodes.REQUEST_DISCONNECT)
                {
                    Console.WriteLine(
                        $"THREAD ID: {Thread.CurrentThread.ManagedThreadId} Client is requesting a disconnect... Disconnecting");
                    break;
                }
            }

            // Shutdown and end connection
            Console.WriteLine($"THREAD ID: {Thread.CurrentThread.ManagedThreadId} - Closing connection...");
            stream.Close();
            client.Close();
        }

        private CommunicationStruct HandleClientRequest(CommunicationStruct requestStruct)
        {
            var threadId = $"THREAD ID: {Thread.CurrentThread.ManagedThreadId}";


            //we initialize the reply struct with a failed status. So unless we override it with relevant data and
            //success status, we send back a failed reply.
            var replyStruct = new CommunicationStruct()
            {
                RequestId        = requestStruct.RequestId,
                RequestCommand   = NetworkServerReplyCommandCodes.REQUEST_FAILURE,
                Payload          = "DONTCARE",
                PlayerIdentifier = requestStruct.PlayerIdentifier
            };


            //todo: try put the repetitive code from each case in a function.
            switch (requestStruct.RequestCommand)
            {
                //command to choose a new word to guess
                case NetworkClientRequestCommandCodes.GUESS_NEW_WORD:
                    var newWordToGuess = _gameLogic.PickWordToGuess(requestStruct.PlayerIdentifier);
                    Console.WriteLine(
                        $"{threadId} - Chose a new word for player: {requestStruct.PlayerIdentifier}. Word is: {newWordToGuess}");

                    replyStruct.RequestCommand = NetworkServerReplyCommandCodes.REQUEST_SUCCESS;
                    replyStruct.Payload        = newWordToGuess;


                    break;
                //command to evaluate guess
                case NetworkClientRequestCommandCodes.EVALUATE_GUESS:
                    Console.WriteLine(
                        $"{threadId} - Evaluation Guess: {requestStruct.Payload}. For Player: {requestStruct.PlayerIdentifier}");
                    var evalResults =
                        _gameLogic.EvaluateGuess(requestStruct.PlayerIdentifier, requestStruct.Payload);

                    replyStruct.RequestCommand = NetworkServerReplyCommandCodes.REQUEST_SUCCESS;
                    replyStruct.Payload        = evalResults;


                    break;
                //command to check if the word exist in the dictionary
                case NetworkClientRequestCommandCodes.CHECK_WORD_EXISTS:
                    Console.WriteLine(
                        $"{threadId} - Checking if word {requestStruct.Payload} exist in the dictionary for player {requestStruct.PlayerIdentifier}");
                    var result = _gameLogic.CheckIsViableWord(requestStruct.Payload);

                    replyStruct.RequestCommand = NetworkServerReplyCommandCodes.REQUEST_SUCCESS;
                    replyStruct.Payload        = result.ToString();
                    break;

                case NetworkClientRequestCommandCodes.REQUEST_DISCONNECT:
                    Console.WriteLine(
                        $"{threadId} - Client for player: '{requestStruct.PlayerIdentifier}' Requests a disconnect...");
                    replyStruct.RequestCommand = NetworkServerReplyCommandCodes.REQUEST_SUCCESS;
                    break;

                default:
                    Console.WriteLine(
                        $"{threadId} - Command {requestStruct.RequestCommand} is an unknown command!  ");
                    break;
            }


            return replyStruct;
        }
    }
}