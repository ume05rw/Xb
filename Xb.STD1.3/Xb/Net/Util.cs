using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Xb.Net
{
    /// <summary>
    /// ネットワーク関連クラス、関数群
    /// </summary>
    /// <remarks></remarks>
    public class Util
    {
        /// <summary>
        /// PHPのmd5関数と同じ形式のMD5ハッシュを生成する。
        /// </summary>
        /// <param name="text"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string Md5(string text
                               , Encoding encoding = null)
        {
            text = text ?? "";
            encoding = encoding ?? Encoding.UTF8;
            
            System.Security.Cryptography.HMACMD5 cryptHandler = null;
            var result = "";

            var textBytes = encoding.GetBytes(text);
            cryptHandler = new System.Security.Cryptography.HMACMD5();
            var hash = cryptHandler.ComputeHash(textBytes);

            foreach (byte elem in hash)
            {
                if (elem < 16)
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
        /// <param name="encoding"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string Sha1(string text
                                , Encoding encoding = null)
        {
            text = text ?? "";
            encoding = encoding ?? Encoding.UTF8;
            
            var textBytes = encoding.GetBytes(text);
            var cryptHandler = new System.Security.Cryptography.HMACSHA1();
            var hash = cryptHandler.ComputeHash(textBytes);

            var result = "";
            foreach (byte elem in hash)
            {
                if (elem < 16)
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
            var addresses = Task.Run(() => System.Net.Dns.GetHostAddressesAsync(System.Net.Dns.GetHostName()))
                                .GetAwaiter()
                                .GetResult();
            
            if (addresses == null)
                return new System.Net.IPAddress[]{};

            return excludeLoopback
                ? addresses.Where(add => !IPAddress.IsLoopback(add)).ToArray()
                : addresses;
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

            var addresses = Xb.Net.Util.GetIpAddresses();
            if (addresses == null)
                return false;

            return addresses.Any(add => add.ToString() == address);
        }


        /// <summary>
        /// ホスト名からIPアドレスを取得する。
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static System.Net.IPAddress[] GetIpAddresses(string hostName)
        {
            var entry = Task.Run(() => System.Net.Dns.GetHostEntryAsync(hostName))
                            .GetAwaiter()
                            .GetResult();
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
            var entry = Task.Run(() => System.Net.Dns.GetHostEntryAsync(ipAddress))
                            .GetAwaiter()
                            .GetResult();
            return entry.HostName;
        }
    }
}
