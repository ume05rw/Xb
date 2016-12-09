using System;
using System.Collections.Generic;



namespace Xb.File
{ 
    /// <summary>
    /// ファイルノードクラス
    /// </summary>
    /// <remarks>
    /// 単一のファイル／ディレクトリを示すクラス
    /// </remarks>
    public class Node
    {

        /// <summary>
        /// ファイルシステムのノード区分
        /// </summary>
        /// <remarks></remarks>
        public enum NodeType
        {

            /// <summary>
            /// ファイル
            /// </summary>
            /// <remarks></remarks>
            File,

            /// <summary>
            /// ディレクトリ
            /// </summary>
            /// <remarks></remarks>
            Directory
        }


        /// <summary>
        /// パスの区切り文字区分
        /// </summary>
        /// <remarks></remarks>
        public enum Separator
        {
            /// <summary>
            /// バックスラッシュ - Windowsスタイル
            /// </summary>
            /// <remarks></remarks>
            Backslash,

            /// <summary>
            /// スラッシュ - Unixスタイル
            /// </summary>
            /// <remarks></remarks>
            Slash
        }


        /// <summary>
        /// 日時基準抽出の処理区分
        /// </summary>
        /// <remarks></remarks>
        public enum DateLimitType
        {

            /// <summary>
            /// 基準日時より新しいものを抽出する。
            /// </summary>
            /// <remarks></remarks>
            Newer,

            /// <summary>
            /// 基準日時を含む、古いものを抽出する。
            /// </summary>
            /// <remarks></remarks>
            Older
        }


        private readonly NodeType _type;
        private readonly string _baseDirectory;
        private readonly string _name;
        private readonly Node _parent;
        private readonly Dictionary<string, Node> _children;
        private readonly Separator _separator;

        private DateTime _updatedate;

        /// <summary>
        /// ノートタイプ ファイル／ディレクトリ区分
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public NodeType Type
        {
            get { return this._type; }
        }


        /// <summary>
        /// ノードの親ディレクトリ
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string BaseDirectory
        {
            get { return this._baseDirectory; }
        }


        /// <summary>
        /// ノードの名前 フルパスでなく単体の名称
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public string Name
        {
            get { return this._name; }
        }


        /// <summary>
        /// ノードのフルパス
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Path
        {
            get
            {
                return System.IO.Path.Combine(this._baseDirectory, this._name);
            }
        }


        /// <summary>
        /// 自身が所属している親ノードを返す。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public Node Parent
        {
            get { return this._parent; }
        }


        /// <summary>
        /// 直下の子ノードの連想配列
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public Dictionary<string, Node> Children
        {
            get { return this._children; }
        }


        /// <summary>
        /// パスのデリミタ文字
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public char SeparatorChar
        {
            get
            {
                return (this._separator == File.Node.Separator.Slash)
                        ? '/' 
                        : '\\';
            }
        }


        /// <summary>
        /// 更新日時
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public DateTime UpdateDate
        {
            get { return this._updatedate; }
        }


        /// <summary>
        /// コンストラクタ - 自動判定するとき
        /// </summary>
        /// <param name="path"></param>
        /// <remarks>
        /// 渡し値文字列から、ファイル or ディレクトリを自動判別するコンストラクタ
        /// </remarks>

        public Node(string path)
        {
            if (path == null)
                path = "";

            System.IO.FileInfo fileInfo = null;
            System.IO.DirectoryInfo directoryInfo = null;

            //パス区切り文字を検出する。
            this._separator = (Separator)(path.LastIndexOf("\\") != -1 ? Separator.Backslash : Separator.Slash);

            if (System.IO.File.Exists(path))
            {
                fileInfo = new System.IO.FileInfo(path);
                this._type = NodeType.File;
                this._name = System.IO.Path.GetFileName(path);
                this._baseDirectory = System.IO.Path.GetDirectoryName(path);
                this._updatedate = fileInfo.LastWriteTime;
            }
            else if (System.IO.Directory.Exists(path))
            {
                directoryInfo = new System.IO.DirectoryInfo(path);
                this._type = NodeType.Directory;
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(path);
                this._name = dir.Name;
                this._baseDirectory = dir.Parent == null
                                        ? ""
                                        : dir.Parent.FullName.TrimEnd(this._separator == Separator.Backslash 
                                                                        ? Convert.ToChar("\\") 
                                                                        : '/');
                this._updatedate = directoryInfo.LastWriteTime;
            }
            else
            {
                Xb.Util.Out("File.Node.New: 渡し値パスの存在が確認できません。");
                throw new ArgumentException("渡し値パスの存在が確認できません。");
            }

            this._children = new Dictionary<string, Node>();
        }


