using System;
using System.Collections.Generic;
using System.Net;

namespace Xb.Net
{
    /// <summary>
    /// ネットワーク関連クラス、関数群
    /// </summary>
    /// <remarks></remarks>
    public class Util
    {
        /// <summary>
        /// 渡し値URLで取得したデータをファイルに書き出す。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fileName"></param>
        /// <param name="directory"></param>
        /// <param name="isOverWrite"></param>
        /// <returns></returns>
        /// <remarks>
        /// HTMLテキスト取得時と画像等のファイル取得時でHTTPヘッダが異なり、今後の業界動向によって
        /// 調整が必要になりそうなので、ほぼ同じ処理でもGetHttpStringと共通化せずに置いておく。
        /// </remarks>
        public static bool GetHttpFile(string url, string fileName = null, string directory = null, bool isOverWrite = true)
        {

            //ファイル名が渡されていないとき、URLからファイル名を切り出す。
            if ((fileName == null))
                fileName = Xb.Net.Http.GetFilename(url);

            //ディレクトリ名が渡されていないとき、実行ファイルのディレクトリを使用する。
            if ((directory == null))
            {
                directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }

            string fileNameFullPath = directory + "\\" + fileName;

            //渡し値のファイルが現在存在するか否かで処理を分岐する。
            if ((System.IO.File.Exists(fileNameFullPath)))
            {
                //渡し値の上書き可／不可によって、処理を分岐する。
                if ((isOverWrite))
                {
                    //上書き可のとき、既存ファイルを削除する。
                    try
                    {
                        System.IO.File.Delete(fileNameFullPath);
                    }
                    catch (Exception)
                    {
                        //ファイルの削除に失敗したとき、異常終了する。
                        return false;
                    }
                }
                else
                {
                    //上書き不可のとき、異常終了する。
                    return false;
                }
            }

            System.IO.FileStream fileStream = null;
            System.Net.HttpWebRequest request = null;
            System.Net.WebResponse response = null;
            System.IO.Stream responseStream = null;
            int retryCount = 0;
            int bt = 0;
            int i = 0;

            retryCount = 10;
            response = null;
            responseStream = null;

            //Httpクエリを生成する
            request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; ja; rv:1.9.2.11) Gecko/20101012 Firefox/3.6.11";
            request.Headers.Add(System.Net.HttpRequestHeader.AcceptLanguage, "ja,en-us;q=0.7,en;q=0.3");
            request.Headers.Add(System.Net.HttpRequestHeader.AcceptCharset, "Shift_JIS,utf-8;q=0.7,*;q=0.7");
            request.Accept = "*/*;q=0.9";
            //"image/png,image/*;q=0.8,*/*;q=0.5"
            request.KeepAlive = true;
            request.Referer = "http://www.google.co.jp/";
            request.Timeout = int.MaxValue;
            //request.Headers.Add(System.Net.HttpRequestHeader.AcceptEncoding, "gzip,deflate")

            //Httpの応答を取得する。
            //リトライ回数分だけ、接続を試す。
            for (i = 0; i <= retryCount; i++)
            {
                try
                {
                    response = request.GetResponse();
                    break;
                }
                catch (Exception)
                {
                    if (i >= retryCount)
                    {
                        request = null;
                        return false;
                    }
                }

                //ウエイトを入れる
                //Windows.Forms.Application.DoEvents()
                System.Threading.Thread.Sleep(1000 * i);
            }

            //リトライ回数分だけ、データ取得を試す。
            for (i = 0; i <= retryCount; i++)
            {
                try
                {
                    responseStream = response.GetResponseStream();
                    break;
                }
                catch (Exception)
                {
                    if (i >= retryCount)
                    {
                        request = null;
                        response.Close();
                        response = null;
                        return false;
                    }
                }

                //ウエイトを入れる
                System.Threading.Thread.Sleep(500 * i);
            }

            //ファイルに書き込むためのFileStreamを作成
            fileStream = new System.IO.FileStream(fileNameFullPath, System.IO.FileMode.Create, System.IO.FileAccess.Write);

            //応答データをファイルに書き込む
            while ((true))
            {
                try
                {
                    bt = responseStream.ReadByte();
                }
                catch (Exception)
                {
                    request = null;
                    response.Close();
                    response = null;
                    responseStream.Close();
                    responseStream.Dispose();
                    responseStream = null;
                    return false;
                }

                if (bt == -1)
                    break;

                fileStream.WriteByte(Convert.ToByte(bt));
            }

            //閉じる
            fileStream.Close();
            request = null;
            response.Close();
            response = null;
            responseStream.Close();
            responseStream = null;

