using System;

namespace Xb.App
{
    /// <summary>
    /// インスタンスベースのログ出力クラス
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class Logger : IDisposable
    {
        //オープンしたファイルの番号
        //private int _fileNo;
        private System.IO.StreamWriter _writer;

        //ログファイルのフルパス
        private readonly string _path;

        /// <summary>
        /// ログファイルのフルパス
        /// </summary>
        public string Path { get { return this._path; } }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="directory"></param>
        /// <remarks></remarks>
        public Logger(string fileName = null, string directory = null)
        {
            //ログファイル名が未指定のとき、日時で名称を生成する。
            if (fileName == null)
            {
                fileName = DateTime.Now.ToString("yyyyMMdd") + "_log.txt";
            }

            //ログファイル出力先が未指定のとき、共通ライブラリ側の設定ファイルからパスを取得する。
            if (directory == null)
            {
                //TODO: Xamarin.iOS/Androidで取得できる実行ファイルパスにする。
                directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }

            this._path = System.IO.Path.Combine(directory, fileName);

            //ログファイル出力先が存在しないとき、フォルダ生成を試みる。
            if (!System.IO.Directory.Exists(directory))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                catch (Exception)
                {
                    Xb.Util.Out("Xb.App.Logger.Constructor: 指定されたログ出力先パスを生成出来ませんでした：" + directory);
                    throw new Exception("b.App.Logger.Constructor: 指定されたログ出力先パスを生成出来ませんでした：" + directory);
                }
            }

            //ログファイルが存在しないとき、新規作成する。
            if (!System.IO.File.Exists(this._path))
            {
                var writer = new System.IO.StreamWriter(this._path, false, System.Text.Encoding.UTF8);
                writer.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " : ログファイル作成");
                writer.Close();
            }

            //ログファイルを追記モードで開く
            this._writer = new System.IO.StreamWriter(this._path, true, System.Text.Encoding.UTF8);
        }


        /// <summary>
        /// ログを書き出す。
        /// </summary>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public void Write(string message)
        {
            try
            {
                var text = string.Format("{0} : {1}",
                                         DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                                         message);

                this._writer.WriteLine(text);
                Xb.Util.Out(text);
            }
            catch (Exception)
            {
                Xb.Util.Out("Xb.App.Logger.Log: ログ出力が出来ませんでした：" + message);
                throw new Exception("Xb.App.Logger.Log: ログ出力が出来ませんでした：" + message);
            }
        }

        /// <summary>
        /// ログファイルを閉じる。
        /// </summary>
        /// <remarks></remarks>
        public void Close()
        {
            try
            {
                this._writer.Close();
            }
            catch (Exception)
            {
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
                    this.Close();
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
