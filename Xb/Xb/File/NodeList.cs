using System;
using System.Collections.Generic;

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
    }
}
