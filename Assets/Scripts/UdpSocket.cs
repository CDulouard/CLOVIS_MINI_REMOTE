using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using Newtonsoft.Json;
using UnityEngine;
using Random = System.Random;


namespace ConsoleApplication1
{
    public class UdpSocket
    {
        private bool _verbose;
        private Socket _socket;
        private IPEndPoint _serverEndPoint;
        private int _bufSize;
        private State _state;
        private AsyncCallback _recv;
        public Dictionary<string, float> LastPos = new Dictionary<string, float>();

        private EndPoint _epFrom = new IPEndPoint(IPAddress.Any, 0);

        private string _hashPass;

        private bool _rcvPong;
        private bool _sendPing;
        private int _tPong;


        private class State
        {
            public byte[] Buffer;

            public State(int bufferSize = 8192)
            {
                Buffer = new byte[bufferSize];
            }
        }


        /// <summary>
        /// This method creat a UdpSocket object.
        /// <example>For example:
        /// <code>
        ///    UdpSocket socket = new UdpSocket();
        /// </code>
        /// results in a new UdpSocket. To start the socket then use start method.
        /// </example>
        /// </summary>
        public UdpSocket()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IsConnected = false;
            IsActive = false;
            _recv = null;
            RemoteUser = null;
        }

        /// <summary>
        /// This method starts the server with (<paramref name="ipAddressServer"/>) as ip
        /// on (<paramref name="portServer"/>) port. It is also possible to set a (<paramref name="password"/>) and
        /// change the (<paramref name="bufferSize"/>). Set (<paramref name="verbose"/>) to false to hide server's
        /// messages.
        /// <example>For example:
        /// <code>
        ///    UdpSocket socket = new UdpSocket();
        ///    socket.Start("127.0.0.1", 27000);
        /// </code>
        /// results in a new UdpSocket sored in socket variable and start it with ip 127.0.0.1 on port 27000.
        /// </example>
        /// </summary>
        ///<param name="ipAddressServer">The ip address used to bind the socket on</param>
        ///<param name="portServer">The port used to bind the socket on</param>
        ///<param name="password">The password used for connection, default "" </param>
        ///<param name="bufferSize">The size of the buffer used to send and receive message</param>
        ///<param name="verbose">Set verbose to false if you don't want to see server's message in the console</param>
        public void Start(string ipAddressServer, int portServer, string password = "", int bufferSize = 8192,
            bool verbose = true)
        {
            _verbose = verbose;
            if (IsActive) return;
            _bufSize = bufferSize;
            _hashPass = CryptPass(password);
            // Start socket
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(ipAddressServer), portServer);
            _socket.Bind(_serverEndPoint);
            _state = new State(bufferSize);

            _tPong = DateTime.Now.Millisecond;
            _rcvPong = false;
            _sendPing = false;

            //  Set flags
            IsConnected = false;
            IsActive = true;

