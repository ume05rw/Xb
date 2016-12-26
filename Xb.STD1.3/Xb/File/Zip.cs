using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xb.File
{
    public class Zip : IDisposable
    {
        /// <summary>
        /// Zip compression level
        /// Zip圧縮強度
        /// </summary>
        public static CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Fastest;

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
                                      , Zip.CompressionLevel
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




        private Stream _stream;
        private ZipArchive _archive;
        private Encoding _encoding;

        /// <summary>
        /// Operation Type
        /// 書き込ませないか否か
        /// </summary>
        public bool ReadOnly { get; private set; }

        /// <summary>
        /// Entries
        /// </summary>
        public ReadOnlyCollection<ZipArchiveEntry> Entries => this._archive.Entries;

        /// <summary>
        /// Constructor
        /// コンストラクタ
        /// </summary>
        public Zip(string zipFileName
                 , bool readOnly = true
                 , Encoding encoding = null)
        {
            this._encoding = encoding ?? Encoding.UTF8;
            this.ReadOnly = readOnly;

            if (this.ReadOnly
                && !System.IO.File.Exists(zipFileName))
            {
                throw new FileNotFoundException($"Xb.File.Zip.Unzip: zip-file not found [{zipFileName}]");
            }

            //not exist zip file, create
            if (!System.IO.File.Exists(zipFileName))
            {
                using (var stream = new FileStream(zipFileName
                                                 , FileMode.CreateNew
                                                 , FileAccess.Write))
                {
                    using (var archive = new ZipArchive(stream
                                                      , ZipArchiveMode.Create))
                    {
                        //create only
                    }
                }
            }

            if (this.ReadOnly)
            {
                this._stream = new FileStream(zipFileName
                                            , FileMode.Open
                                            , FileAccess.Read);
                this._archive = new ZipArchive(this._stream
                                             , ZipArchiveMode.Read
                                             , false
                                             , this._encoding);
            }
            else
            {
                this._stream = new FileStream(zipFileName
                                            , FileMode.Open
                                            , FileAccess.ReadWrite);
                this._archive = new ZipArchive(this._stream
                                             , ZipArchiveMode.Update
                                             , false
                                             , this._encoding);
            }
        }


        /// <summary>
        /// Constructor
        /// コンストラクタ
        /// </summary>
        public Zip(Stream readableStream
                 , Encoding encoding = null)
        {
            this._encoding = encoding ?? Encoding.UTF8;
            this.ReadOnly = true;


            if (readableStream == null)
            {
                throw new ArgumentNullException(nameof(readableStream), $"Xb.File.Zip.Constructor: readableStream null");
            }

            this._stream = readableStream;
            this._archive = new ZipArchive(this._stream
                                         , ZipArchiveMode.Read
                                         , false
                                         , this._encoding);
        }


        /// <summary>
        /// Get byte-array in zip
        /// Zipエントリの内容をバイト配列で取得する。
        /// </summary>
        /// <param name="entry"></param>
        /// <returns>directory return zero-length array</returns>
        public byte[] GetBytes(ZipArchiveEntry entry)
        {
            if(!this._archive.Entries.Contains(entry))
                throw new ArgumentOutOfRangeException(nameof(entry), $"Xb.File.Zip.GetBytes: entry not found [{entry}]");

            var result = new List<byte>();
            using (var stream = entry.Open())
            {
                using (var reader = new StreamReader(stream))
                {
                    var buffer = new byte[1024];

                    while (true)
                    {
                        var size = stream.Read(buffer, 0, buffer.Length);
                        if (size == 0)
                            break;

                        result.AddRange(size == buffer.Length
                                            ? buffer
                                            : buffer.Take(size));
                    }
                }
            }

            return result.ToArray();
        }


        /// <summary>
        /// Get byte-array in zip on async
        /// Zipエントリの内容をバイト配列で取得する。
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public async Task<byte[]> GetBytesAsync(ZipArchiveEntry entry)
        {
            return await Task.Run(() => this.GetBytes(entry));
        }


        /// <summary>
        /// Delete entry
        /// Zipファイル内のエントリを削除する
        /// </summary>
        /// <param name="entry"></param>
        public void Delete(ZipArchiveEntry entry)
        {
            if (this.ReadOnly)
                throw new InvalidOperationException("Xb.File.Zip.WriteBytes: read-only");

            if (!this._archive.Entries.Contains(entry))
                throw new ArgumentOutOfRangeException(nameof(entry), $"Xb.File.Zip.Delete: entry not found [{entry}]");

            entry.Delete();
        }


        /// <summary>
        /// Delete entry on async
        /// Zipファイル内のエントリを削除する
        /// </summary>
        /// <param name="entry"></param>
        public async Task DeleteAsync(ZipArchiveEntry entry)
        {
            await Task.Run(() => { this.Delete(entry); });
        }


        /// <summary>
        /// Create new entry in archive
        /// </summary>
        /// <param name="entryName"></param>
        /// <returns></returns>
        public ZipArchiveEntry GetNewEntry(string entryName)
        {
            return this._archive.CreateEntry(entryName);
        }

        /// <summary>
        /// Overwrite entry
        /// Zipファイル内のエントリを上書きする
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="bytes"></param>
        /// <returns>Updated(New) Entry</returns>
        /// <remarks>
        /// entry are REPLACED.
        /// When overwriting entry a value smaller than the existing data,
        /// remains surplus of existing data.
        /// </remarks>
        public ZipArchiveEntry WriteBytes(ZipArchiveEntry entry
                                        , byte[] bytes)
        {
            if(this.ReadOnly)
                throw new InvalidOperationException("Xb.File.Zip.WriteBytes: read-only");

            var fullName = entry.FullName;
            if (this._archive.Entries.Contains(entry))
                this.Delete(entry);

            entry = this._archive.CreateEntry(fullName);

            using (var stream = entry.Open())
            {
                using (var writer = new StreamWriter(stream))
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            return entry;
        }


        /// <summary>
        /// Overwrite entry on async
        /// Zipファイル内のエントリを上書きする
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="bytes"></param>
        /// <returns>Updated(New) Entry</returns>
        public async Task<ZipArchiveEntry> WriteBytesAsync(ZipArchiveEntry entry
                                                         , byte[] bytes)
        {
            return await Task.Run(() => this.WriteBytes(entry, bytes));
        }


        ///// <summary>
        ///// Get byte-array in zip
        ///// Zipエントリの内容をバイト配列で取得する。
        ///// </summary>
        ///// <param name="entryString"></param>
        ///// <returns></returns>
        //public byte[] GetBytes(string entryString)
        //{
        //    var entry = this._archive.Entries.FirstOrDefault(ent => ent.FullName == entryString);
        //    if(entry == null)
        //        throw new ArgumentOutOfRangeException(nameof(entry), $"Xb.File.Zip.GetBytes: entry not found [{entry}]");
        //    return this.GetBytes(entry);
        //}
        ///// <summary>
        ///// Get byte-array in zip on async
        ///// Zipエントリの内容をバイト配列で取得する。
        ///// </summary>
        ///// <param name="entryString"></param>
        ///// <returns></returns>
        //public async Task<byte[]> GetBytesAsync(string entryString)
        //{
        //    return await Task.Run(() => this.GetBytes(entryString));
        //}
        ///// <summary>
        ///// Delete entry
        ///// Zipファイル内のエントリを削除する
        ///// </summary>
        ///// <param name="entryString"></param>
        //public void Delete(string entryString)
        //{
        //    if (this.ReadOnly)
        //        throw new InvalidOperationException("Xb.File.Zip.WriteBytes: read-only");
        //    var entry = this._archive.Entries.FirstOrDefault(ent => ent.FullName == entryString);
        //    if (entry == null)
        //        throw new ArgumentOutOfRangeException(nameof(entryString), $"Xb.File.Zip.Delete: entry not found [{entryString}]");
        //    this.Delete(entry);
        //}
        ///// <summary>
        ///// Delete entry on async
        ///// Zipファイル内のエントリを削除する
        ///// </summary>
        ///// <param name="entryString"></param>
        //public async Task DeleteAsync(string entryString)
        //{
        //    await Task.Run(() => { this.Delete(entryString); });
        //}
        ///// <summary>
        ///// Overwrite entry on async
        ///// Zipファイル内のエントリを上書きする
        ///// </summary>
        ///// <param name="entryString"></param>
        ///// <param name="bytes"></param>
        //public void WriteBytes(string entryString
        //                     , byte[] bytes)
        //{
        //    if (this.ReadOnly)
        //        throw new InvalidOperationException("Xb.File.Zip.WriteBytes: read-only");
        //    var entry = this._archive.Entries.FirstOrDefault(ent => ent.FullName == entryString);
        //    if (entry == null)
        //        entry = this._archive.CreateEntry(entryString);
        //    this.WriteBytes(entry, bytes);
        //}
        ///// <summary>
        ///// Overwrite entry on async
        ///// Zipファイル内のエントリを上書きする
        ///// </summary>
        ///// <param name="entryString"></param>
        ///// <param name="bytes"></param>
        //public async Task WriteBytesAsync(string entryString
        //                                , byte[] bytes)
        //{
        //    await Task.Run(() => { this.WriteBytes(entryString, bytes); });
        //}



        #region IDisposable Support

        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    { this._archive.Dispose(); }
                    catch (Exception){}

                    try
                    { this._stream.Dispose(); }
                    catch (Exception) {}

                    this._encoding = null;

                }   
                disposedValue = true;
            }
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
