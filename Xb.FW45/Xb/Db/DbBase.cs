using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Xb.Db
{
    /// <summary>
    /// Database Connection Manager Base Class
    /// データベース接続規定クラス
    /// </summary>
    public partial class DbBase : IDisposable
    {
        /// <summary>
        /// transaction begin command
        /// トランザクション開始SQLコマンド
        /// </summary>
        protected string TranCmdBegin = "BEGIN";

        /// <summary>
        /// transaction commit command
        /// トランザクション確定SQLコマンド
        /// </summary>
        protected string TranCmdCommit = "COMMIT";

        /// <summary>
        /// transanction rollback command
        /// トランザクションロールバックSQLコマンド
        /// </summary>
        protected string TranCmdRollback = "ROLLBACK";

        /// <summary>
        /// 1 record selection query template
        /// レコード存在検証SQLテンプレート
        /// </summary>
        protected string SqlFind = "SELECT * FROM {0} WHERE {1} LIMIT 1 ";


        /// <summary>
        /// Wild-Card Position type
        /// Like検索時のワイルドカード位置
        /// </summary>
        /// <remarks></remarks>
        public enum LikeMarkPosition
        {
            /// <summary>
            /// Front
            /// 前にワイルドカード(後方一致)
            /// </summary>
            Before,

            /// <summary>
            /// After
            /// 後にワイルドカード(前方一致)
            /// </summary>
            After,

            /// <summary>
            /// Both
            /// 前後にワイルドカード(部分一致)
            /// </summary>
            Both,

            /// <summary>
            /// None
            /// ワイルドカードなし(完全一致)
            /// </summary>
            None
        }

        /// <summary>
        /// Text Encoding
        /// テキストのエンコーディング型
        /// </summary>
        /// <remarks></remarks>
        public enum EncodeType
        {
            /// <summary>
            /// Shift-JIS
            /// </summary>
            Sjis,

            /// <summary>
            /// UTF-8
            /// </summary>
            Utf8,

            /// <summary>
            /// EUC-JP
            /// </summary>
            Eucjp
        }

        /// <summary>
        /// Connection
        /// DBコネクション
        /// </summary>
        protected System.Data.Common.DbConnection Connection;

        /// <summary>
        /// Command
        /// DBコマンド
        /// </summary>
        protected System.Data.Common.DbCommand Command;

        /// <summary>
        /// Adapter
        /// DBアダプタ
        /// </summary>
        protected System.Data.Common.DbDataAdapter Adapter;
        
        /// <summary>
        /// Hostname (or IpAddress)
        /// 接続先アドレス(orサーバホスト名)
        /// </summary>
        protected string _address;

        /// <summary>
        /// Schema name
        /// 接続DBスキーマ名
        /// </summary>
        protected string _name;

        /// <summary>
        /// User name
        /// 接続ユーザー名
        /// </summary>
        protected string _user;

        /// <summary>
        /// Password
        /// 接続パスワード
        /// </summary>
        protected string _password;

        /// <summary>
        /// Additional connection string
        /// 接続時の補助設定記述用文字列
        /// </summary>
        protected string _additionalConnectionString;

        /// <summary>
        /// Table name
        /// 接続スキーマ配下のテーブル名リスト
        /// </summary>
        protected List<string> _tableNames;

        /// <summary>
        /// Table-Structure Datatable
        /// 接続スキーマ配下のテーブル構造クエリ結果を保持するDataTable
        /// </summary>
        protected DataTable _structureTable;

        /// <summary>
        /// Models of Tables
        /// 接続スキーマ配下テーブルごとのXb.Db.Modelオブジェクト配列
        /// </summary>
        protected Dictionary<string, Db.Model> _models;

        /// <summary>
        /// Encode type
        /// 文字列エンコード
        /// </summary>
        protected Db.DbBase.EncodeType _encodeType = EncodeType.Utf8;

        /// <summary>
        /// Encode
        /// 文字列処理時のエンコードオブジェクト
        /// </summary>
        protected System.Text.Encoding _encoding = System.Text.Encoding.UTF8;

        /// <summary>
        /// Transaction-Flag
        /// 現在トランザクション処理中か否か
        /// </summary>
        protected bool _isInTransaction;



        /// <summary>
        /// Hostname(or IpAddress)
        /// 接続先アドレス(orサーバホスト名)
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Address => this._address;

        /// <summary>
        /// Schema name
        /// 接続DBスキーマ名
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Name => this._name;

        /// <summary>
        /// User name
        /// 接続ユーザー名
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string User => this._user;

        /// <summary>
        /// Password
        /// 接続パスワード
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Password => this._password;

        /// <summary>
        /// Table names list
        /// 接続スキーマ配下のテーブル名リスト
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<string> TableNames => this._tableNames;

        /// <summary>
        /// Table-Scructure Datatable
        /// テーブル情報DataTable
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataTable StructureTable => this._structureTable;

        /// <summary>
        /// Models of Tables
        /// モデルリスト
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public Dictionary<string, Xb.Db.Model> Models => this._models;

        /// <summary>
        /// Encode type
        /// 文字列エンコード種
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public Db.DbBase.EncodeType EncodingType
        {
            get { return this._encodeType; }
            set
            {
                switch (value)
                {
                    case EncodeType.Utf8:
                        this._encoding = System.Text.Encoding.UTF8;
                        break;
                    case EncodeType.Sjis:
                        this._encoding = System.Text.Encoding.GetEncoding("Shift_JIS");
                        break;
                    case EncodeType.Eucjp:
                        this._encoding = System.Text.Encoding.GetEncoding("EUC-JP");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }

                this._encodeType = value;
            }
        }

        /// <summary>
        /// Encode
        /// 文字列処理時のエンコードオブジェクト
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public System.Text.Encoding Encoding => this._encoding;

        /// <summary>
        /// Transaction flag
        /// このコネクションが、現在トランザクション中か否かを返す。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsInTransaction => this._isInTransaction;

        /// <summary>
        /// Connection flag
        /// 接続状態
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual bool IsConnected => false;


        /// <summary>
        /// Constructor(dummy)
        /// コンストラクタ(ダミー)
        /// </summary>
        public DbBase()
        {
            throw new InvalidOperationException("Execute only subclass");
        }


        /// <summary>
        /// Constructor
        /// コンストラクタ
        /// </summary>
        /// <param name="name"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="address"></param>
        /// <param name="encodingType"></param>
        /// <param name="additionalString"></param>
        protected DbBase(string name, 
                         string user = "", 
                         string password = "", 
                         string address = "", 
                         Xb.Db.DbBase.EncodeType encodingType = EncodeType.Utf8, 
                         string additionalString = "")
        {
            this._address = address;
            this._name = name;
            this._user = user;
            this._password = password;
            this._additionalConnectionString = additionalString;
            this.EncodingType = encodingType;

            //Connect
            this.Open();
        }
        

        /// <summary>
        /// Connect DB
        /// DBへ接続する
        /// </summary>>
        /// <remarks></remarks>
        protected virtual void Open()
        {
            Xb.Util.Out("Xb.Db.Open: Execute only subclass");
            throw new ApplicationException("Execute only subclass");
        }


        /// <summary>
        /// Get Table-Structures
        /// 接続先DBの構造を取得する。
        /// </summary>
        /// <remarks></remarks>
        protected virtual void GetStructure()
        {
            Xb.Util.Out("Xb.Db.GetStructure: Execute only subclass");
            throw new ApplicationException("Execute only subclass");
        }


        /// <summary>
        /// Build Models of Tables
        /// テーブルごとのモデルオブジェクトを生成、保持させる。
        /// </summary>
        /// <remarks></remarks>
        protected void BuildModels()
        {
            if (this._tableNames == null || this._structureTable == null)
            {
                Xb.Util.Out("Xb.Db.BuildModels: Table-Structure not found");
                throw new ApplicationException("Table-Structure not found");
            }

            var view = new DataView(this._structureTable);
            this._models = new Dictionary<string, Model>();

            foreach (string name in this._tableNames)
            {
                view.RowFilter = string.Format("TABLE_NAME = '{0}'", name);
                this._models.Add(name.ToUpper(), new Db.Model(this, view.ToTable()));
            }
        }


        /// <summary>
        /// Get Model of Table
        /// 渡し値テーブル名のモデルインスタンスを取得する。
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public Xb.Db.Model GetModel(string tableName)
        {
            tableName = tableName.ToUpper();

            if (!this._models.ContainsKey(tableName))
            {
                Xb.Util.Out("Xb.Db.GetModel: Table not found");
                throw new ArgumentException("Table not found");
            }

            return this._models[tableName];
        }


        /// <summary>
        /// Get Table-Structure DataTable
        /// 渡し値テーブルに絞り込んだカラム情報DataTableを取得する。
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual DataTable GetTableInfo(string tableName)
        {
            var view = new DataView(this._structureTable);

            view.RowFilter = string.Format("TABLE_NAME='{0}'", tableName);

            return view.ToTable();
        }


        /// <summary>
        /// Get 1 row matched
        /// 条件に合致した最初の行を返す
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="whereString"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual DataRow Find(string tableName, string whereString)
        {
            if (whereString == null)
                whereString = "";

            var sql = string.Format(this.SqlFind, tableName, whereString);
            var dt = this.Query(sql);

            //No-Data -> Nothing
            if (dt == null || dt.Rows.Count <= 0)
                return null;

            //return first row
            return dt.Rows[0];
        }


        /// <summary>
        /// Get all rows matched
        /// 条件に合致した全行データを返す。
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="whereString"></param>
        /// <param name="orderString"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual DataTable FindAll(string tableName, 
                                         string whereString = null, 
                                         string orderString = null)
        {
            whereString = whereString ?? "";
            orderString = orderString ?? "";

            var sql = $" SELECT * FROM {tableName} ";

            if (!string.IsNullOrEmpty(whereString))
                sql += $" WHERE {whereString}";

            if (!string.IsNullOrEmpty(orderString))
                sql += $" ORDER BY {orderString}";

            return this.Query(sql);
        }


        /// <summary>
        /// Disconnect Database
        /// DBへの接続を解除する
        /// </summary>
        /// <remarks></remarks>
        public virtual void Close()
        {
            this.Dispose();
        }


        /// <summary>
        /// Get Quoted-String
        /// 文字列項目のクォートラップ処理
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <remarks>
        /// for MsSqlServer, SqLite. override on Mysql
        /// 標準をSqlServer/SQLite風クォートにセット。MySQLではOverrideする。
        /// </remarks>
        public virtual string Quote(string text)
        {
            return Xb.Str.SqlQuote(text);
        }

        /// <summary>
        /// Get Quoted-String
        /// 文字列項目のクォートラップ処理
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <remarks>
        /// for MsSqlServer, SqLite. override on Mysql
        /// 標準をSqlServer/SQLite風クォートにセット。MySQLではOverrideする。
        /// </remarks>
        public virtual string Quote(string text, LikeMarkPosition likeMarkPos)
        {
            switch (likeMarkPos)
            {
                case LikeMarkPosition.Before:
                    text = "%" + text;
                    break;
                case LikeMarkPosition.After:
                    text += "%";
                    break;
                case LikeMarkPosition.Both:
                    text = "%" + text + "%";
                    break;
                case LikeMarkPosition.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(likeMarkPos), likeMarkPos, null);
            }
            return Xb.Str.SqlQuote(text);
        }

        /// <summary>
        /// Execute Non-Select query, Get effected row count
        /// SQL文でSELECTでないコマンドを実行する
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual int Execute(string sql)
        {
            //check connection
            if (!this.IsConnected)
                throw new ApplicationException("Connection not ready");

            try
            {
                //実行
                this.Command.CommandText = sql;
                int result = this.Command.ExecuteNonQuery();
                this.Command.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                Xb.Util.Out(ex);
                throw new ApplicationException("Xb.Db.Execute fail \r\n" + ex.Message + "\r\n" + sql);
            }
        }


        /// <summary>
        /// Execute Select query, Get DataTable
        /// SQL文でクエリを実行し、結果を返す
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual DataTable Query(string sql)
        {
            //check connection
            if (!this.IsConnected)
                throw new ApplicationException("Connection not ready");

            try
            {
                var dt = new DataTable();
                this.Adapter.Fill(dt);
                this.Adapter.Dispose();
                return dt;
            }
            catch (Exception ex)
            {
                Xb.Util.Out(ex);
                throw new ApplicationException("Xb.Db.Query fail \r\n" + ex.Message + "\r\n" + sql);
            }
        }

        /// <summary>
        /// Execute Select query, Attach to Passing-DataTable
        /// SQL文でクエリを実行し、渡し値DataTableに結果をセットする。
        /// </summary>
        /// <param name="sql"></param>
        public virtual void Fill(string sql, DataTable dt)
        {
            //check connection
            if (!this.IsConnected)
                return;

            try
            {
                dt = dt ?? new DataTable();

                this.Adapter.Fill(dt);
                this.Adapter.Dispose();
                return;
            }
            catch (Exception ex)
            {
                Xb.Util.Out(ex);
                throw new ApplicationException("Xb.Db.Fill fail \r\n" + ex.Message + "\r\n" + sql);
            }
        }


        /// <summary>
        /// Check exist record
        /// データの存在チェック
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual bool Exist(string sql)
        {
            var dt = this.Query(sql);
            return (dt != null && dt.Rows.Count != 0);
        }


        /// <summary>
        /// Start transaction
        /// トランザクションを開始する
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual bool BeginTransaction()
        {
            try
            {
                //check connection
                if (!this.IsConnected)
                    return false;

                //now on transaction, return true. Do NOT nesting.
                //トランザクションの入れ子を避ける。
                if (this._isInTransaction)
                    return true;

                //begin transaction
                this.Execute(this.TranCmdBegin);

                this._isInTransaction = true;
                return true;
            }
            catch (Exception ex)
            {
                Xb.Util.Out(ex);
                return false;
            }
        }


        /// <summary>
        /// Commit transaction
        /// トランザクションを確定する
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual bool CommitTransaction()
        {
            try
            {
                //check connection
                if (!this.IsConnected)
                    return false;

                //Commit transaction
                this.Execute(this.TranCmdCommit);

                this._isInTransaction = false;
                return true;
            }
            catch (Exception ex)
            {
                Xb.Util.Out(ex);
                return false;
            }
        }


        /// <summary>
        /// Rollback transaction
        /// トランザクションを戻す
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual bool RollbackTransaction()
        {
            try
            {
                //check connection
                if (!this.IsConnected)
                    return false;

                //Rollback transaction
                this.Execute(this.TranCmdRollback);

                this._isInTransaction = false;
                return true;
            }
            catch (Exception ex)
            {
                Xb.Util.Out(ex);
                return false;
            }
        }


        /// <summary>
        /// Get Database backup file
        /// データベースのバックアップファイルを生成する。
        /// </summary>
        /// <param name="fileName"></param>
        /// <remarks></remarks>
        public virtual bool BackupDb(string fileName)
        {
            Xb.Util.Out("Xb.Db.DbBase.BackupDb: Execute only subclass");
            throw new ApplicationException("Execute only subclass");
        }


        /// <summary>
        /// Remove file if exist
        /// 既存のファイルがあったとき、削除する。
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        protected bool RemoveIfExints(string fileName)
        {
            //渡し値パスが実在することを確認する。
            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(fileName)))
            {
                Xb.Util.Out("Xb.Db.DbBase.RemoveIfExints: File-Path not found");
                throw new ArgumentException("File-Path not found");
            }

            if (!System.IO.File.Exists(fileName))
                return true;

            try
            {
                System.IO.File.Delete(fileName);
            }
            catch (Exception ex)
            {
                Xb.Util.Out("Xb.Db.DbBase.RemoveIfExints: Cannot Delete files");
                Xb.Util.Out(ex);
                return false;
            }

            return true;
        }


        private bool _disposedValue = false;

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    if (this._models != null)
                    {
                        foreach (KeyValuePair<string, Model> pair in this._models)
                        {
                            if(pair.Value != null)
                                pair.Value.Dispose();
                        }
                    }


                    //now on transaction, Do rollback
                    if (this._isInTransaction)
                        this.RollbackTransaction();

                    //disconnect
                    try
                    { this.Connection.Close(); }
                    catch (Exception){}
                    
                    this.Connection = null;
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
