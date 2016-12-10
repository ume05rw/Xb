using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;


namespace Xb.Db
{

    /// <summary>
    /// Connection manager class for Microsoft Sql Server
    /// Microsoft Sql Server用DB接続管理クラス
    /// </summary>
    public class MsSql : Db.DbBase
    {
        /// <summary>
        /// transaction begin command
        /// トランザクション開始SQLコマンド
        /// </summary>
        private new string TranCmdBegin = "BEGIN TRANSACTION";

        /// <summary>
        /// transaction commit command
        /// トランザクション確定SQLコマンド
        /// </summary>
        private new string TranCmdCommit = "COMMIT TRANSACTION";

        /// <summary>
        /// transanction rollback command
        /// トランザクションロールバックSQLコマンド
        /// </summary>
        private new string TranCmdRollback = "ROLLBACK TRANSACTION";

        /// <summary>
        /// 1 record selection query template
        /// レコード存在検証SQLテンプレート
        /// </summary>
        private new string SqlFind = "SELECT TOP(1) * FROM {0} WHERE {1} ";

        /// <summary>
        /// Connection
        /// DBコネクション
        /// </summary>
        private new SqlConnection Connection;


        /// <summary>
        /// Connection flag
        /// 接続状態
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public override bool IsConnected
        {
            get
            {
                try
                {
                    if (this.Connection == null)
                        return false;
                    
                    return (this.Connection.State != ConnectionState.Broken 
                            && this.Connection.State != ConnectionState.Closed 
                            && this.Connection.State != ConnectionState.Connecting);
                }
                catch (Exception ex)
                {
                    Xb.Util.Out(ex);
                    return false;
                }
            }
        }


        /// <summary>
        /// Constructor
        /// コンストラクタ
        /// </summary>
        /// <param name="name"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="address"></param>
        /// <param name="isBuildStructureModels"></param>
        /// <param name="encodingType"></param>
        /// <param name="additionalString"></param>
        /// <remarks></remarks>
        public MsSql(string name, 
                     string user = "sa", 
                     string password = "sa", 
                     string address = "localhost", 
                     bool isBuildStructureModels = true,
                     Db.DbBase.EncodeType encodingType = EncodeType.Sjis, 
                     string additionalString = "") 
            : base(name, 
                   user, 
                   password, 
                   address, 
                   encodingType, 
                   additionalString)
        {
            base.TranCmdBegin = this.TranCmdBegin;
            base.TranCmdCommit = this.TranCmdCommit;
            base.TranCmdRollback = this.TranCmdRollback;
            base.SqlFind = this.SqlFind;

            if (isBuildStructureModels)
            {
                //Get Table-Structures
                this.GetStructure();
            }
        }


        /// <summary>
        /// Constructor
        /// コンストラクタ
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="name"></param>
        /// <param name="isBuildStructureModels"></param>
        /// <param name="encodingType"></param>
        /// <remarks></remarks>
        public MsSql(SqlConnection connection, 
                     string name,
                     bool isBuildStructureModels = true,
                     Db.DbBase.EncodeType encodingType = EncodeType.Sjis)
        {
            base.TranCmdBegin = this.TranCmdBegin;
            base.TranCmdCommit = this.TranCmdCommit;
            base.TranCmdRollback = this.TranCmdRollback;
            base.SqlFind = this.SqlFind;

            this._name = name;
            this.Connection = connection;
            this.EncodingType = encodingType;

            //init transaction flag
            this._isInTransaction = false;

            //set connection refference
            base.Connection = this.Connection;

            if (isBuildStructureModels)
            {
                //Get Table-Structures
                this.GetStructure();
            }
        }


        /// <summary>
        /// Connect DB
        /// DBへ接続する
        /// </summary>>
        /// <remarks></remarks>
        protected override void Open()
        {
            //now has connection, exit
            if (this.IsConnected)
                return;

            //build connection string
            string connectionString 
                = string.Format("server={0};user id={1}; password={2}; database={3}; pooling=false{4}", 
                                this._address, 
                                this._user, 
                                this._password, 
                                this._name, 
                                string.IsNullOrEmpty(this._additionalConnectionString) 
                                    ? "" 
                                    : "; " + this._additionalConnectionString);

            try
            {
                //connect DB
                this.Connection = new SqlConnection();
                this.Connection.ConnectionString = connectionString;
                this.Connection.Open();
            }
            catch (Exception)
            {
                this.Connection = null;
            }

            if (!this.IsConnected)
            {
                Xb.Util.Out("Xb.Db.MsSql.Open: Cannot connect DB");
                throw new ApplicationException("Xb.Db.MsSql.Open: Cannot connect DB");
            }

            //init transaction
            this._isInTransaction = false;

            //set connection refference
            base.Connection = this.Connection;
        }


