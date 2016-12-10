using System;

namespace Xb.Net
{

    /// <summary>
    /// TCP接続管理クラス
    /// </summary>
    /// <remarks>
    /// ※注意※ 送受信データサイズは最大で 1GByte まで。
    /// </remarks>
    public class Tcp : IDisposable
    {
        /// <summary>
        /// クライアントソケットのデータ保持クラス
        /// </summary>
        /// <remarks></remarks>
        private class Client
        {

            public const int BufferLength = 1024;
            public readonly System.Net.Sockets.Socket Socket;
            public readonly byte[] Buffer;

            public System.IO.MemoryStream Stream;

            public Client(System.Net.Sockets.Socket socket)
            {
                this.Socket = socket;
                this.Buffer = new byte[BufferLength + 1];
                this.Stream = new System.IO.MemoryStream();
            }
        }


        /// <summary>
        /// データ受信イベント引数クラス
        /// </summary>
        /// <remarks></remarks>
        public class RemoteEventArgs : EventArgs
        {
            public readonly byte[] Bytes;
            public readonly System.Net.Sockets.Socket RemoteSocket;

            public RemoteEventArgs(byte[] bytes, System.Net.Sockets.Socket remoteSocket)
            {
                if ((bytes == null))
                    bytes = new byte[]{};
                this.Bytes = bytes;
                this.RemoteSocket = remoteSocket;
            }
        }


        public delegate void RecieveEventHandler(object sender, RemoteEventArgs e);
        public event RecieveEventHandler Recieved;
        public event EventHandler Connected;
        public event EventHandler Disconnected;


        private System.Net.Sockets.Socket _socketLocal;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <remarks></remarks>
        public Tcp()
        {
        }


        /// <summary>
        /// イベントをレイズする。
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="aryArgs"></param>
        /// <remarks></remarks>

        private void FireEvent(object eventType, object aryArgs)
        {
            object[] args = new object[] {};
            System.Net.Sockets.Socket socket = null;
            byte[] bytes = null;

            if (aryArgs != null)
            {
                args = (object[])aryArgs;
            }

            switch (eventType.ToString())
            {
                case "Connected":
                    if (args.Length > 0)
                        socket = (System.Net.Sockets.Socket)args[0];
                    if (Connected != null)
                    {
                        Connected(this, new RemoteEventArgs(null, socket));
                    }

                    break;
                case "Disconnected":
                    if (Disconnected != null)
                    {
                        Disconnected(this, new EventArgs());
                    }

                    break;
                case "Recieved":
                    if (args.Length > 0)
                        bytes = (byte[])args[0];
                    if (args.Length > 1)
                        socket = (System.Net.Sockets.Socket)args[1];
                    try
                    {
                        if (Recieved != null)
                        {
                            Recieved(this, new RemoteEventArgs(bytes, socket));
                        }
                    }
                    catch (Exception)
                    {
                        Xb.Util.Out("Net.Tcp.RaiseEventOnUiThread: イベント用パラメータパースに失敗しました。");
                        throw new ArgumentException("イベント用パラメータパースに失敗しました。");
                    }

                    break;
                default:
                    break;
                    //何もしない。
            }
        }


        /// <summary>
        /// リモートへ接続する。
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <remarks></remarks>
        public void Connect(string ipAddress, int port)
        {
            //IPアドレスが不正のとき、異常終了。
            if (ipAddress == null)
            {
                Xb.Util.Out("渡し値IPアドレスが不正です。");
                throw new ArgumentException("渡し値IPアドレスが不正です。");
            }

            //IPアドレスが渡されていないとき、もしくはポートがTCP範囲外のとき、異常終了する。
            if (port < 0 || 65335 < port)
            {
                Xb.Util.Out("渡し値ポートが不正です。");
                throw new ArgumentException("渡し値ポートが不正です。");
            }

            string[] ipAddressSegments = ipAddress.Split((".")[0]);
            int segInt = 0;
            int i = 0;
            byte[] ipBytes = new byte[4];

            //IPアドレスの書式チェック
            //"."で分割したとき、4つに分かれていないとき、異常終了。
            if (ipAddressSegments.Length != 4)
            {
                Xb.Util.Out("渡し値アドレスが不正です。");
                throw new ArgumentException("渡し値アドレスが不正です。");
            }

            //分割したセグメントがそれぞれ0～255の範囲を超えているとき異常終了。
            for (i = 0; i <= 3; i++)
            {
                if ((!int.TryParse(ipAddressSegments[i], out segInt)))
                {
                    Xb.Util.Out("渡し値アドレスが不正です。");
                    throw new ArgumentException("渡し値アドレスが不正です。");
                }
                if ((segInt < 0 || 255 < segInt))
                {
                    Xb.Util.Out("渡し値アドレスが不正です。");
                    throw new ArgumentException("渡し値アドレスが不正です。");
                }
                ipBytes[i] = BitConverter.GetBytes(segInt)[0];
            }

            this.Connect(new System.Net.IPAddress(ipBytes), port);

        }


