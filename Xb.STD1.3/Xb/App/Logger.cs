using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Xb.App
{
    /// <summary>
    /// Log-Writer
    /// ログ出力クラス
    /// </summary>
    public class Logger : IDisposable
    {
        /// <summary>
        /// Write log on static
        /// ログを書き出す。
        /// </summary>
        /// <param name="message"></param>
        public static void Out(string message)
        {
            Logger.StaticOutput(Logger.FormatMessage(message));
        }

        /// <summary>
        /// Write formatted-log on static
        /// 値を差し込んだ文字列をログに書き出す。
        /// </summary>
        /// <param name="format"></param>
        /// <param name="values"></param>
        public static void Out(string format, params object[] values)
        {
            Logger.StaticOutput(Logger.FormatMessage(string.Format(format, values)));
        }

        /// <summary>
        /// Write Exception-info log on static
        /// 例外情報をログに書き出す。
        /// </summary>
        /// <param name="ex"></param>
        public static void Out(Exception ex)
        {
            Logger.StaticOutput(Logger.GetHighlightedText(Xb.Util.GetErrorString(ex)));
        }

        /// <summary>
        /// Get hilighted text
        /// </summary>
        /// <param name="messages"></param>
        private static string GetHighlightedText(params string[] messages)
        {
            var time = DateTime.Now;
            var list = new List<string>();

            list.Add("");
            list.Add("");
            list.Add(time.ToString("HH:mm:ss.fff") + ":");
            list.Add("##################################################");
            list.Add("#");

            foreach (string message in messages)
            {
                var lines = message.Replace("\r\n", "\n").Replace("\r", "\n").Trim('\n').Split('\n');
                foreach (var line in lines)
                {
                    list.Add($"# {line}");
                }
            }

            list.Add("#");
            list.Add("##################################################");
            list.Add("");
            list.Add("");

            return string.Join("\r\n", list);
        }

        /// <summary>
        /// output message to log file on static
        /// </summary>
        /// <param name="message"></param>
        private static void StaticOutput(string message)
        {
            var fileName = $"xbstlogger_{DateTime.Now:yyyyMMdd}.log";
            var directory = System.IO.Directory.GetCurrentDirectory();
            Logger.Init(fileName, directory);

            var fullPath = System.IO.Path.Combine(directory, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Append, FileAccess.Write))
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    try
                    {
                        writer.WriteLine(message);
                        System.Diagnostics.Debug.WriteLine(message);
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
        private static void Init(string fileName
                               , string directory)
        {
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
                                     , Logger.FormatMessage("create log file\r\n"));
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
        private string _path;


        /// <summary>
        /// Constructor
        /// コンストラクタ
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="directory"></param>
        /// <remarks></remarks>
        public Logger(string fileName = null, string directory = null)
        {
            fileName = string.IsNullOrEmpty(fileName)
                            ? $"xblogger_{DateTime.Now:yyyyMMdd}.log"
                            : fileName;
            directory = directory ?? System.IO.Directory.GetCurrentDirectory();

            Logger.Init(fileName, directory);


            this._path = System.IO.Path.Combine(directory, fileName);

            //open file on append-mode
            this._stream = new System.IO.FileStream(this._path
                                                  , FileMode.Append
                                                  , FileAccess.Write);
            this._writer = new System.IO.StreamWriter(this._stream
                                                    , System.Text.Encoding.UTF8);
        }


        /// <summary>
        /// output message to log file on instance
        /// </summary>
        /// <param name="message"></param>
        /// <remarks></remarks>
        private void InstanceOutput(string message)
        {
            try
            {
                this._writer.WriteLine(message);
                System.Diagnostics.Debug.WriteLine(message);
            }
            catch (Exception ex)
            {
                Xb.Util.Out($"Xb.App.Logger.InstanceOutput: write failure [{message}]");
                Xb.Util.Out(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Write log
        /// ログを書き出す。
        /// </summary>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public void Write(string message)
        {
            this.InstanceOutput(Logger.FormatMessage(message));
        }

        /// <summary>
        /// Write formatted-log
        /// 値を差し込んだ文字列をログに書き出す。
        /// </summary>
        /// <param name="format"></param>
        /// <param name="values"></param>
        public void Write(string format, params object[] values)
        {
            this.InstanceOutput(Logger.FormatMessage(string.Format(format, values)));
        }

        /// <summary>
        /// Write Exception-info log on static
        /// 例外情報をログに書き出す。
        /// </summary>
        /// <param name="ex"></param>
        public void Write(Exception ex)
        {
            this.InstanceOutput(Logger.GetHighlightedText(Xb.Util.GetErrorString(ex)));
        }

        //連続して書き込むとき排他できない
        ///// <summary>
        ///// Write message on async
        ///// ログを非同期で書き出す。
        ///// </summary>
        ///// <param name="message"></param>
        ///// <remarks></remarks>
        //public void WriteAsync(string message)
        //{
        //    try
        //    {
        //        var text = Logger.FormatMessage(message);
        //        Task.Run(() =>
        //        {
        //            Debug.WriteLine($"CanWrite?: {this._stream.CanWrite}");
        //            this._writer.WriteLineAsync(Logger.FormatMessage(message));
        //        });
        //        Xb.Util.Out(text);
        //    }
        //    catch (Exception ex)
        //    {
        //        Xb.Util.Out($"Xb.App.Logger.WriteAsync: write failure [{message}]");
        //        Xb.Util.Out(ex);
        //        throw ex;
        //    }
        //}


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

                    this._path = null;
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