            Receive();
            Thread.Sleep(1);
            if (_verbose)
            {
                Debug.Log("Server started on ip " + ipAddressServer.ToString() + " and port " + portServer.ToString() +
                          ".");
            }
        }

        /// <summary>
        /// This method stops the server.
        /// <example>For example:
        /// <code>
        ///    UdpSocket socket = new UdpSocket();
        ///    socket.Start("127.0.0.1", 27000);
        ///    socket.Stop();
        /// </code>
        /// Start a UdpSocket with ip 127.0.0.1 on port 27000 then stop it.
        /// </example>
        /// </summary>
        public void Stop()
        {
            if (!IsActive) return;
            _socket.Close();
            IsActive = false;
        }


        /// <summary>
        /// This method can send a message to a given machine. The ip of the machine can be specified
        /// in the (<paramref name="targetIp"/>) parameter and the port in the (<paramref name="targetPort"/>)
        /// parameter. The message is specified in the (<paramref name="text"/>) parameter.
        /// <example>For example:
        /// <code>
        ///    UdpSocket socket = new UdpSocket();
        ///    socket.Start("127.0.0.1", 27000);
        ///    socket.SendTo("127.0.0.1", 27000, "Hello");
        /// </code>
        /// This code creat a new udpSocket and use it to send the message "Hello" to itself.
        /// </example>
        /// </summary>
        ///<param name="targetIp">The ip address used to bind the socket on</param>
        ///<param name="targetPort">The port used to bind the socket on</param>
        ///<param name="text">The message to send as a string </param>
        public void SendTo(string targetIp, int targetPort, string text)
        {
            var target = new IPEndPoint(IPAddress.Parse(targetIp), targetPort);
            SendToProcess(target, text);
        }

        /// <summary>
        /// This method can send a message to a given machine. The IPEndPoint corresponding to the machine
        /// is specified by the (<paramref name="target"/>) parameter.
        /// The message is specified in the (<paramref name="text"/>) parameter.
        /// <example>For example:
        /// <code>
        ///    UdpSocket socket = new UdpSocket();
        ///    socket.Start("127.0.0.1", 27000);
        ///    socket.SendTo("127.0.0.1", 27000, "Hello");
        /// </code>
        /// This code creat a new udpSocket and use it to send the message "Hello" to itself.
        /// </example>
        /// </summary>
        ///<param name="target">The IPEndPoint used to bind the socket on</param>
        ///<param name="text">The message to send as a string </param>
        public void SendTo(IPEndPoint target, string text)
        {
            SendToProcess(target, text);
        }

        /// <summary>
        /// This method is the process used by the methods SendTo.
        /// This method can send a message to a given machine. The IPEndPoint corresponding to the machine
        /// is specified by the (<paramref name="target"/>) parameter.
        /// The message is specified in the (<paramref name="text"/>) parameter.
        /// <example>For example:
        /// <code>
        ///    UdpSocket socket = new UdpSocket();
        ///    socket.Start("127.0.0.1", 27000);
        ///    socket.SendTo("127.0.0.1", 27000, "Hello");
        /// </code>
        /// This code creat a new udpSocket and use it to send the message "Hello" to itself.
        /// </example>
        /// </summary>
        ///<param name="target">The IPEndPoint used to bind the socket on</param>
        ///<param name="text">The message to send as a string </param>
        private void SendToProcess(IPEndPoint target, string text)
        {
            try
            {
                var data = Encoding.ASCII.GetBytes(text);
                var sendState = new State(_bufSize);
                _socket.BeginSendTo(data, 0, data.Length, SocketFlags.None, target, (ar) =>
                {
                    var so = (State) ar.AsyncState;
                    try
                    {
                        var bytes = _socket.EndSend(ar);
                        if (_verbose)
                        {
                            Debug.Log(String.Format("SEND: {0}, {1}", bytes, text));
                        }
                    }
                    catch
                    {
                        if (_verbose)
                        {
                            Debug.Log(String.Format("Unable to send message to: {0}", target));
                        }
                    }
                }, sendState);
            }
            catch
            {
                if (_verbose)
                {
                    Debug.Log(String.Format("Destination unavailable"));
                }
            }
        }

        /// <summary>
        /// This method can creat a new connection to a given machine.
        /// The parameter (<paramref name="targetEndPoint"/>) is used to give both ip and port for the machine.
        /// <example>For example:
        /// <code>
        ///    UdpSocket socket = new UdpSocket();
        ///    socket.Start("127.0.0.1", 27000);
        ///    socket.CreatConnection(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000));
        /// </code>
        /// This code creat a new udpSocket and creat a connection to itself.
        /// </example>
        /// </summary>
        ///<param name="targetEndPoint">The IPEndPoint related to the machine we want to connect to.</param>
        public void CreatConnection(IPEndPoint targetEndPoint)
        {
            _socket.Connect(targetEndPoint);
            IsConnected = true;
            Send(new Message(1, "{\"connection_status\" : 1 }").ToJson());
        }


        /// <summary>
        /// Send a message to the machine we are connected with.
        /// The message to send have to be specified in the (<paramref name="text"/>) parameter.
        /// <example>For example:
        /// <code>
        ///    UdpSocket socket = new UdpSocket();
        ///    socket.Start("127.0.0.1", 27000);
        ///    socket.CreatConnection(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000));
        ///    socket.Send("Hello");
        /// </code>
        /// This code creat a new udpSocket, connect itself to itself and use the Send function to send
        /// the message "Hello" to itself.
        /// </example>
        /// </summary>
        ///<param name="text">The message to send as a string </param>
        public void Send(string text)
        {
            if (!IsConnected) return;
            var data = Encoding.ASCII.GetBytes(text);
            _socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
            {
                var so = (State) ar.AsyncState;
                var bytes = _socket.EndSend(ar);
                if (_verbose)
                {
                    Debug.Log(String.Format("SEND: {0}, {1}", bytes, text));
                }
            }, _state);
        }

        /// <summary>
        /// This method hash the (<paramref name="password"/>) using SHA1 algorithm.
        /// <example>For example:
        /// <code>
        ///    var hashPass = CryptPass("test");
        /// </code>
        /// result in a string stores in hashPass which is the hashed version of the word "test"
        /// </example>
        /// </summary>
        ///<param name="password">The password to hash</param>
        ///<returns>A string which is the hashed password</returns>
        public static string CryptPass(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var sha1 = SHA1.Create();
            var hashBytes = sha1.ComputeHash(bytes);
            return HexStringFromBytes(hashBytes);
        }

        /// <summary>
        /// This method convert the byte array (<paramref name="bytes"/>) into string in hexadecimal format.
        /// <example>For example:
        /// <code>
        ///    byte[] numbers = { 0, 16, 104, 213 }
        ///    var newString = HexStringFromBytes(numbers);
        /// </code>
        /// result in a string stores in newString which value "001068d5"
        /// </example>
        /// </summary>
        ///<param name="bytes">The byte array to convert in hexadecimal string</param>
        ///<returns>A string which is the byte array converted in hexadecimal</returns>
        public static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }

            return sb.ToString();
        }

        /// <summary>
        /// This method is called when the server starts. It receive the message and call the
        /// Handler method.
        /// </summary>
        private void Receive()
        {
            try
            {
                _socket.BeginReceiveFrom(_state.Buffer, 0, _bufSize, SocketFlags.None, ref _epFrom, _recv = (ar) =>
                {
                    var so = (State) ar.AsyncState;
                    try
                    {
                        var bytes = _socket.EndReceiveFrom(ar, ref _epFrom);
                        Handler(so, bytes);
                    }
                    catch
                    {
                        if (_verbose)
                        {
                            Debug.Log(String.Format("Reception error"));
                        }
                    }

                    try
                    {
                        _socket.BeginReceiveFrom(so.Buffer, 0, _bufSize, SocketFlags.None, ref _epFrom, _recv, so);
                    }
                    catch
                    {
                        Receive();
                    }
                }, _state);
            }
            catch
            {
                if (_verbose)
                {
                    Debug.Log(String.Format("Error"));
                }
            }
        }

        /// <summary>This method returns an IPEndPoint object with the ip and port from the EndPoint parameter
        /// (<paramref name="ep"/>).
        /// </summary>
        ///<param name="ep">The EndPoint to convert into IPEndPoint</param>
        ///<returns>The IPEndPoint converted from the EndPoint</returns>
        public static IPEndPoint EndPointToIpEndPoint(EndPoint ep)
        {
            var strEp = ep.ToString();
            var ipAdress = "";
            var port = "";
            var isIpAdress = true;

            foreach (var c in strEp)
            {
                if (c != ':' && isIpAdress)
                {
                    ipAdress += c;
                }
                else if (c == ':')
                {
                    isIpAdress = false;
                }
                else
                {
                    port += c;
                }
            }


            return new IPEndPoint(IPAddress.Parse(ipAdress), int.Parse(port));
        }

        /// <summary>
        /// This method manage the behaviour of the server when it receive a message.
        /// </summary>
        private void Handler(State so, int nBytes)
        {
            /*
             * Message id starting by 1xx are post message (message that ask for changing something on the server)
             * Message id starting by 2xx are answer message
             * Message id starting by 3xx are get message (message that ask to know something from the server)
             *
             * 
             * Message id = 101 incoming request for connection
             * Message id = 201 answer to a connection request
             */
            var rcvString = Encoding.ASCII.GetString(so.Buffer, 0, nBytes);
            if (!Message.IsMessage(rcvString))
            {
                /*
                 * This condition means the incoming message is not a message object
                 */
                switch (rcvString)
                {
                    case "ping":
                        SendTo(EndPointToIpEndPoint(_epFrom), "pong");
                        break;
                    case "pong":
                        if (_sendPing)
                        {
                            _tPong = DateTime.Now.Millisecond;
                            _rcvPong = true;
                        }

                        break;
                    default:
                        if (_verbose)
                        {
                            Debug.Log(String.Format("RECV: {0}: {1}, {2}", _epFrom.ToString(), nBytes, rcvString));
                        }

                        break;
                }
            }
            else if (!new Message(rcvString).CheckMessage())
            {
                /*
                 * This condition means the message is a corrupted Message object
                 */
            }
            else
            {
                /*
                 * This condition means the message is a a Message object and it is not corrupted
                 */

                /* Write here the code to execute when a new Message is received */
                var rcvMessage = new Message(rcvString);

                if (RemoteUser != null && EndPointToIpEndPoint(_epFrom).Equals(RemoteUser))
                {
                    /*
                     * This condition means the source of the incoming message is the identified remote user
                     */
                    switch (rcvMessage.id)
                    {
                        case 101: // Ask for Connection
                            /*
                             * The incoming message comes from an user already connected.
                             */
                            SendTo(RemoteUser,
                                new Message(201, "{" + '"' + "connection_status" + '"' + ": 1}").ToJson());
                            break;
                        default:
                            if (_verbose)
                            {
                                Debug.Log(String.Format("Unknown id"));
                                Debug.Log(String.Format(rcvString));
                            }

                            break;
                    }
                }
                else
                {
                    /*
                     * This condition means the source of the message is not the remote user
                     */
                    switch (rcvMessage.id)
                    {
                        case 101: // Ask for Connection
                            /*
                             * The incoming message must have two keys "password" and "verbose".
                             * "password" is the hashed password with SHA1 algorithm.
                             * "verbose" tell the server if he must send a reply. Set the value to 1 for a reply else 0.
                             * Example request :
                             * {"id": 101, "parity": 1, "len": 71, "message": "{\"password\": \"a94a8fe5ccb19ba61c4c0873d391e987982fbbd3\" , \"verbose\": 1}"}
                             * If the password is correct then the default remote user is the origin of the request.
                             */
                            try
                            {
                                var temp = new ConnectionMessage(rcvMessage.message);
                                if (temp.password.Equals(_hashPass))
                                {
                                    RemoteUser = EndPointToIpEndPoint(_epFrom);
                                    if (temp.verbose == 1)
                                    {
                                        SendTo(RemoteUser,
                                            new Message(201, "{" + '"' + "connection_status" + '"' + ": 1}").ToJson());
                                    }
                                }
                                else
                                {
                                    if (temp.verbose == 1)
                                    {
                                        SendTo(EndPointToIpEndPoint(_epFrom),
                                            new Message(201, "{" + '"' + "connection_status" + '"' + ": 0}").ToJson());
                                    }
                                }
                            }
                            catch
                            {
                                if (_verbose)
                                {
                                    Debug.Log(String.Format("Message format not correct for connection"));
                                }
                            }

                            break;

                        case 2:
                            try
                            {
                                var tmp = JsonConvert.DeserializeObject<Dictionary<string, string>>(rcvMessage.message);
                                if (tmp["answer"] == "CONNECTED")
                                {
                                    Manager.RemoteUser = EndPointToIpEndPoint(_epFrom);
                                    Debug.Log(EndPointToIpEndPoint(_epFrom));
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.Log(e);
                            }

                            break;

                        case 4:
                            try
                            {
                                var tmp = JsonConvert.DeserializeObject<OldRobotStatus>(rcvMessage.message);
                                Debug.Log(tmp.ToPosDict());
                                lock (LastPos)
                                {
                                    LastPos = tmp.ToPosDict();
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.Log(e);
                            }

                            break;

                        default:
                            if (_verbose)
                            {
                                Debug.Log("Unknown id");
                                Debug.Log(rcvString);
                            }

                            break;
                    }
                }
            }
        }

        /// <summary>This method returns the ping between two machines in ms.
        /// (<paramref name="ipAddress"/>) is the ip address of the machine to ping.
        /// (<paramref name="port"/>) is the port of the machine to ping.
        /// (<paramref name="timeOut"/>) is the max time to wait between ping and pong.
        /// </summary>
        ///<param name="ipAddress">The is the ip address of the machine to ping.</param>
        ///<param name="port">The is the port of the machine to ping.</param>
        ///<param name="timeOut">is the max time to wait between ping and pong.</param>
        ///<returns>The ping between two machines in ms</returns>
        public int Ping(string ipAddress, int port, int timeOut = 1000)
        {
            var target = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            return PingProcess(target, timeOut);
        }

        /// <summary>This method returns the ping between two machines in ms.
        /// (<paramref name="target"/>) is the IPEndPoint corresponding to the machine to ping.
        /// (<paramref name="timeOut"/>) is the max time to wait between ping and pong.
        /// </summary>
        ///<param name="target">The IPEndPoint corresponding to the machine to ping.</param>
        ///<param name="timeOut">is the max time to wait between ping and pong.</param>
        ///<returns>The ping between two machines in ms</returns>
        public int Ping(IPEndPoint target, int timeOut = 1000)
        {
            return PingProcess(target, timeOut);
        }

        /// <summary>This method is used by the ping methods to compute the ping.
        /// (<paramref name="target"/>) is the IPEndPoint corresponding to the machine to ping.
        /// (<paramref name="timeOut"/>) is the max time to wait between ping and pong.
        /// </summary>
        ///<param name="target">The IPEndPoint corresponding to the machine to ping.</param>
        ///<param name="timeOut">is the max time to wait between ping and pong.</param>
        ///<returns>The ping between two machines in ms</returns>
        private int PingProcess(IPEndPoint target, int timeOut = 1000)
        {
            var tPing = DateTime.Now.Millisecond;
            _sendPing = true;
            SendTo(target, "ping");

            while (!_rcvPong && DateTime.Now.Millisecond - tPing < timeOut)
            {
            }

            if (!_rcvPong)
            {
                _sendPing = false;
                _rcvPong = false;
                return int.MaxValue;
            }

            _sendPing = false;
            _rcvPong = false;
            return _tPong - tPing;
        }

        public IPEndPoint RemoteUser { get; set; }

        public bool IsActive { get; private set; }
        public bool IsConnected { get; set; }


        /// <summary>This method returns a random string with a length specified in the
        /// (<paramref name="length"/>) parameter.
        /// <example>For example:
        /// <code>
        ///    var myString = UdpSocket.CreateRandomString(5)
        /// </code>
        /// result in a string random string with length 5.
        /// </example>
        /// </summary>
        ///<param name="length">The length of the random string</param>
        ///<returns>A random string with specified length</returns>
        public static string CreateRandomString(int length)
        {
            var strBuild = new StringBuilder();
            var random = new Random();

            for (var i = 0; i < length; i++)
            {
                var flt = random.NextDouble();
                var shift = Convert.ToInt32(Math.Floor(25 * flt));
                var letter = Convert.ToChar(shift + 65);
                strBuild.Append(letter);
            }

            return strBuild.ToString();
        }

        public Dictionary<string, float> GetLastPos()
        {
            lock (LastPos)
            {
                return LastPos;
            }
        }
    }
}