        /// <summary>
        /// リモートへ接続する。
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <remarks></remarks>

        public void Connect(System.Net.IPAddress ipAddress, int port)
        {
            //IPアドレスが不正のとき、異常終了。
            if (ipAddress == null)
            {
                Xb.Util.Out("渡し値IPアドレスが不正です。");
                throw new ArgumentException("渡し値IPアドレスが不正です。");
            }

            //渡し値ポートがTCP範囲外のとき、異常終了。
            if (port < 0 | 65335 < port)
            {
                Xb.Util.Out("渡し値ポートが不正です。");
                throw new ArgumentException("渡し値ポートが不正です。");
            }

            System.Net.IPEndPoint endPoint = null;
            Client client = null;

            try
            {
                endPoint = new System.Net.IPEndPoint(ipAddress, port);
                this._socketLocal = new System.Net.Sockets.Socket(ipAddress.AddressFamily, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                this._socketLocal.Connect(endPoint);

                client = new Client(this._socketLocal);
                this._socketLocal.BeginReceive(client.Buffer, 0, Net.Tcp.Client.BufferLength, System.Net.Sockets.SocketFlags.None, new AsyncCallback(Recieve), client);
            }
            catch (Exception ex)
            {
                Xb.Util.Out("接続に失敗しました：" + ex.Message);
                throw new ArgumentException("接続に失敗しました：" + ex.Message);
            }

            //接続完了イベントをレイズする。
            this.FireEvent("Connected", new object[] { client.Socket });
        }


        /// <summary>
        /// リモートデータ受信時の処理
        /// </summary>
        /// <param name="ar"></param>
        /// <remarks></remarks>

        private void Recieve(IAsyncResult ar)
        {
            Client client = (Client)ar.AsyncState;
            byte[] recieveBytes = null;
            int length = 0;

            //受信データ長を取得する。
            try
            {
                length = client.Socket.EndReceive(ar);
            }
            catch (ObjectDisposedException)
            {
                //クライアントソケットが既に破棄されているとき、何もしない。
                return;
            }
            catch (Exception)
            {
                length = 0;
            }

            //切断通知か否かを検証する。
            if ((length <= 0))
            {
                //データが無いとき、切断されたと見做してリモートによる切断イベントをレイズする。
                client.Socket.Close();
                this.FireEvent("Disconnected", new object[] {});
                return;
            }

            //受診したデータを蓄積する。
            if (client.Buffer != null)
            {
                //クライアント用ストリームが存在しないとき、生成する。
                if (client.Stream == null)
                    client.Stream = new System.IO.MemoryStream();

                //クライアント用ストリームへ、バッファ内容を一括書き込みする。
                //第二引数に注意：ストリーム上のオフセットでなく、バッファByte配列上のオフセットになる。
                client.Stream.Write(client.Buffer, 0, length);
            }

            //ソケットオブジェクト側の受信サイズプロパティの値が 0 のとき、
            //一連のストリーム受信が完了したと見做す。
            if (client.Socket.Available == 0)
            {
                recieveBytes = client.Stream.ToArray();

                //クライアント用ストリームを破棄、更新する。
                client.Stream.Close();
                client.Stream = new System.IO.MemoryStream();

                //受信データが存在するとき、データ取得イベントをレイズする
                if ((recieveBytes.Length > 0))
                {
                    this.FireEvent("Recieved", new object[] {
                        recieveBytes,
                        client.Socket
                    });
                }
            }

            //クライアントソケットで再度データ待ち受けを開始する。
            try
            {
                client.Socket.BeginReceive(client.Buffer, 0, Net.Tcp.Client.BufferLength, System.Net.Sockets.SocketFlags.None, new AsyncCallback(Recieve), client);
            }
            catch (Exception)
            {
                //RSTフラグによる切断のとき、BeginRecieveに失敗する。
                client.Socket.Close();
                //リモートによる切断イベントをレイズする。
                this.FireEvent("Disconnected", new object[] {});
            }

        }


        /// <summary>
        /// 渡し値アドレスでサービスを開始する。
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <remarks></remarks>
        public void Listen(string ipAddress, int port)
        {
            //IPアドレスが渡されていないとき、もしくはポートがTCP範囲外のとき、異常終了する。
            if ((ipAddress == null || port < 0 || 65335 < port))
            {
                Xb.Util.Out("渡し値アドレス、もしくはポートが不正です。");
                throw new ArgumentException("渡し値アドレス、もしくはポートが不正です。");
            }

            System.Net.IPAddress[] addresses = null;
            int i = 0;
            int addIdx = 0;

            //渡し値アドレスの存在チェック
            addIdx = -1;
            addresses = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());
            for (i = 0; i <= addresses.Length - 1; i++)
            {
                if (addresses[i].ToString() == ipAddress)
                {
                    addIdx = i;
                    break;
                }
            }
            if (addIdx == -1)
            {
                Xb.Util.Out("渡し値アドレスが、ローカルデバイス上に見つかりません。");
                throw new ArgumentException("渡し値アドレスが、ローカルデバイス上に見つかりません。");
            }

            this.Listen(addresses[addIdx], port);
        }


