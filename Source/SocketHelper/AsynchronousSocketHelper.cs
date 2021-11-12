using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Timers;

namespace AsynchronousSocketHelper
{
    public abstract class AsynchronousSocketServer
    {
        private bool isListening = false;
        public bool IsListening { get => isListening; }

        public int ClientsConnected { get; private set; } = 0;
        
        public string Ipv4 { get; protected set; } = "";
        public int Port { get; set; } = 28102;

        private List<Socket> clients = new List<Socket>();
        Mutex mutex = new Mutex();

        Socket listenerSocket;

        // Thread signal.  
        public ManualResetEvent allDone = new ManualResetEvent(false);
        
        /// <summary>
        /// This method is invoked when the server received a message
        /// 
        /// </summary>
        /// <param name="message"> raw message received </param>
        /// <returns> string that will be echoed to the client </returns>
        protected abstract void MessageReceived(string message, ClientHeader header, Socket handler);
        
        public AsynchronousSocketServer(int port)
        {
            Port = port;
        }

        public virtual void StartListening()
        {
            if (isListening)
                return;

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                // Establish the local endpoint for the socket.  
                // The DNS name of the computer  

                IPHostEntry ipHostEntry = Dns.Resolve(Dns.GetHostName());
                IPAddress ipAddress = ipHostEntry.AddressList[0];

                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Port);
                

                // Create a TCP/IP socket.  
                listenerSocket = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);


                listenerSocket.Bind(localEndPoint);
                listenerSocket.Listen(100);

                Ipv4 = ipAddress.MapToIPv4().ToString();

                // Check for clients connection
                var clientsTimer = new System.Timers.Timer(1000);
                clientsTimer.Enabled = true;
                clientsTimer.Elapsed += ClientsTimer_Elapsed;
                clientsTimer.Start();
                
