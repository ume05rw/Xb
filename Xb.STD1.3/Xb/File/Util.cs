﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xb.File
{
    public class Util
    {
        /// <summary>
        /// Get file-bytes array
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static byte[] GetBytes(string fileName)
        {
            if(!System.IO.File.Exists(fileName))
                throw new FileNotFoundException($"Xb.File.Util.GetFileBytes: file not found [{fileName}]");

            var stream = new System.IO.FileStream(fileName
                                                , FileMode.Open
                                                , FileAccess.Read);
            var buffer = new byte[1024];
            var result = new List<byte>();
            while (true)
            {
                var size = stream.Read(buffer, 0, buffer.Length);
                if (size == 0)
                    break;

                result.AddRange(size == buffer.Length 
                                    ? buffer 
                                    : buffer.Take(size));
            }

            stream.Dispose();

            return result.ToArray();
        }


        /// <summary>
        /// Get file-bytes array on async
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async static Task<byte[]> GetBytesAsync(string fileName)
        {
            byte[] result = null;
            await Task.Run(() =>
            {
                result = Xb.File.Util.GetBytes(fileName);
            });
            return result;
        }


        /// <summary>
        /// Get file-text, auto-detect encoding
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetText(string fileName)
        {
            var bytes = Xb.File.Util.GetBytes(fileName);
            var encoding = Xb.Str.GetEncode(bytes);
            return encoding.GetString(bytes);
        }


        /// <summary>
        /// Get file-text on async, auto-detect encoding
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async static Task<string> GetTextAsync(string fileName)
        {
            string result = null;
            await Task.Run(() =>
            {
                result = Xb.File.Util.GetText(fileName);
            });
            return result;
        }


        /// <summary>
        /// Get file-text with passed encoding
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string GetText(string fileName
                                   , Encoding encoding)
        {
            if (!System.IO.File.Exists(fileName))
                throw new FileNotFoundException($"Xb.File.Util.GetFileBytes: file not found [{fileName}]");

            if(encoding == null)
                throw new ArgumentNullException(nameof(encoding), $"Xb.File.Util.GetFileBytes: encoding null");

            var stream = new System.IO.FileStream(fileName
                                                , FileMode.Open
                                                , FileAccess.Read);
            var reader = new StreamReader(stream, encoding);
            var result = reader.ReadToEnd();

            reader.Dispose();
            stream.Dispose();

            return result;
        }


        /// <summary>
        /// Get file-text with passed encoding on async
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public async static Task<string> GetTextAsync(string fileName
                                                    , Encoding encoding)
        {
            string result = null;
            await Task.Run(() =>
            {
                result = Xb.File.Util.GetText(fileName, encoding);
            });
            return result;
        }


        /// <summary>
        /// Write byte-array to file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        public static void WriteBytes(string fileName
                                    , byte[] bytes)
        {
            bytes = bytes ?? (new byte[] {});
            
            var stream = new System.IO.FileStream(fileName
                                                , FileMode.Create
                                                , FileAccess.Write);
            stream.Write(bytes, 0, bytes.Length);
            stream.Dispose();
        }


        /// <summary>
        /// Write byte-array to file on async
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public async static Task WriteBytesAsync(string fileName
                                               , byte[] bytes)
        {
            await Task.Run(() =>
            {
                Xb.File.Util.WriteBytes(fileName, bytes);
            });
        }


        /// <summary>
        /// Write text to file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="text"></param>
        /// <param name="encoding"></param>
        public static void WriteText(string fileName
                                   , string text
                                   , Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            text = text ?? "";

            Xb.File.Util.WriteBytes(fileName, encoding.GetBytes(text));
        }


        /// <summary>
        /// Write text to file on async
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="text"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public async static Task WriteTextAsync(string fileName
                                              , string text
                                              , Encoding encoding = null)
        {
            await Task.Run(() =>
            {
                Xb.File.Util.WriteText(fileName, text, encoding);
            });
        }
    }
}