        /// <summary>
        /// 渡し値アドレスでサービスを開始する。
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <remarks></remarks>
        public void Listen(System.Net.IPAddress ipAddress, int port)
        {
            //IPアドレスが不正のとき、異常終了。
            if ((ipAddress == null))
            {
                Xb.Util.Out("渡し値IPアドレスが不正です。");
                throw new ArgumentException("渡し値IPアドレスが不正です。");
            }

            //渡し値ポートがTCP範囲外のとき、異常終了。
            if ((port < 0 | 65335 < port))
            {
                Xb.Util.Out("渡し値ポートが不正です。");
                throw new ArgumentException("渡し値ポートが不正です。");
            }

            System.Net.IPAddress[] addresses = null;
            int i = 0;
            int addIdx = 0;
            System.Net.IPEndPoint endPoint = null;

            //渡し値アドレスの存在チェック
            addIdx = -1;
            addresses = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());
            for (i = 0; i <= addresses.Length - 1; i++)
            {
                if ((addresses[i].ToString() == ipAddress.ToString()))
                {
                    addIdx = i;
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
            if ((addIdx == -1))
            {
                Xb.Util.Out("渡し値アドレスが、ローカルデバイス上に見つかりません。");
                throw new ArgumentException("渡し値アドレスが、ローカルデバイス上に見つかりません。");
            }

            //ソケットを生成する。
            endPoint = new System.Net.IPEndPoint(ipAddress, port);
            this._socketLocal = null;
            this._socketLocal = new System.Net.Sockets.Socket(endPoint.Address.AddressFamily, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

            //サービスポートにバインドする。
            try
            {
                this._socketLocal.Bind(endPoint);
            }
            catch (Exception ex)
            {
                //バインド失敗時、ポートが使用中と思われる。エラー応答する。
                this._socketLocal = null;
                Xb.Util.Out("ソケットオープンに失敗しました：" + ex.Message);
                throw new ArgumentException("ソケットオープンに失敗しました：" + ex.Message);
            }

            //サービスを開始する。
            this._socketLocal.Listen(1000);
            this._socketLocal.BeginAccept(Accept, this._socketLocal);

        }

        /// <summary>
        /// サービスポートへの接続を受け付ける。
        /// </summary>
        /// <param name="ar"></param>
        /// <remarks></remarks>

        private void Accept(IAsyncResult ar)
        {
            System.Net.Sockets.Socket localSocket = null;
            System.Net.Sockets.Socket remoteSocket = null;
            Client client = null;

            try
            {
                localSocket = (System.Net.Sockets.Socket)ar.AsyncState;
                remoteSocket = localSocket.EndAccept(ar);

                //接続したソケットでデータ受付を開始する。
                client = new Client(remoteSocket);
                remoteSocket.BeginReceive(client.Buffer, 0, Net.Tcp.Client.BufferLength, System.Net.Sockets.SocketFlags.None, new AsyncCallback(Recieve), client);

                //新たにサービス受付用ソケットを生成、開始する。
                localSocket.BeginAccept(new AsyncCallback(Accept), localSocket);
            }
            catch (Exception)
            {
                return;
            }

            //接続完了イベントをレイズする。
            this.FireEvent("Connected", new object[] { client.Socket });
        }


        /// <summary>
        /// 接続先にデータを送付する。
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool Send(byte[] bytes)
        {

            //接続していないとき、異常終了する。
            if (this._socketLocal == null || bytes == null || !this._socketLocal.Connected)
                return false;

            try
            {
                this._socketLocal.Send(bytes);
            }
            catch (Exception)
            {
                return false;
            }

            return true;

        }


        /// <summary>
        /// 接続先にデータを送付する。
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool Send(string text)
        {

            if (this._socketLocal == null || text == null)
                return false;

            System.Text.Encoding encode = null;
            encode = System.Text.Encoding.UTF8;
            return this.Send(text, encode);

        }


        /// <summary>
        /// 接続先にデータを送付する。
        /// </summary>
        /// <param name="text"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool Send(string text, System.Text.Encoding encode)
        {

            if (this._socketLocal == null || text == null || encode == null)
                return false;

            return this.Send(encode.GetBytes(text));

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
                    if (this._socketLocal == null)
                        return;
                    if (!this._socketLocal.Connected)
                        return;

                    this._socketLocal.Close();
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