            return true;
        }


        /// <summary>
        /// PHPのmd5関数と同じ形式のMD5ハッシュを生成する。
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string Md5(string text)
        {
            if (text == null)
                text = "";

            byte[] textBytes = null;
            byte[] hash = null;
            System.Security.Cryptography.MD5CryptoServiceProvider cryptHandler = null;
            string result = "";

            textBytes = System.Text.Encoding.Default.GetBytes(text);
            cryptHandler = new System.Security.Cryptography.MD5CryptoServiceProvider();
            hash = cryptHandler.ComputeHash(textBytes);

            foreach (byte elem in hash)
            {
                if ((elem < 16))
                {
                    result += "0" + elem.ToString("x");
                }
                else
                {
                    result += elem.ToString("x");
                }
            }
            return result;
        }


        /// <summary>
        /// PHPのsha1関数と同じ形式のMD5ハッシュを生成する。
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string Sha1(string text)
        {
            if (text == null)
                text = "";

            byte[] textBytes = null;
            byte[] hash = null;
            System.Security.Cryptography.SHA1CryptoServiceProvider cryptHandler = null;
            string result = "";

            textBytes = System.Text.Encoding.Default.GetBytes(text);
            cryptHandler = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            hash = cryptHandler.ComputeHash(textBytes);

            foreach (byte elem in hash)
            {
                if ((elem < 16))
                {
                    result += "0" + elem.ToString("x");
                }
                else
                {
                    result += elem.ToString("x");
                }
            }

            return result;
        }


        /// <summary>
        /// ローカルデバイスのIPアドレスを全て取得する。
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static System.Net.IPAddress[] GetIpAddresses(bool excludeLoopback = false)
        {
            System.Net.IPAddress[] addresses = null;
            List<System.Net.IPAddress> adds = null;
            List<System.Net.IPAddress> excluded = null;

            addresses = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());

            if (addresses == null)
                return new System.Net.IPAddress[]{};

            if (!excludeLoopback)
                return addresses;

            adds = new List<IPAddress>();
            excluded = new List<IPAddress>();

            adds.AddRange(addresses);
            foreach (IPAddress address in adds)
            {
                if (System.Net.IPAddress.IsLoopback(address))
                    excluded.Add(address);
            }
            foreach (IPAddress exc in excluded)
            {
                adds.Remove(exc);
            }

            return adds.ToArray();
        }


        /// <summary>
        /// 渡し値アドレスが、ローカルデバイスが保持しているIPアドレスのどれかに合致するか否かを検証する。
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool IsMyIpAddress(string address)
        {
            if (address == null)
                return false;

            System.Net.IPAddress[] adds = Net.Util.GetIpAddresses();
            if (adds == null)
                return false;

            //FW2.0互換のため、Linq使用せず。
            foreach (System.Net.IPAddress @add in adds)
            {
                if (@add.ToString() == address)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// ホスト名からIPアドレスを取得する。
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static System.Net.IPAddress[] GetIpAddresses(string hostName)
        {
            System.Net.IPHostEntry entry = System.Net.Dns.GetHostEntry(hostName);
            return entry.AddressList;
        }


        /// <summary>
        /// IPアドレスからホスト名を取得する。
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetHostname(string ipAddress)
        {
            System.Net.IPHostEntry entry = System.Net.Dns.GetHostEntry(ipAddress);
            return entry.HostName;
        }


        ///// <summary>
        ///// ファイアウォールがアクティブか否かを検証する。
        ///// </summary>
        ///// <returns></returns>
        ///// <remarks>
        ///// ダメっぽい。ローカルI/F同士ではWindowsFirewallは阻害しない様子。
        ///// どうしたものか。
        ///// </remarks>
        //public static bool IsFirewallActive()
        //{
        //    System.Random rnd = new System.Random();
        //    int port = 0;
        //    Net.Tcp server = default(Net.Tcp);
        //    Net.Tcp client = default(Net.Tcp);
        //    System.Net.IPAddress[] addresses = Net.Util.GetIpAddresses(true);

        //    try
        //    {
        //        foreach (System.Net.IPAddress @add in addresses)
        //        {
        //            for (int i = 0; i <= 3; i++)
        //            {
        //                port = rnd.Next(1024, 65535);

        //                server = new Net.Tcp();
        //                server.Listen(@add, port);

        //                client = new Net.Tcp();
        //                client.Connect(@add, port);

        //                client.Dispose();
        //                server.Dispose();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return true;
        //    }

        //    return false;
        //}
    }
}
