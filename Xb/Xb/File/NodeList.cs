using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Xb.File
{

    /// <summary>
    /// ノード列挙クラス
    /// </summary>
    /// <remarks></remarks>
    public class NodeList
    {

        private readonly List<Node> _nodes;

        private readonly Node _rootNode;

        /// <summary>
        /// 保持するノードのリスト。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// 保持する全ノードがフラットにセットされている。
        /// </remarks>
        public List<Node> Nodes
        {
            get { return _nodes; }
        }


        /// <summary>
        /// 起点となるノード
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// 保持するノードをツリー状に表現するための、Nodeオブジェクトの起点。
        /// </remarks>
        public Node RootNode
        {
            get { return _rootNode; }
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="rootPath"></param>
        /// <remarks>
        /// 渡し値パスからファイルシステム上のファイル／ディレクトリを取得して生成するコンストラクタ。
        /// </remarks>

        public NodeList(string rootPath)
        {
            Node tmpNode = new Node(rootPath);

            if (tmpNode.Type == File.Node.NodeType.Directory)
            {
                _rootNode = tmpNode;
            }
            else
            {
                _rootNode = new Node(tmpNode.BaseDirectory);
            }

            _nodes = new List<Node>();
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="updateDate"></param>
        /// <param name="isDirectory"></param>
        /// <remarks>
        /// FTPパス等、パス文字列のみではシステム上からファイルシステム上のパスと認識できないときに
        /// 各種プロパティを手動で指定して生成するコンストラクタ。
        /// </remarks>

        public NodeList(string rootPath, DateTime updateDate, bool isDirectory)
        {
            Node tmpNode = new Node(rootPath, updateDate, isDirectory);

            if (tmpNode.Type == File.Node.NodeType.Directory)
            {
                _rootNode = tmpNode;
            }
            else
            {
                _rootNode = new Node(tmpNode.BaseDirectory);
            }

            _nodes = new List<Node>();
        }


        /// <summary>
        /// ノードを追加する。
        /// </summary>
        /// <param name="nodeValue"></param>
        /// <remarks></remarks>

        public void Add(Node nodeValue)
        {
            _nodes.Add(null);
            _nodes[_nodes.Count - 1] = nodeValue;
        }


        /// <summary>
        /// ノードをルートノード直下の子ノードとして追加する。
        /// </summary>
        /// <param name="nodeValue"></param>
        /// <remarks></remarks>

        public void AddAsRootChild(Node nodeValue)
        {
            _nodes.Add(null);
            _nodes[_nodes.Count - 1] = nodeValue;
            _rootNode.AddChild(nodeValue);
        }


        /// <summary>
        /// ツリー構造と無関係に、配下の全ノードのリストを返す。
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<string> GetAllPaths()
        {

            List<string> result = new List<string>();
            foreach (Node n in _nodes)
            {
                result.Add(n.Path);
            }

            return result;
        }


        /// <summary>
        /// ツリー構造と無関係に、配下の全ファイルノードを返す。
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<string> GetFilePaths()
        {
            List<string> result = new List<string>();
            foreach (Node n in _nodes)
            {
                if (n.Type != File.Node.NodeType.File)
                    continue;

                result.Add(n.Path);
            }

            return result;
        }


        /// <summary>
        /// ツリー構造と無関係に、配下の全ディレクトリノードを返す。
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<string> GetDirectoryPaths()
        {
            List<string> result = new List<string>();
            foreach (Node n in _nodes)
            {
                if (n.Type != File.Node.NodeType.Directory)
                    continue;
                result.Add(n.Path);
            }

            return result;
        }


        /// <summary>
        /// ルートノード配下の全ノード中から、パス文字列が渡し値に合致した最初のノードを返す。
        /// </summary>
        /// <param name="matchString"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public Node Find(string matchString)
        {
            matchString = matchString.ToLower();
            foreach (Node n in _nodes)
            {
                if (n.Path.ToLower().IndexOf(matchString) != -1)
                    return n;
            }

            return null;
        }


        /// <summary>
        /// ルートノード配下の全ノード中から、パス文字列が渡し値に合致した全てのノードを返す。
        /// </summary>
        /// <param name="matchString"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<Node> FindAll(string matchString)
        {
            matchString = matchString.ToLower();
            List<Node> result = new List<Node>();
            foreach (Node n in _nodes)
            {
                if (n.Path.ToLower().IndexOf(matchString) != -1)
                    result.Add(n);
            }

            return result;
        }


        /// <summary>
        /// 配下ノードのパス文字列を配列で返す。
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public string[] ToArray()
        {
            List<string> result = new List<string>();
            for (int i = 0; i <= this._nodes.Count - 1; i++)
            {
                result.Add(this._nodes[i].Path);
            }

            return result.ToArray();
        }


        /// <summary>
        /// 接続先のファイル／ディレクトリ構造を一階層分のみ取得する。
        /// </summary>
        /// <param name="directory">走査の起点となるディレクトリパス。</param>
        /// <param name="extensions">ファイル名の拡張子候補(カンマ区切り文字列)</param>
        /// <returns>パターンに合致したファイルと、全てのディレクトリのパスリスト</returns>
        /// <remarks>
        /// C# と VB.NET の入門サイト より、下記ページ上の処理を土台にしている。
        /// http://jeanne.wankuma.com/tips/vb.net/directory/getfilesmostdeep.html
        /// </remarks>
        public static Xb.File.NodeList GetNodeList(string directory, string extensions = "")
        {
            if (!System.IO.Directory.Exists(directory))
                return null;

            if (extensions == null)
                extensions = "";

            var result = new Xb.File.NodeList(directory);
            Regex regex = null;
            var filter = extensions.Split(Convert.ToChar(","));
            var filterString = "";

            //ファイル絞り込み用に、拡張子検出正規表現オブジェクトを生成する。
            if (!string.IsNullOrEmpty(extensions))
            {
                for (var i = 0; i <= filter.Length - 1; i++)
                {
                    filterString += filter[i].ToLower() + "|";
                    filterString += filter[i].ToUpper() + "|";
                }
                filterString = filterString.Substring(0, filterString.Length - 1);
            }
            regex = new Regex(".+\\.(" + filterString + ")$", RegexOptions.IgnoreCase);

            //このディレクトリ内のすべてのファイルを走査する
            foreach (string path in System.IO.Directory.GetFiles(directory))
            {
                //指定拡張子が無いか指定に合致したとき、ファイルを取得する。
                if (string.IsNullOrEmpty(extensions) | regex.Match(path).Success)
                {
                    result.AddAsRootChild(new Xb.File.Node(path));
                }
            }

            //このディレクトリ内のすべてのディレクトリを取得する。
            foreach (string path in System.IO.Directory.GetDirectories(directory))
            {
                result.AddAsRootChild(new Xb.File.Node(path));
            }

            return result;
        }


        /// <summary>
        /// 接続先の指定ディレクトリ以下全ファイル／ディレクトリ構造を取得する。
        /// </summary>
        /// <param name="directory">走査の起点となるディレクトリパス。</param>
        /// <param name="extensions">ファイル名の拡張子候補(カンマ区切り文字列)</param>
        /// <returns>パターンに合致したファイルと、全てのディレクトリのパスリスト</returns>
        /// <remarks></remarks>
        public static Xb.File.NodeList GetNodeListRecursive(string directory, string extensions = "")
        {
            var result = default(Xb.File.NodeList);
            var backup = new Dictionary<string, Xb.File.Node>();

            result = GetNodeList(directory, extensions);

            foreach (string key in result.RootNode.Children.Keys)
            {
                if (result.RootNode.Children[key].Type == Xb.File.Node.NodeType.Directory)
                {
                    //要素がディレクトリのとき、そのディレクトリ以下の要素を再帰取得する。
                    var tmp = Xb.File.NodeList.GetNodeListRecursive(result.RootNode.Children[key].Path, 
                                                                    extensions);

                    //参照中に要素上書きができないため、Nodeオブジェクトの書き換え候補をリストに保持しておく。
                    backup.Add(key, tmp.RootNode);

                    //戻り値に、直下でない要素を追加する。
                    foreach (Xb.File.Node childNode in tmp.Nodes)
                    {
                        result.Add(childNode);
                    }
                }
            }
            //ディレクトリ要素を、再帰処理完了後の要素と差し替える。
            foreach (string key in backup.Keys)
            {
                result.RootNode.Children[key] = backup[key];
            }

            GC.Collect();

            return result;
        }
    }
}