        /// <summary>
        /// Get Table-Structure
        /// 接続先DBの構造を取得する。
        /// </summary>
        /// <remarks></remarks>
        protected override void GetStructure()
        {
            System.Text.StringBuilder sql = null;
            DataTable dt = null;

            //get Table list
            sql = new System.Text.StringBuilder();
            sql.AppendFormat(" SELECT ");
            sql.AppendFormat("     NAME AS TABLE_NAME ");
            sql.AppendFormat(" FROM ");
            sql.AppendFormat("     SYS.OBJECTS ");
            sql.AppendFormat(" WHERE ");
            sql.AppendFormat("     TYPE = 'U' ");
            sql.AppendFormat("     AND name <> 'sysdiagrams' ");
            sql.AppendFormat(" ORDER BY ");
            sql.AppendFormat("     NAME ");
            dt = this.Query(sql.ToString());

            this._tableNames = dt.AsEnumerable().Select(row => row["TABLE_NAME"].ToString()).ToList();


            //Get Column info
            sql = new System.Text.StringBuilder();
            sql.AppendFormat(" SELECT ");
            sql.AppendFormat("      TBL.NAME AS TABLE_NAME ");
            sql.AppendFormat("     ,COL.column_id AS COLUMN_INDEX ");
            sql.AppendFormat("     ,COL.NAME AS COLUMN_NAME ");
            sql.AppendFormat("     ,TYP.NAME AS 'TYPE' ");
            sql.AppendFormat("     ,CASE WHEN COL.PRECISION = 0 THEN COL.MAX_LENGTH ELSE NULL END AS CHAR_LENGTH ");
            sql.AppendFormat("     ,CASE WHEN COL.PRECISION = 0 THEN NULL ELSE COL.PRECISION END AS NUM_PREC ");
            sql.AppendFormat("     ,COL.SCALE AS NUM_SCALE ");
            sql.AppendFormat("     ,CASE ");
            sql.AppendFormat("          WHEN XCL.INDEX_COLUMN_ID IS NOT NULL AND IDX.IS_PRIMARY_KEY = 1 THEN 1 ");
            sql.AppendFormat("          ELSE 0 ");
            sql.AppendFormat("      END AS IS_PRIMARY_KEY ");
            sql.AppendFormat("     ,COL.IS_NULLABLE AS IS_NULLABLE ");
            sql.AppendFormat("     ,CMT.value AS COMMENT ");
            sql.AppendFormat(" FROM ");
            sql.AppendFormat("     SYS.COLUMNS AS COL ");
            sql.AppendFormat(" LEFT JOIN SYS.OBJECTS AS TBL");
            sql.AppendFormat("     ON COL.OBJECT_ID = TBL.OBJECT_ID ");
            sql.AppendFormat(" LEFT JOIN SYS.TYPES AS TYP ");
            sql.AppendFormat("     ON COL.SYSTEM_TYPE_ID = TYP.SYSTEM_TYPE_ID ");
            sql.AppendFormat(" LEFT JOIN SYS.INDEXES AS IDX ");
            sql.AppendFormat("     ON  COL.OBJECT_ID = IDX.OBJECT_ID ");
            sql.AppendFormat("     AND IDX.IS_PRIMARY_KEY = 1 ");
            sql.AppendFormat(" LEFT JOIN SYS.INDEX_COLUMNS AS XCL ");
            sql.AppendFormat("     ON  COL.OBJECT_ID = XCL.OBJECT_ID ");
            sql.AppendFormat("     AND XCL.INDEX_ID = IDX.INDEX_ID ");
            sql.AppendFormat("     AND COL.COLUMN_ID = XCL.COLUMN_ID ");
            sql.AppendFormat(" LEFT JOIN SYS.EXTENDED_PROPERTIES AS CMT ");
            sql.AppendFormat("     ON  CMT.MAJOR_ID = TBL.object_id ");
            sql.AppendFormat("     AND CMT.MINOR_ID = COL.column_id ");
            sql.AppendFormat(" WHERE ");
            sql.AppendFormat("     TBL.TYPE = 'U' ");
            sql.AppendFormat("     AND TYP.NAME != 'SYSNAME' ");
            sql.AppendFormat(" ORDER BY ");
            sql.AppendFormat("     TBL.NAME ASC ");
            sql.AppendFormat("     ,COL.COLUMN_ID ASC ");
            dt = this.Query(sql.ToString());
            this._structureTable = dt;

            //build Models of Tables
            this.BuildModels();
        }


        /// <summary>
        /// Execute Non-Select query, Get effected row count
        /// SQL文でSELECTでないコマンドを実行する
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public override int Execute(string sql)
        {
            this.Command = new SqlCommand(sql, this.Connection);
            return base.Execute(sql);
        }


        /// <summary>
        /// Execute Select query, Get DataTable
        /// SQL文でクエリを実行し、結果を返す
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public override DataTable Query(string sql)
        {
            this.Adapter = new SqlDataAdapter(sql, this.Connection);
            return base.Query(sql);
        }


        /// <summary>
        /// Get Database backup file
        /// データベースのバックアップファイルを取得する。
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public override bool BackupDb(string fileName)
        {

            //check file-path
            if (!base.RemoveIfExints(fileName))
                return false;

            //execute backup
            try
            {
                this.Execute(string.Format("BACKUP DATABASE {0} TO DISK = '{1}'  with INIT, NAME='{2}'", 
                                           this._name, 
                                           fileName, 
                                           this._name));
            }
            catch (Exception ex)
            {
                Xb.Util.Out("Xb.Db.MsSql.BackupDb: backup query failure：" + ex.Message);
                throw new ApplicationException("backup query failure：" + ex.Message);
            }

            return true;
        }
    }
}
