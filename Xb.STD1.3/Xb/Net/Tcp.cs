using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Xb.Net
{
    /// <summary>
    /// TCP Socket Client / Server
    /// TCP接続管理クラス
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class Tcp : IDisposable
    {
        /// <summary>
        /// Socket-Role type
        /// ソケット役割
        /// </summary>
        public enum RoleType
        {
            /// <summary>
            /// for Client
            /// クライアント
            /// </summary>
            Client,

            /// <summary>
            /// for Server
            /// サーバ
            /// </summary>
            Server
        }

        /// <summary>
        /// Object-set for async timeout
        /// 非同期メソッドタイムアウト用保持データ一式
        /// </summary>
        private class TimerSet : IDisposable
        {
            public Timer Timer { get; set; }
            public EndPoint EndPoint { get; set; }
            public SocketAsyncEventArgs EventArgs { get; set; }

            public void Dispose()
            {
                this.Timer?.Dispose();
                this.EndPoint = null;
            }
        }

        /// <summary>
        /// Network-Action event arguments
        /// 通信アクションイベント引数クラス
        /// </summary>
        public class ActionEventArgs : EventArgs
        {
            public EndPoint EndPoint { get; private set; }

            public ActionEventArgs(EndPoint endPoint)
            {
                this.EndPoint = endPoint;
            }
        }

        /// <summary>
        /// Data-Recieve event arguments
        /// データ受信イベント引数クラス
        /// </summary>
        /// <remarks></remarks>
        public class RecieveEventArgs : ActionEventArgs
        {
            public byte[] Bytes { get; private set; }

            public RecieveEventArgs(byte[] bytes
                , EndPoint endPoint)
                : base(endPoint)
            {
                this.Bytes = bytes ?? new byte[] {};
            }
        }


        /// <summary>
        /// Network-timeout Exception
        /// 通信タイムアウト例外
        /// </summary>
        public class TimeoutException : Exception
        {
            public EndPoint EndPoint { get; private set; }
            public SocketAsyncOperation Operation { get; private set; }

            public TimeoutException(EndPoint endPoint
                , SocketAsyncOperation operation
                , string message = "")
                : base(message)
            {
                this.EndPoint = endPoint;
                this.Operation = operation;
            }
        }


        /// <summary>
        /// Connected Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void ConnectEventHandler(object sender, ActionEventArgs e);
        public event ConnectEventHandler Connected;

        /// <summary>
        /// Accept Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void AcceptEventHandler(object sender, ActionEventArgs e);
        public event AcceptEventHandler Accepted;

        /// <summary>
        /// Disconnected Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void DisconnectEventHandler(object sender, ActionEventArgs e);
        public event DisconnectEventHandler Disconnected;

        /// <summary>
        /// Sended Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void SendEventHandler(object sender, ActionEventArgs e);
        public event SendEventHandler Sended;

        /// <summary>
        /// Recieved Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void RecieveEventHandler(object sender, RecieveEventArgs e);
        public event RecieveEventHandler Recieved;


        /// <summary>
        /// Socket-Role type
        /// ソケット役割
        /// </summary>
        public RoleType Role { get; private set; } = RoleType.Client;

        /// <summary>
        /// Timeout (millisec)
        /// </summary>
        public int Timeout { get; set; } = 30000;

        /// <summary>
        /// my own socket
        /// ローカル側(自分自身)のソケット
        /// </summary>
        private System.Net.Sockets.Socket _localSocket;

        /// <summary>
        /// Destination's socket
        /// リモート側のソケット配列
        /// </summary>
        private Dictionary<EndPoint, Socket> _remoteSockets
            = new Dictionary<EndPoint, Socket>();

        /// <summary>
        /// timer for async methods
        /// 非同期メソッドタイムアウト処理用タイマー
        /// </summary>
        private Dictionary<SocketAsyncEventArgs, TimerSet> _timeoutTimers
            = new Dictionary<SocketAsyncEventArgs, TimerSet>();



        /// <summary>
        /// Destinations's EndPoints list
        /// 接続先EndPointオブジェクトリスト
        /// </summary>
        public EndPoint[] Remotes
        {
            get
            {
                return this._remoteSockets
                    .Select(pair => pair.Value.RemoteEndPoint)
                    .ToArray();
            }
        }


        /// <summary>
        /// Constructor - client mode
        /// コンストラクタ - クライアントモード
        /// </summary>
        /// <param name="connectIpAddress"></param>
        /// <param name="connectPort"></param>
        /// <remarks></remarks>
        public Tcp(string connectIpAddress
                 , int connectPort)
        {
            //IPアドレスがnull
            if (connectIpAddress == null)
            {
                Xb.Util.Out("Xb.Net.Tcp.Constructor: ip-address null");
                throw new ArgumentException("Xb.Net.Tcp.Constructor: ip-address null");
            }

            //ポートがTCP範囲外
            if (connectPort < 0 || 65335 < connectPort)
            {
                Xb.Util.Out($"Xb.Net.Tcp.Constructor: tcp-port out of range [{connectPort}]");
                throw new ArgumentOutOfRangeException(nameof(connectPort),
                    $"Xb.Net.Tcp.Constructor: tcp-port out of range [{connectPort}]");
            }

            IPAddress ipAddress;

            //IPアドレスのパース失敗
            if (!IPAddress.TryParse(connectIpAddress, out ipAddress))
            {
                Xb.Util.Out($"Xb.Net.Tcp.Constructor: invalid ip-address [{connectIpAddress}]");
                throw new ArgumentException($"Xb.Net.Tcp.Constructor: invalid ip-address [{connectIpAddress}]");
            }

            this.Connect(ipAddress, connectPort);
        }


        /// <summary>
        /// Constructor - client mode
        /// コンストラクタ - クライアントモード
        /// </summary>
        /// <param name="connectIpAddress"></param>
        /// <param name="connectPort"></param>
        /// <remarks></remarks>
        public Tcp(System.Net.IPAddress connectIpAddress
                 , int connectPort)
        {
            this.Connect(connectIpAddress, connectPort);
        }


        /// <summary>
        /// Connect to remote
        /// リモートへ接続する。
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <remarks></remarks>
        private void Connect(System.Net.IPAddress ipAddress
                           , int port)
        {
            try
            {
                this._localSocket = new Socket(ipAddress.AddressFamily
                    , SocketType.Stream
                    , ProtocolType.Tcp);

                var ev = new SocketAsyncEventArgs
                {
                    RemoteEndPoint = new System.Net.IPEndPoint(ipAddress, port)
                };

                ev.Completed += this.OnCompleted;
                this._localSocket.ConnectAsync(ev);

                this.SetTimer(ev, ev.RemoteEndPoint);
            }
            catch (Exception ex)
            {
                Xb.Util.Out(ex);
                throw ex;
            }

            //接続完了イベントをレイズする。
            //this.FireEvent("Connected", new object[] { this._socketLocal });


            this.Role = RoleType.Client;
        }


        /// <summary>
        /// Constructor - server mode
        /// コンストラクタ - サーバモード
        /// </summary>
        /// <param name="listenPort"></param>
        public Tcp(int listenPort)
        {
            //ポートがTCP範囲外
            if (listenPort < 0 || 65335 < listenPort)
            {
                Xb.Util.Out($"Xb.Net.Tcp.Constructor: tcp-port out of range [{listenPort}]");
                throw new ArgumentOutOfRangeException(nameof(listenPort)
                    , $"Xb.Net.Tcp.Constructor: tcp-port out of range [{listenPort}]");
            }

            //ソケットを生成する。
            //IPv4, v6両対応の全ローカルアドレスをListenする
            //http://dobon.net/vb/dotnet/internet/udpclient.html
            //http://dobon.net/vb/dotnet/internet/udpclient.html
            var endPoint = new System.Net.IPEndPoint(System.Net.IPAddress.IPv6Any, listenPort);
            this._localSocket = new System.Net.Sockets.Socket(endPoint.Address.AddressFamily
                , System.Net.Sockets.SocketType.Stream
                , System.Net.Sockets.ProtocolType.Tcp);
            this._localSocket.SetSocketOption(System.Net.Sockets.SocketOptionLevel.IPv6
                , System.Net.Sockets.SocketOptionName.IPv6Only
                , 0);

            try
            {
                this._localSocket.Bind(endPoint);
                this._localSocket.Listen(1000);

                var ev = new SocketAsyncEventArgs();
                ev.Completed += OnCompleted;
                this._localSocket.AcceptAsync(ev);
            }
            catch (Exception ex)
            {
                //バインド失敗時、ポートが使用中と思われる。エラー応答する。
                this._localSocket = null;

                Xb.Util.Out(ex);
                throw ex;
            }

            this.Role = RoleType.Server;
        }


        /// <summary>
        /// Operation competed event
        /// 各種操作完了イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ev"></param>
        private void OnCompleted(object sender, SocketAsyncEventArgs ev)
        {
            var socket = (Socket) sender;
            //Debug.WriteLine(String.Format("Async operation completed: {0}; Error: {1}; "
            //    , ev.LastOperation
            //    , ev.SocketError));

            switch (ev.LastOperation)
            {
                case SocketAsyncOperation.Connect:

                    //Xb.Util.Out($"Xb.Net.Tcp.OnCompleted: Connect, "
                    //            + $"RemoteEndPoint = {socket.RemoteEndPoint}, "
                    //            + $"LocalEndPoint = {socket.LocalEndPoint}");
                    //Xb.Util.Out($"same localSocket? : {(socket == this._localSocket)}");

                    if (ev.SocketError != SocketError.Success)
                        throw new Exception("Xb.Net.Tcp.OnCompleted: Connect failure");

                    var evConnect = new SocketAsyncEventArgs();
                    evConnect.Completed += this.OnCompleted;
                    evConnect.SetBuffer(new byte[1024], 0, 1024);
                    ev.ConnectSocket.ReceiveAsync(evConnect);

                    //fire event
                    this.Connected?.Invoke(this, new ActionEventArgs(socket.RemoteEndPoint));

                    break;

                case SocketAsyncOperation.Receive:

                    //Xb.Util.Out($"Xb.Net.Tcp.OnCompleted: Recieve, "
                    //            + $"RemoteEndPoint = {socket.RemoteEndPoint}, "
                    //            + $"LocalEndPoint = {socket.LocalEndPoint}");
                    //Xb.Util.Out($"same localSocket? : {(socket == this._localSocket)}");

                    if (ev.SocketError != SocketError.Success
                        || ev.BytesTransferred == 0)
                    {
                        //Xb.Util.Out($"Xb.Net.Tcp.OnCompleted: Recieve Error, "
                        //            + $"RemoteEndPoint = {socket.RemoteEndPoint}");

                        if (this._remoteSockets.ContainsKey(socket.RemoteEndPoint))
                        {
                            this._remoteSockets.Remove(socket.RemoteEndPoint);

                            //Xb.Util.Out("this._remoteSockets CHANGED!!");
                            //Xb.Util.OutHighlighted(this._remoteSockets
                            //    .Select(pair => pair.Key.ToString())
                            //    .ToArray());
                        }

                        //fire event
                        this.Disconnected?.Invoke(this
                            , new ActionEventArgs(socket.RemoteEndPoint));

                        socket.Dispose();
                        ev.Dispose();

                        return;
                    }

                    var bytes = ev.Buffer.Skip(ev.Offset)
                        .Take(ev.BytesTransferred)
                        .ToArray();
                    var asText = Encoding.ASCII.GetString(bytes);
                    //Debug.WriteLine($"Received: {ev.BytesTransferred} bytes: \"{asText}\"");

                    socket.ReceiveAsync(ev);

                    //fire event
                    this.Recieved?.Invoke(this
                        , new RecieveEventArgs(bytes
                            , socket.RemoteEndPoint));

                    break;

                case SocketAsyncOperation.Send:
                    //Xb.Util.Out($"Xb.Net.Tcp.OnCompleted: Send, "
                    //            + $"RemoteEndPoint = {socket.RemoteEndPoint}, "
                    //            + $"LocalEndPoint = {socket.LocalEndPoint}");
                    //Xb.Util.Out($"same localSocket? : {(socket == this._localSocket)}");

                    if (ev.SocketError != SocketError.Success)
                        throw new Exception("Xb.Net.Tcp.OnCompleted: Send failure");

                    this.Sended?.Invoke(this, new ActionEventArgs(socket.RemoteEndPoint));

                    break;

                case SocketAsyncOperation.Accept:

                    //Xb.Util.Out($"Xb.Net.Tcp.OnCompleted: Accept, "
                    //            + $"RemoteEndPoint = 'CANNOT READ', "
                    //            + $"LocalEndPoint = {socket.LocalEndPoint}");
                    //Xb.Util.Out($"same localSocket? : {(socket == this._localSocket)}");

                    if (ev.SocketError != SocketError.Success)
                        throw new Exception("Xb.Net.Tcp.OnCompleted: Accept failure");

                    //Continue Accepting
                    var clientSocket = ev.AcceptSocket;
                    ev.AcceptSocket = null;
                    socket.AcceptAsync(ev);

                    //Xb.Util.Out($"clientSocket.RemoteEndPoint: {clientSocket.RemoteEndPoint}");
                    //Xb.Util.Out($"clientSocket.LocalEndPoint: {clientSocket.LocalEndPoint}");

                    //regist client socket
                    if (!this._remoteSockets.ContainsKey(clientSocket.RemoteEndPoint))
                    {
                        this._remoteSockets.Add(clientSocket.RemoteEndPoint, clientSocket);

                        //Xb.Util.Out("this._remoteSockets CHANGED!!");
                        //Xb.Util.OutHighlighted(this._remoteSockets
                        //    .Select(pair => pair.Key.ToString())
                        //    .ToArray());
                    }


                    //Set Recieve event for Accepted socket.
                    var newEv = new SocketAsyncEventArgs();
                    newEv.Completed += this.OnCompleted;
                    newEv.SetBuffer(new byte[1024], 0, 1024);
                    clientSocket.ReceiveAsync(newEv);

                    //fire event
                    this.Accepted?.Invoke(this
                        , new ActionEventArgs(clientSocket.RemoteEndPoint));

                    break;

                case SocketAsyncOperation.Disconnect:

                    //Xb.Util.Out($"Xb.Net.Tcp.OnCompleted: Disconnect, "
                    //            + $"RemoteEndPoint = {socket.RemoteEndPoint}, "
                    //            + $"LocalEndPoint = {socket.LocalEndPoint}");
                    //Xb.Util.Out($"same localSocket? : {(socket == this._localSocket)}");

                    if (this._remoteSockets.ContainsKey(socket.RemoteEndPoint))
                    {
                        this._remoteSockets.Remove(socket.RemoteEndPoint);

                        //Xb.Util.Out("this._remoteSockets CHANGED!!");
                        //Xb.Util.OutHighlighted(this._remoteSockets
                        //    .Select(pair => pair.Key.ToString())
                        //    .ToArray());
                    }

                    this.Disconnected?.Invoke(this
                        , new ActionEventArgs(socket.RemoteEndPoint));

                    socket.Dispose();
                    ev.Dispose();

                    break;

                default:

                    break;
            }
        }


        /// <summary>
        /// Send Data to Remote
        /// 接続先にデータを送付する。
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public void Send(byte[] bytes
                       , EndPoint endPoint = null)
        {
            //TODO: implement Timeout

            //validate passing values
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes), "Xb.Net.Tcp.Send: passing null bytes");

            //sending target sockets
            var targetSockets = new List<Socket>();

            switch (this.Role)
            {
                case RoleType.Client:

                    if (this._localSocket == null
                        || !this._localSocket.Connected)
                        throw new InvalidOperationException("Xb.Net.Tcp.Send: not connected.");

                    //if client-mode, only one sending-target.
                    targetSockets.Add(this._localSocket);

                    break;
                case RoleType.Server:

                    if (endPoint != null
                        && !this._remoteSockets.ContainsKey(endPoint))
                        throw new InvalidOperationException("Xb.Net.Tcp.Send: target not found");

                    if (this._remoteSockets.Count <= 0)
                        throw new InvalidOperationException("Xb.Net.Tcp.Send: hasn't clients");

                    if (endPoint != null)
                    {
                        //order target, pick one
                        targetSockets.Add(this._remoteSockets
                            .Where(pair => pair.Key == endPoint)
                            .Select(pair => pair.Value)
                            .First());
                    }
                    else
                    {
                        //no-order, pick all
                        targetSockets.AddRange(this._remoteSockets
                            .Select(pair => pair.Value));
                    }

                    break;
                default:
                    throw new InvalidOperationException($"Xb.Net.Tcp.Send: undefined Xb.Net.Tcp.RoleType [{this.Role}]");
            }

            try
            {
                foreach (var socket in targetSockets)
                {
                    var ev = new SocketAsyncEventArgs();
                    ev.Completed += this.OnCompleted;
                    ev.SetBuffer(bytes, 0, bytes.Length);
                    socket.SendAsync(ev);

                    this.SetTimer(ev, socket.RemoteEndPoint);
                }
            }
            catch (Exception ex)
            {
                Xb.Util.Out(ex);
                throw ex;
            }
        }


        /// <summary>
        /// Send Text-Data to Remote
        /// 接続先にデータを送付する。
        /// </summary>
        /// <param name="text"></param>
        /// <param name="encoding">Default: UTF-8</param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public void Send(string text
                       , System.Text.Encoding encoding = null
                       , EndPoint endPoint = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            this.Send(encoding.GetBytes(text), endPoint);
        }


        /// <summary>
        /// Set timer for operation-timeout
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="endPoint"></param>
        private void SetTimer(SocketAsyncEventArgs ev
            , EndPoint endPoint)
        {
            var timerSet = new TimerSet();
            timerSet.EventArgs = ev;
            timerSet.EndPoint = endPoint;
            timerSet.Timer = new Timer(new TimerCallback(OnTimeout), timerSet, this.Timeout, -1);
            this._timeoutTimers.Add(ev, timerSet);
        }


        /// <summary>
        /// Timeouted
        /// </summary>
        /// <param name="arg"></param>
        private void OnTimeout(object arg)
        {
            var timerSet = (TimerSet) arg;

            var endPoint = timerSet.EndPoint;
            var operation = timerSet.EventArgs.LastOperation;

            this.RemoveTimer(timerSet.EventArgs);

            //throw exception
            throw new TimeoutException(endPoint, operation);
        }


        /// <summary>
        /// Remove timer for operation-timeout
        /// </summary>
        /// <param name="ev"></param>
        private void RemoveTimer(SocketAsyncEventArgs ev)
        {
            if (this._timeoutTimers.ContainsKey(ev))
            {
                var timerSet = this._timeoutTimers[ev];
                this._timeoutTimers.Remove(ev);
                timerSet.Dispose();
            }
        }

        // 重複する呼び出しを検出するには
        private bool _disposedValue = false;

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    foreach (var pair in this._timeoutTimers)
                    {
                        try
                        { pair.Value.Dispose(); }
                        catch (Exception) { }
                    }
                    this._timeoutTimers = null;

                    foreach (var pair in this._remoteSockets)
                    {
                        try
                        { pair.Value.Dispose(); }
                        catch (Exception) { }
                    }
                    this._remoteSockets = null;

                    try
                    { this._localSocket?.Dispose(); }
                    catch (Exception) { }
                }
            }
            this._disposedValue = true;
        }

        #region " IDisposable Support "
        // このコードは、破棄可能なパターンを正しく実装できるように Visual Basic によって追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(ByVal disposing As Boolean) に記述します。
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }

}