        /// <summary>
        /// コンストラクタ - 任意の値を定義するとき
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isDirectory"></param>
        /// <remarks>
        /// ファイル or ディレクトリ の区分を指定するコンストラクタ
        /// </remarks>

        public Node(string path, DateTime updateDate, bool isDirectory)
        {
            if (path == null)
                path = "";

            int tmpIdx = 0;

            //パス区切り文字を検出する。
            this._separator = (Separator)(path.LastIndexOf("\\") != -1 ? Separator.Backslash : Separator.Slash);

            this._type = (NodeType)(isDirectory ? NodeType.Directory : NodeType.File);
            this._updatedate = updateDate;

            if (!string.IsNullOrEmpty(path))
            {
                tmpIdx = path.LastIndexOf(this.SeparatorChar);
                this._name = path.Substring(tmpIdx + 1);

                if (tmpIdx != -1)
                {
                    this._baseDirectory = path.Substring(0, tmpIdx + 1);
                }
                else
                {
                    this._baseDirectory = "";
                }
            }
            else
            {
                this._name = "";
                this._baseDirectory = "";
            }

            this._children = new Dictionary<string, Node>();

            //App.Out("new-Node path: " & path & "  / BaseDir: " & Me._baseDirectory & "  / Name: " & Me._name)
        }


        /// <summary>
        /// 子ノードを追加する。
        /// </summary>
        /// <param name="child"></param>
        /// <remarks></remarks>

        public void AddChild(Node child)
        {
            //_children.Add(child.Name, child) '参照セットする。Dictionary.AddはByval定義のようなので。
            _children.Add(child.Name, null);
            _children[child.Name] = child;
        }


        /// <summary>
        /// 配下の全ノードを取得する。
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<File.Node> GetAllChildren()
        {
            List<File.Node> result = new List<File.Node>();
            foreach (Node child in this.Children.Values)
            {
                result.Add(child);

                if ((child.Type == File.Node.NodeType.Directory))
                {
                    result.AddRange(child.GetAllChildren());
                }
            }

            return result;
        }


        /// <summary>
        /// ノードツリー配下の全ノードから、パス文字列が渡し値に合致した最初のノードを返す。
        /// </summary>
        /// <param name="matchString"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public Node Find(string matchString)
        {
            File.Node tmpNode = null;
            //先に直下ノードを走査する。
            foreach (Node child in this.Children.Values)
            {
                if (child.Path.IndexOf(matchString) != -1)
                    return child;
            }
            //直下ノードで合致が無いとき、子ノードの子を再帰処理する。
            foreach (Node child in this.Children.Values)
            {
                tmpNode = child.Find(matchString);
                if (tmpNode != null)
                    return tmpNode;
            }

            //一件も合致が無いとき、Nothingを返す。
            return null;
        }


        /// <summary>
        /// 配下の全ノードのうち、渡し値文字列に合致したものを取得する。
        /// </summary>
        /// <param name="matchString"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<File.Node> FindAll(string matchString)
        {
            List<File.Node> result = new List<File.Node>();
            foreach (Node child in this.Children.Values)
            {
                if (child.Path.IndexOf(matchString) != -1)
                {
                    result.Add(child);
                }

                if (child.Type == File.Node.NodeType.Directory)
                {
                    result.AddRange(child.FindAll(matchString));
                }
            }

            return result;
        }


        /// <summary>
        /// 配下の全ノードのうち、渡し値日時・抽出処理区分に合致したものを取得する。
        /// </summary>
        /// <param name="datetime"></param>
        /// <param name="limitType"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<File.Node> FindAll(DateTime datetime, DateLimitType limitType = DateLimitType.Newer)
        {
            List<File.Node> result = new List<File.Node>();
            if (datetime == null)
                return result;

            foreach (Node child in this.Children.Values)
            {
                if (child.UpdateDate != null)
                {
                    switch (limitType)
                    {
                        case DateLimitType.Newer:
                            if ((child.UpdateDate > datetime))
                                result.Add(child);
                            break;
                        case DateLimitType.Older:
                            if ((child.UpdateDate <= datetime))
                                result.Add(child);
                            break;
                    }
                }

                if (child.Type == File.Node.NodeType.Directory)
                {
                    result.AddRange(child.FindAll(datetime, limitType));
                }
            }

            return result;
        }
    }
}
