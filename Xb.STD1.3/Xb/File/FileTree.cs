using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xb.File
{
    public partial class FileTree : IDisposable
    {
        /// <summary>
        /// Nodes array
        /// 配下のノード配列
        /// </summary>
        private Dictionary<string, Xb.File.FileTree.Node> _nodes;


        /// <summary>
        /// Root Node
        /// </summary>
        public Xb.File.FileTree.Node RootNode { get; private set; }


        /// <summary>
        /// Node indexer
        /// ノード要素インデクサ
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Xb.File.FileTree.Node this[string path]
        {
            get
            {
                return this._nodes.ContainsKey(path)
                            ? this._nodes[path]
                            : null;
            }
            protected set
            {
                this._nodes[path] = value;
            }
        }


        /// <summary>
        /// Node-Path array(key)
        /// ノードパス配列
        /// </summary>
        public string[] Paths
        {
            get { return this._nodes.Select(pair => pair.Key).ToArray(); }
        }


        /// <summary>
        /// Node array
        /// ノード配列
        /// </summary>
        public Xb.File.FileTree.Node[] Nodes
        {
            get { return this._nodes.Select(pair => pair.Value).ToArray(); }
        }


        /// <summary>
        /// Constructor
        /// コンストラクタ
        /// </summary>
        public FileTree(string rootPath)
        {
            this.RootNode = new Xb.File.FileTree.Node(this, rootPath);

            this._nodes = new Dictionary<string, Node>();
            this._nodes.Add(this.RootNode.FullPath, this.RootNode);
        }


        /// <summary>
        /// Get matched Node-objects by fullpath(key)
        /// パス(キー)が合致したNodeオブジェクトの配列を返す
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        private Xb.File.FileTree.Node[] GetNodes(List<string> paths)
        {
            return this._nodes.Where(pair => paths.Contains(pair.Key))
                              .Select(pair => pair.Value)
                              .ToArray();
        }


        /// <summary>
        /// Get first-node of matched needle
        /// 渡し値文字列が合致した最初の子ノードを返す
        /// </summary>
        /// <param name="needle"></param>
        /// <returns></returns>
        public Xb.File.FileTree.Node Find(string needle)
        {
            return this._nodes
                       .Where(pair => pair.Key.IndexOf(needle
                                                     , StringComparison.Ordinal) >= 0)
                       .Select(pair => pair.Value)
                       .FirstOrDefault();
        }


        /// <summary>
        /// Get all-nodes of matched needle
        /// 渡し値文字列が合致した全ての子ノードを返す
        /// </summary>
        /// <param name="needle"></param>
        /// <returns></returns>
        public Xb.File.FileTree.Node[] FindAll(string needle)
        {
            return this._nodes
                       .Where(pair => pair.Key.IndexOf(needle
                                                     , StringComparison.Ordinal) >= 0)
                       .Select(pair => pair.Value)
                       .ToArray();
        }


        /// <summary>
        /// Tree-Structure Re-Scan recursive(VERY HEAVY!)
        /// ツリー構造をルートノードから再帰的に取得する
        /// </summary>
        /// <returns></returns>
        public async Task ScanRecursiveAsync()
        {
            await this.RootNode.ScanRecursiveAsync();
        }


        /// <summary>
        /// Returns Tree-object with the passing path as the root
        /// 指定パスをルートにした、Treeオブジェクトを返す
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Xb.File.FileTree GetTree(string path)
        {
            var result = new Xb.File.FileTree(path);
            result.RootNode.Scan();
            return result;
        }


        /// <summary>
        /// Returns a Tree object that scans all nodes under the passing path (VERY HEAVY!)
        /// 指定パス配下の全ノードをスキャンしたTreeオブジェクトを返す。重い！
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<Xb.File.FileTree> GetTreeRecursiveAsync(string path)
        {
            var result = new Xb.File.FileTree(path);
            await result.RootNode.ScanRecursiveAsync();
            return result;
        }



        /// <summary>
        /// Delimiter char
        /// パスの区切り文字
        /// </summary>
        private static readonly char[] DelimiterChars = new char[] { '\\', '/' };

        /// <summary>
        /// Format path-string
        /// パス文字列を整形する
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string FormatPath(string path)
        {
            return (path == null)
                        ? ""
                        : path.TrimEnd(Xb.File.FileTree.DelimiterChars);
        }



        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var pair in this._nodes)
                        pair.Value.Dispose();

                    this._nodes = null;

                    this.RootNode = null;

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
