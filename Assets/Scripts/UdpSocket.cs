using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using UnityEngine;

public class UdpSocket
{
        private UdpClient _client;
        private IPEndPoint _clientEndPoint;
        private Thread _listener;
        private Thread _process;
        private bool _isListening;
        
        private bool _messageReceived;
        public UdpSocket()
        {
            _isListening = false;
            IsConnected = false;
            _messageReceived = false;
            IsActive = false;
        }
        public void Start(string ipAddress, int port)
        {
            if (IsActive) return;
            _isListening = false;
            IsConnected = false;
            _messageReceived = false;
            IsActive = true;
            _clientEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            _client = new UdpClient(_clientEndPoint);
            _process = new Thread(Process);
            _process.Start();
            Listen();
        }

        public void Stop()
        {
            if (!IsActive) return;
            IsActive = false;
            _messageReceived = true;
            _listener.Join();
            _client.Close();
        }

        public void ConnectionToServer(string ipAddress, int port, string password)
        {    
            var ipServerEndPoint =  new IPEndPoint(IPAddress.Parse(ipAddress), port);
            _client.Connect(ipServerEndPoint);
            var hashPass = CryptPass("test");
            var message = "{\\\"pass\\\" : \\\"" + hashPass + "\\\"}";
            Send(1, message);
            IsConnected = true;
        }

        private static string CryptPass(string pass)
        {
            var bytes = Encoding.UTF8.GetBytes(pass);
            var sha1 = SHA1.Create();
            var hashBytes = sha1.ComputeHash(bytes);
            return HexStringFromBytes(hashBytes);
        }
            
        private static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }

        public void Send(int id, string message)
        {
            var msgToSend = new Message(id, message).ToJson();
            var data = Encoding.UTF8.GetBytes(msgToSend);
            _client.BeginSend(data, data.Length, SendCallback, this);
        }

        private void SendCallback(IAsyncResult ar)
        {
            _client.EndSend(ar);
        }

        private void Listen()
        {
            _listener = new Thread(ListenProcess);
            _listener.Start();
        }

        private void ListenProcess()
        {
            while (IsActive)
            {
                if (!_isListening)
                {
                    _client.BeginReceive(ReceiveCallback, this);
                    _isListening = true;
                }
                while (!_messageReceived && _isListening)
                {
                    Thread.Sleep(100);
                }
                Thread.Sleep(10);
            }
            
        }
        
        private void ReceiveCallback(IAsyncResult ar)
        {
            var receiveBytes = _client.EndReceive(ar, ref _clientEndPoint);
            var receiveString = Encoding.UTF8.GetString(receiveBytes);
            Manager.msg = receiveString;
            Debug.Log($"Received: {receiveString}"); //DEBUG
            
            _messageReceived = true;
            _isListening = false;
        }

        private void Process()
        {
            while (IsActive)
            {
                if (_messageReceived)
                {
                    _messageReceived = false;
                }
                
                Thread.Sleep(100);
            }
        }
        
        public bool IsActive { get; private set; }
        public bool IsConnected { get; set; }
}
