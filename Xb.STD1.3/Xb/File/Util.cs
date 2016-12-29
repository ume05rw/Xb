using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xb.File
{
    public class Util
    {
        /// <summary>
        /// Remove file, directory recursive
        /// ファイル／ディレクトリを再帰的に削除する
        /// </summary>
        /// <param name="path"></param>
        /// <param name="force"></param>
        /// <remarks>C# recursive option bug, FUCK!</remarks>
        public static void Delete(string path, bool force = false)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                return;
            }

            if (Directory.Exists(path))
            {
                //remove files
                var filePaths = Directory.GetFiles(path);
                foreach (var filePath in filePaths)
                {
                    if(force)
                        System.IO.File.SetAttributes(filePath, FileAttributes.Normal);

                    System.IO.File.Delete(filePath);
                }

                //remove directory 
                var directoryPaths = Directory.GetDirectories(path);
                foreach (var directoryPath in directoryPaths)
                {
                    Xb.File.Util.Delete(directoryPath);
                }

                Directory.Delete(path);
            }
        }


        /// <summary>
        /// Get file-bytes array
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static byte[] GetBytes(string fileName)
        {
            if(!System.IO.File.Exists(fileName))
                throw new FileNotFoundException($"Xb.File.Util.GetFileBytes: file not found [{fileName}]");

            byte[] result;
            using (var stream = new System.IO.FileStream(fileName
                                                       , FileMode.Open
                                                       , FileAccess.Read))
            {
                result = Xb.Byte.GetBytes(stream);
            }

            return result;
        }


        /// <summary>
        /// Get file-bytes array on async
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<byte[]> GetBytesAsync(string fileName)
        {
            return await Task.Run(() => Xb.File.Util.GetBytes(fileName));
        }


        /// <summary>
        /// Get file-text, auto-detect encoding for Japanese
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
        /// Get file-text on async, auto-detect encoding for Japanese
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<string> GetTextAsync(string fileName)
        {
            return await Task.Run(() => Xb.File.Util.GetText(fileName));
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

            string result = "";
            using (var stream = new System.IO.FileStream(fileName
                                                       , FileMode.Open
                                                       , FileAccess.Read))
            {
                using (var reader = new StreamReader(stream, encoding))
                {
                    result = reader.ReadToEnd();
                }
            }

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
            return await Task.Run(() => Xb.File.Util.GetText(fileName, encoding));
        }


        /// <summary>
        /// Write byte-array to file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        public static void WriteBytes(string fileName
                                    , byte[] bytes)
        {
            //bytesが空でも書き込みは実行する。
            //中身を削除したい場合が有り得る。
            bytes = bytes ?? new byte[] {};
            
            using (var stream = new System.IO.FileStream(fileName
                                                       , FileMode.Create
                                                       , FileAccess.Write))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
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
            await Task.Run(() => Xb.File.Util.WriteText(fileName, text, encoding));
        }




        /// <summary>
        /// Zip directory
        /// 渡し値フォルダをzipファイル化する
        /// </summary>
        /// <param name="directory"></param>
        /// <remarks>
        /// https://msdn.microsoft.com/ja-jp/library/system.io.compression.zipfile(v=vs.110).aspx
        /// </remarks>
        public static void ToZip(string directory
                               , Encoding encoding = null)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Xb.File.Zip.ToZip: directory not found [{directory}]");

            encoding = encoding ?? Encoding.UTF8;

            var dirInfo = new DirectoryInfo(directory);
            var parentDirectory = dirInfo.Parent.FullName;
            var zipFileName = Path.Combine(parentDirectory, $"{dirInfo.Name}.zip");

            Xb.File.Util.Delete(zipFileName);

            ZipFile.CreateFromDirectory(dirInfo.FullName
                                      , zipFileName
                                      , CompressionLevel.Fastest
                                      , true
                                      , encoding);
        }


        /// <summary>
        /// Unzip archive
        /// zipファイルを解凍する
        /// </summary>
        /// <param name="zipFileName"></param>
        public static void Unzip(string zipFileName
                               , Encoding encoding = null)
        {
            if (!System.IO.File.Exists(zipFileName))
                throw new FileNotFoundException($"Xb.File.Zip.Unzip: zip-file not found [{zipFileName}]");

            encoding = encoding ?? Encoding.UTF8;

            var fileInfo = new System.IO.FileInfo(zipFileName);
            var parentDirectory = fileInfo.DirectoryName;
            var unzipedDirectory
                = Path.Combine(parentDirectory
                             , fileInfo.Name
                                       .Substring(0
                                                , fileInfo.Name.Length
                                                    - fileInfo.Extension.Length));

            Xb.File.Util.Delete(unzipedDirectory);

            ZipFile.ExtractToDirectory(fileInfo.FullName
                                     , parentDirectory
                                     , encoding);
        }
    }
}
