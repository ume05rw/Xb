using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xb.File
{
    public partial class Tree
    {
        /// <summary>
        /// File-system node
        /// ファイルシステム要素クラス
        /// </summary>
        public class Node : IDisposable
        {
            /// <summary>
            /// Node type(file or directory)
            /// </summary>
            public enum NodeType
            {
                File,
                Directory
            }

            /// <summary>
            /// Xb.File.Tree
            /// </summary>
            public Xb.File.Tree Tree { get; private set; }

            /// <summary>
            /// Parent-Node full-path
            /// 親ノードのフルパス
            /// </summary>
            private string _parentPath;

            /// <summary>
            /// Child-Node array of full-path(key)
            /// 子ノードのフルパス配列
            /// </summary>
            private List<string> _childPaths;

            /// <summary>
            /// Parent-node
            /// 親ノード
            /// </summary>
            public Xb.File.Tree.Node Parent => this.Tree[this._parentPath];

            /// <summary>
            /// Child-Nodes array
            /// 子ノード配列
            /// </summary>
            public Xb.File.Tree.Node[] Children => this.Tree.GetNodes(this._childPaths);

            /// <summary>
            /// Node-name (not full-path)
            /// ノード名称
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Extension
            /// 拡張子
            /// </summary>
            public string Extension { get; private set; }

            /// <summary>
            /// Node type(file or directory)
            /// </summary>
            public NodeType Type { get; private set; }

            /// <summary>
            /// Full-Path
            /// </summary>
            public string FullPath { get; private set; }

            /// <summary>
            /// Parent path
            /// </summary>
            public string ParentPath => this._parentPath;

            /// <summary>
            /// Last update-date
            /// 最終更新日時
            /// </summary>
            public DateTime UpdateDate { get; private set; }

            /// <summary>
            /// ルートノードか否か
            /// is root node?
            /// </summary>
            public bool IsRootNode => (this.Parent == null);


            /// <summary>
            /// Constructor
            /// コンストラクタ
            /// </summary>
            /// <remarks>
            /// get infomation from file-system.
            /// </remarks>
            public Node(Xb.File.Tree tree
                      , string path)
            {
                if (tree == null)
                {
                    Xb.Util.Out($"Xb.File.Tree.Node.Constructor: tree null");
                    throw new ArgumentException($"Xb.File.Tree.Node.Constructor: tree null");
                }

                if (string.IsNullOrEmpty(path))
                {
                    Xb.Util.Out($"Xb.File.Tree.Node.Constructor: path null");
                    throw new ArgumentException($"Xb.File.Tree.Node.Constructor: path null");
                }


                if (System.IO.File.Exists(path))
                {
                    this.Type = NodeType.File;
                    this.Name = System.IO.Path.GetFileName(path);
                    this.Extension = System.IO.Path.GetExtension(path);
                    this.UpdateDate = (new System.IO.FileInfo(path)).LastWriteTime;
                    this._parentPath = Xb.File.Tree.FormatPath(System.IO.Path.GetDirectoryName(path));
                }
                else if (System.IO.Directory.Exists(path))
                {
                    this.Type = NodeType.Directory;
                    this.Name = System.IO.Path.GetFileName(path);
                    this.Extension = "";
                    this.UpdateDate = (new System.IO.DirectoryInfo(path)).LastWriteTime;
                    this._parentPath = Xb.File.Tree.FormatPath(System.IO.Path.GetDirectoryName(path));
                }
                else
                {
                    Xb.Util.Out($"Xb.File.Tree.Node.Constructor: path not found [{path}]");
                    throw new ArgumentException($"Xb.File.Tree.Node.Constructor: path not found [{path}]");
                }

                this.FullPath = System.IO.Path.Combine(this._parentPath, this.Name);
                this.Tree = tree;
                this._childPaths = new List<string>();
            }


            /// <summary>
            /// Constructor
            /// コンストラクタ
            /// </summary>
            /// <param name="tree"></param>
            /// <param name="path"></param>
            /// <param name="updateDate"></param>
            /// <param name="isDirectory"></param>
            /// <remarks>
            /// passing all infomation
            /// </remarks>
            public Node(Xb.File.Tree tree
                      , string path
                      , DateTime updateDate
                      , bool isDirectory)
            {
                if (tree == null)
                {
                    Xb.Util.Out($"Xb.File.Tree.Node.Constructor: tree null");
                    throw new ArgumentException($"Xb.File.Tree.Node.Constructor: tree null");
                }

                if (string.IsNullOrEmpty(path))
                {
                    Xb.Util.Out($"Xb.File.Tree.Node.Constructor: path null");
                    throw new ArgumentException($"Xb.File.Tree.Node.Constructor: path null");
                }

                if (!isDirectory)
                {
                    this.Type = NodeType.File;
                    this.Name = System.IO.Path.GetFileName(path);
                    this.Extension = System.IO.Path.GetExtension(path);
                    this.UpdateDate = updateDate;
                    this._parentPath = Xb.File.Tree.FormatPath(System.IO.Path.GetDirectoryName(path));
                }
                else if (System.IO.Directory.Exists(path))
                {
                    this.Type = NodeType.Directory;
                    this.Name = System.IO.Path.GetFileName(path);
                    this.Extension = "";
                    this.UpdateDate = updateDate;
                    this._parentPath = Xb.File.Tree.FormatPath(System.IO.Path.GetDirectoryName(path));
                }

                this.FullPath = System.IO.Path.Combine(this._parentPath, this.Name);
                this.Tree = tree;
                this._childPaths = new List<string>();
            }


            /// <summary>
            /// Scan & refresh nodes
            /// 子ノードを走査する
            /// </summary>
            public void Scan()
            {
                if (this.Type == NodeType.File)
                    return;

                //直下のファイル／ディレクトリのパス文字列配列を取得する。
                var children = new List<string>();

                try
                {
                    children.AddRange(System.IO.Directory.GetFiles(this.FullPath)
                                                         .Select(Xb.File.Tree.FormatPath)
                                                         .ToArray());
                    children.AddRange(System.IO.Directory.GetDirectories(this.FullPath)
                                                         .Select(Xb.File.Tree.FormatPath)
                                                         .ToArray());
                }
                catch (Exception)
                {
                    Xb.Util.Out($"Xb.File.Tree.Node.Scan: Scan failure, may be not permitted [{this.FullPath}]");
                    return;
                }


                //以前存在していて現在は無いパスを削除する。
                var removeTargets = this._childPaths.Where(path => !children.Contains(path))
                                                    .ToArray();
                foreach (var removeTarget in removeTargets)
                {
                    this._childPaths.Remove(removeTarget);
                    this.Tree._nodes.Remove(removeTarget);
                }

                //新しく追加されたパスをループ
                foreach (var path in children.Where(path => !this._childPaths.Contains(path)))
                {
                    var node = new Xb.File.Tree.Node(this.Tree, path);
                    this.Tree._nodes.Add(path, node);
                    this._childPaths.Add(path);
                }
            }


            /// <summary>
            /// Scan & refresh nodes recursive on async
            /// 子ノードを再帰的に走査する
            /// </summary>
            /// <returns></returns>
            public async Task ScanRecursive()
            {
                if (this.Type == NodeType.File)
                    return;

                await Task.Run(() =>
                {
                    this.Scan();
                });

                //配下のディレクトリをループ
                foreach (var node in this.Children.Where(node => node.Type == NodeType.Directory))
                {
                    await node.ScanRecursive();
                }
            }


            /// <summary>
            /// Add child node
            /// 子ノードを追加する
            /// </summary>
            /// <param name="childNode"></param>
            public void AddChild(Xb.File.Tree.Node childNode)
            {
                if(!this._childPaths.Contains(childNode.FullPath))
                    this._childPaths.Add(childNode.FullPath);

                if (!this.Tree.Paths.Contains(childNode.FullPath))
                    this.Tree._nodes.Add(childNode.FullPath, childNode);
            }

            /// <summary>
            /// Add child-node array
            /// 子ノード配列を追加する
            /// </summary>
            /// <param name="childNodes"></param>
            public void AddRangeChildren(IEnumerable<Xb.File.Tree.Node> childNodes)
            {
                foreach (var childNode in childNodes)
                    this.AddChild(childNode);
            }


            /// <summary>
            /// Get all-children recursive
            /// 配下の全ノードを再帰的に取得する
            /// </summary>
            /// <returns></returns>
            public Xb.File.Tree.Node[] GetAllChildrenRecursive()
            {
                var result = new List<Xb.File.Tree.Node>();

                foreach (var path in this._childPaths)
                {
                    var child = this.Tree[path];
                    if(child == null)
                        continue;
                    
                    result.Add(child);

                    if (child.Type == NodeType.Directory)
                        result.AddRange(child.GetAllChildrenRecursive());
                }

                return result.ToArray();
            }


            /// <summary>
            /// Get first-node of matched needle
            /// 渡し値文字列が合致した最初の子ノードを返す
            /// </summary>
            /// <param name="needle"></param>
            /// <returns></returns>
            public Xb.File.Tree.Node Find(string needle)
            {
                foreach (var path in this._childPaths)
                {
                    var child = this.Tree[path];
                    if(child == null)
                        continue;

                    if (child.FullPath.IndexOf(needle, StringComparison.Ordinal) >= 0)
                        return child;
                }

                foreach (var path in this._childPaths)
                {
                    var child = this.Tree[path];
                    if (child == null)
                        continue;

                    var childResult = child.Find(needle);
                    if (childResult != null)
                        return childResult;
                }

                return null;
            }


            /// <summary>
            /// Get all-nodes of matched needle
            /// 渡し値文字列が合致した全ての子ノードを返す
            /// </summary>
            /// <param name="needle"></param>
            /// <returns></returns>
            public Xb.File.Tree.Node[] FindAll(string needle)
            {
                var result = new List<Xb.File.Tree.Node>();

                foreach (var path in this._childPaths)
                {
                    var child = this.Tree[path];
                    if (child == null)
                        continue;

                    if (child.FullPath.IndexOf(needle, StringComparison.Ordinal) >= 0)
                        result.Add(child);

                    result.AddRange(child.FindAll(needle));
                }

                return result.ToArray();
            }



            #region IDisposable Support
            private bool disposedValue = false; // 重複する呼び出しを検出するには

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        this._parentPath = null;

                        for (var i = 0; i < this._childPaths.Count; i++)
                            this._childPaths[i] = null;

                        this.Tree = null;

                        this.Name = null;
                        this.FullPath = null;
                        this.Extension = null;
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
}