                isListening = true;
                while (isListening)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Debug.WriteLine($"Server is listening at {ipAddress}");
                    Debug.WriteLine("Waiting for a connection...");
                    listenerSocket.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listenerSocket);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                isListening = false;
            }

        }

        private void ClientsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            mutex.WaitOne();
            ClientsConnected = 0;
            mutex.ReleaseMutex();

            for (int i = 0; i < clients.Count; i++)
            {
                mutex.WaitOne();
                if (clients[i].IsConnected())
                {
                    ClientsConnected++;
                }
                else
                {
                    clients.RemoveAt(i);
                    i--;
                }
                mutex.ReleaseMutex();
            }
        }

        public virtual void StopListening()
        {
            isListening = false;
            allDone.Set();
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
            
            Debug.WriteLine("Client Connected");
            mutex.WaitOne();
            clients.Add(handler);
            mutex.ReleaseMutex();
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.
            int bytesRead = 0;

            try
            {
                bytesRead = handler.EndReceive(ar);
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read
                // more data.  
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the
                    // client. Display it on the console.  
                    //Debug.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                    //   content.Length, content);

                    int headerLength = content.IndexOf('>') + 1;
                    string headerStr = content.Substring(1, headerLength - 2);
                    string message = content.Substring(headerLength, content.Length - headerLength - 5);
                    ClientHeader header = ClientHeader.Error;
                    Enum.TryParse(headerStr, out header);
                    
                    MessageReceived(message, header, handler);
                    state.sb.Clear();
                }

                Debug.WriteLine("Received {0} bytes from client.", bytesRead);
            }

            // Not all data received. Get more.  
            if (handler.IsConnected())
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
        }
        
        
        public void Send(Socket handler, string data, ServerHeader header)
        {
            string message = $"<{header}>{data}<EOF>";

            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(message);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Debug.WriteLine("Sent {0} bytes to client.", bytesSent);

                
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
    }


    public abstract class AsynchronousSocketClient
    {
        // The port number for the remote device.  
        private int port = 11000;
        private Socket clientSocket = null;
        string ip;

        // ManualResetEvent instances signal completion.  
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);

        Mutex mutex = new Mutex();

        public AsynchronousSocketClient(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        // Return true if success
        public bool Start()
        {
            Stop();
            Connect();

            if (IsRunning())
            {
                Receive(clientSocket);
                return true;
            }

            return false;
        }

        public bool IsRunning()
        {

            return clientSocket != null && clientSocket.IsConnected();
        }

        public void Stop()
        {
            Disconnect();
        }

        protected abstract void MessageReceived(string message, ServerHeader header, Socket handler);


        protected void Connect()
        {
            if (clientSocket != null && clientSocket.IsConnected())
            {
                return;
            }

            try
            {
                IPAddress ipAddress = IPAddress.Parse(ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.  
                clientSocket = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                clientSocket.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), clientSocket);
                Debug.WriteLine("Connecting to " + remoteEP);
                connectDone.WaitOne();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        protected void Disconnect()
        {
            if (clientSocket == null)
                return;

            if (clientSocket.IsConnected())
                clientSocket.Shutdown(SocketShutdown.Both);

            clientSocket.Close();
            clientSocket.Dispose();
            clientSocket = null;
        }

        public void SendMessage(string message, ClientHeader header)
        {
            if (!(clientSocket != null && clientSocket.IsConnected()))
                return;

            // Connect to a remote device.  
            try
            {
                //Connect("NGUYEN_MSI_LAPTOP", 28102);
                // Send test data to the remote device.  
                Send(clientSocket, $"<{header}>{message}<EOF>");
                sendDone.WaitOne();

                //Disconnect();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return;
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

            }
            catch (Exception e)
            {
                //Debug.WriteLine(e.ToString());
                mutex.WaitOne();

                if (clientSocket != null)
                {
                    clientSocket.Dispose();
                    clientSocket = null;
                }

                mutex.ReleaseMutex();
            }

            // Signal that the connection has been made.  
            connectDone.Set();
        }

        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                Disconnect();
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                if (state is null)
                    return;

                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                string content = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                // There might be more data, so store the data received so far.  
                state.sb.Append(content);
                content = state.sb.ToString();
                if (bytesRead > 0)
                {
                    if (content.IndexOf("<EOF>") >= 0)
                    {
                        // All the data has arrived; Call callback.  
                        if (state.sb.Length > 1)
                        {
                            int headerLength = content.IndexOf('>') + 1;
                            string headerStr = content.Substring(1, headerLength - 2);
                            string message = content.Substring(headerLength, content.Length - headerLength - 5);
                            ServerHeader header = ServerHeader.Error;
                            Enum.TryParse(headerStr, out header);

                            MessageReceived(message, header, client);
                        }

                        state.sb.Clear();
                    }

                    Debug.WriteLine("Received {0} bytes from server.", bytesRead);
                }

                // Get the rest of the data.  
                if (client != null && client.IsConnected())
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        private void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Debug.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
    }

    // State object for reading client data asynchronously  
    public class StateObject
    {
        // Size of receive buffer.  
        public const int BufferSize = 10000;

        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];

        // Received data string.
        public StringBuilder sb = new StringBuilder();

        // Client socket.
        public Socket workSocket = null;
    }

    public enum ClientHeader
    {
        None,
        SignUpRequest,
        SignInRequest,
        RequestCountryHeader, // Request all countries' name
        RequestCountryData, // Request specific country
        Error
    }

    public enum ServerHeader
    {
        None,
        SignUpSuccess,
        SignUpFailed,
        SignInSuccess,
        SignInFailed,
        FetchCountryHeader,
        FetchCountryData,
        Error
    }

    static class SocketExtensions
    {
        public static bool IsConnected(this Socket socket)
        {
            try
            {
                bool part1 = socket.Poll(1, SelectMode.SelectRead);
                bool part2 = (socket.Available == 0);
                if ((part1 && part2) || !socket.Connected)
                    return false;
                else
                    return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }

    static class AsynchronousSocketHelper
    {
        public static void Main()
        {

        }
    }
}
