using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Xb.App
{
    /// <summary>
    /// Log-Writer, instance based
    /// インスタンスベースのログ出力クラス
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class Logger : IDisposable
    {
        /// <summary>
        /// static log-writing method
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message
                             , string fileName = null
                             , string directory = null)
        {
            Logger.Init(ref fileName, ref directory);

            var fullPath = System.IO.Path.Combine(directory, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Append, FileAccess.Write))
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    try
                    {
                        var text = Logger.FormatMessage(message);
                        writer.WriteLine(text);
                        Xb.Util.Out(text);
                    }
                    catch (Exception ex)
                    {
                        Xb.Util.Out($"Xb.App.Logger.Log: write failure [{message}]");
                        Xb.Util.Out(ex);
                        throw ex;
                    }
                }
            }
        }


        /// <summary>
        /// Initialize log-file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="directory"></param>
        private static void Init(ref string fileName
                               , ref string directory)
        {
            fileName = string.IsNullOrEmpty(fileName)
                            ? DateTime.Now.ToString("yyyyMMdd") + "_log.txt"
                            : fileName;
            directory = directory ?? System.IO.Directory.GetCurrentDirectory();

            //not exist directory, try-create
            if (!System.IO.Directory.Exists(directory))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                catch (Exception ex)
                {
                    Xb.Util.Out($"Xb.App.Logger.Init: directory cannot create [{directory}]");
                    Xb.Util.Out(ex);
                    throw ex;
                }
            }

            var fullPath = System.IO.Path.Combine(directory, fileName);

            //not exist file, create
            if (!System.IO.File.Exists(fullPath))
            {
                Xb.File.Util.WriteText(fullPath
                                     , Logger.FormatMessage("create log file"));
            }
        }


        /// <summary>
        /// Format log-message string
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static string FormatMessage(string message)
        {
            return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} : {message}";
        }


        /// <summary>
        /// Log-file stream
        /// </summary>
        private System.IO.FileStream _stream;

        /// <summary>
        /// Stream writer
        /// </summary>
        private System.IO.StreamWriter _writer;

        /// <summary>
        /// Log-file path
        /// ログファイルのフルパス
        /// </summary>
        public string Path { get; private set; }


        /// <summary>
        /// Constructor
        /// コンストラクタ
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="directory"></param>
        /// <remarks></remarks>
        public Logger(string fileName = null, string directory = null)
        {
            Logger.Init(ref fileName, ref directory);

            this.Path = System.IO.Path.Combine(directory, fileName);

            //open file on append-mode
            this._stream = new System.IO.FileStream(this.Path
                                                  , FileMode.Append
                                                  , FileAccess.Write);
            this._writer = new System.IO.StreamWriter(this._stream
                                                    , System.Text.Encoding.UTF8);
        }


        /// <summary>
        /// Write message
        /// ログを書き出す。
        /// </summary>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public void Write(string message)
        {
            try
            {
                var text = Logger.FormatMessage(message);
                this._writer.WriteLine(text);
                Xb.Util.Out(text);
            }
            catch (Exception ex)
            {
                Xb.Util.Out($"Xb.App.Logger.Write: write failure [{message}]");
                Xb.Util.Out(ex);
                throw ex;
            }
        }


        /// <summary>
        /// Write message on async
        /// ログを書き出す。
        /// </summary>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public async Task WriteAsync(string message)
        {
            try
            {
                var text = Logger.FormatMessage(message);
                await this._writer.WriteLineAsync(text);
                Xb.Util.Out(text);
            }
            catch (Exception ex)
            {
                Xb.Util.Out($"Xb.App.Logger.WriteAsync: write failure [{message}]");
                Xb.Util.Out(ex);
                throw ex;
            }
        }


        // 重複する呼び出しを検出するには
        private bool _disposedValue;

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    if (this._writer != null)
                    {
                        try
                        { this._writer.Dispose(); }
                        catch (Exception){}
                    }

                    if (this._stream != null)
                    {
                        try
                        { this._stream.Dispose(); }
                        catch (Exception) { }
                    }

                    this.Path = null;
                }
            }
            this._disposedValue = true;
        }

        #region "IDisposable Support"
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
