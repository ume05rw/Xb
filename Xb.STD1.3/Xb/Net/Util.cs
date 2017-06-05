using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

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
            
            var result = "";

            var textBytes = encoding.GetBytes(text);
            var cryptHandler = System.Security.Cryptography.MD5.Create();
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
            var cryptHandler = System.Security.Cryptography.SHA1.Create();
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
        /// Get ip addresses on local devices(deprecated)
        /// ローカルデバイスのIPアドレスを全て取得する。
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Please switch to GetLocalIpAddresses
        /// </remarks>
        public static System.Net.IPAddress[] GetIpAddresses(bool excludeLoopback = false)
        {
            return Util.GetLocalIpAddresses(excludeLoopback);
        }


        /// <summary>
        /// ローカルデバイスのIPアドレスを全て取得する。
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static System.Net.IPAddress[] GetLocalIpAddresses(bool excludeLoopback = false)
        {
            //ローカルホスト名の解決に失敗することがあるため、
            //NetworkInterfaceからデバイス情報を取得するように修正。
            var addresses = NetworkInterface.GetAllNetworkInterfaces()
                                            .SelectMany(itf => itf.GetIPProperties().UnicastAddresses)
                                            .Select(uaddr => uaddr.Address);
            //var addresses = Task.Run(() => System.Net.Dns.GetHostAddressesAsync(System.Net.Dns.GetHostName()))
            //                    .GetAwaiter()
            //                    .GetResult();

            return excludeLoopback
                ? addresses.Where(add => !IPAddress.IsLoopback(add)).ToArray()
                : addresses.ToArray();
        }


        /// <summary>
        /// ローカルデバイスのIPv4アドレスを全て取得する。
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static System.Net.IPAddress[] GetLocalV4Addresses(bool excludeLoopback = false)
        {
            var addresses = NetworkInterface.GetAllNetworkInterfaces()
                                            .SelectMany(itf => itf.GetIPProperties().UnicastAddresses)
                                            .Select(uaddr => uaddr.Address)
                                            .Where(addr => addr.AddressFamily == AddressFamily.InterNetwork);
            
            return excludeLoopback
                ? addresses.Where(add => !IPAddress.IsLoopback(add)).ToArray()
                : addresses.ToArray();
        }


        /// <summary>
        /// ローカルデバイスのIPv6アドレスを全て取得する。
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static System.Net.IPAddress[] GetLocalV6Addresses(bool excludeLoopback = false)
        {
            var addresses = NetworkInterface.GetAllNetworkInterfaces()
                                            .SelectMany(itf => itf.GetIPProperties().UnicastAddresses)
                                            .Select(uaddr => uaddr.Address)
                                            .Where(addr => addr.AddressFamily == AddressFamily.InterNetworkV6);
            
            return excludeLoopback
                ? addresses.Where(add => !IPAddress.IsLoopback(add)).ToArray()
                : addresses.ToArray();
        }


        /// <summary>
        /// ローカルデバイスのIPv4プライベートアドレスを全て取得する。
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static System.Net.IPAddress[] GetLocalV4PrivateAddresses()
        {
            if (Util._privateAddressHeaders == null)
                Util.InitPrivateAddressHeaders();

            return NetworkInterface.GetAllNetworkInterfaces()
                                   .SelectMany(itf => itf.GetIPProperties().UnicastAddresses)
                                   .Select(uaddr => uaddr.Address)
                                   .Where(addr => addr.AddressFamily == AddressFamily.InterNetwork
                                                  && !IPAddress.IsLoopback(addr)
                                                  && Util._privateAddressHeaders
                                                         .Any(paddr => addr.ToString().StartsWith(paddr)))
                                   .ToArray();
        }


        private static string[] _privateAddressHeaders = null;
        /// <summary>
        /// IPv4プライベートアドレス判定用プレフィクス文字列を生成する。
        /// </summary>
        private static void InitPrivateAddressHeaders()
        {
            var list = new List<string>()
            {
                "10.",
                "192."
            };

            for (var i = 16; i <= 31; i++)
                list.Add($"172.{i}.");

            Util._privateAddressHeaders = list.ToArray();
        }


        /// <summary>
        /// 渡し値アドレスがIPv4プライベートアドレスか否かを判定する。
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool IsV4PrivateAddress(string address)
        {
            if (Util._privateAddressHeaders == null)
                Util.InitPrivateAddressHeaders();

            return Util._privateAddressHeaders.Any(paddr => address.StartsWith(paddr));
        }


        /// <summary>
        /// 渡し値アドレスがIPv4プライベートアドレスか否かを判定する。
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool IsV4PrivateAddress(IPAddress address)
        {
            if (Util._privateAddressHeaders == null)
                Util.InitPrivateAddressHeaders();

            //IPv4でないならfalse
            if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                return false;

            return Util._privateAddressHeaders.Any(paddr => address.ToString().StartsWith(paddr));
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
            
            return Xb.Net.Util.GetIpAddresses()
                              .Any(add => add.ToString() == address);
        }


        /// <summary>
        /// 渡し値アドレスが、ローカルデバイスが保持しているIPアドレスのどれかに合致するか否かを検証する。
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool IsMyIpAddress(IPAddress address)
        {
            if (address == null)
                return false;
            
            return Xb.Net.Util.GetIpAddresses()
                              .Any(add => add.ToString() == address.ToString());
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
