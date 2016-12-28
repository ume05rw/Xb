﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xb.File.Tree
{
    public abstract class NodeBase : Xb.File.Tree.INode
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
        /// Child add event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void ChildAddEventHandler(object sender, NodeEventArgs e);

        /// <summary>
        /// Child added event
        /// </summary>
        public event ChildAddEventHandler ChildAdded;

        /// <summary>
        /// Delete-myself event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void DeleteEventHandler(object sender, NodeEventArgs e);

        /// <summary>
        /// Deleteed-myself event
        /// </summary>
        public event DeleteEventHandler Deleted;

        /// <summary>
        /// Tree(= node manager) object
        /// ノード管理オブジェクト
        /// </summary>
        public virtual Xb.File.Tree.ITree Tree { get; protected set; }

        /// <summary>
        /// Parent-node
        /// 親ノード
        /// </summary>
        public Xb.File.Tree.INode Parent => this.Tree.GetNode(this.ParentPath);

        /// <summary>
        /// Child-Nodes array
        /// 子ノード配列
        /// </summary>
        public Xb.File.Tree.INode[] Children => this.Tree.GetNodes(this.ChildPaths);


        /// <summary>
        /// Parent-Node full-path
        /// 親ノードのフルパス
        /// </summary>
        protected string ParentPath { get; set; }

        /// <summary>
        /// Child-Node array of full-path(key)
        /// 子ノードのフルパス配列
        /// </summary>
        protected List<string> ChildPaths { get; set; }

        /// <summary>
        /// Node-name (not full-path)
        /// ノード名称
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Extension
        /// 拡張子
        /// </summary>
        public string Extension { get; protected set; }

        /// <summary>
        /// Node type(file or directory)
        /// </summary>
        public NodeType Type { get; protected set; }

        /// <summary>
        /// Full-Path
        /// </summary>
        public string FullPath { get; protected set; }

        /// <summary>
        /// Last update-date
        /// 最終更新日時
        /// </summary>
        public DateTime UpdateDate { get; protected set; }

        /// <summary>
        /// ルートノードか否か
        /// is root node?
        /// </summary>
        public bool IsRootNode => (this.Parent == null);




        /// <summary>
        /// Constructor
        /// コンストラクタ
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="path"></param>
        /// <param name="updateDate"></param>
        /// <param name="type"></param>
        protected NodeBase(Xb.File.Tree.ITree tree
                         , string path
                         , DateTime updateDate
                         , NodeType type = NodeType.File)
        {
            if (tree == null)
            {
                Xb.Util.Out($"Xb.File.Tree.NodeBase.Constructor: tree null");
                throw new ArgumentException($"Xb.File.TreeBase.Node.Constructor: tree null");
            }

            if (string.IsNullOrEmpty(path))
            {
                Xb.Util.Out($"Xb.File.Tree.NodeBase.Constructor: path null");
                throw new ArgumentException($"Xb.File.TreeBase.Node.Constructor: path null");
            }

            switch (type)
            {
                case NodeType.File:

                    this.Type = NodeType.File;
                    this.Name = System.IO.Path.GetFileName(path);
                    this.Extension = System.IO.Path.GetExtension(path);
                    this.UpdateDate = updateDate;
                    this.ParentPath = TreeBase.FormatPath(System.IO.Path.GetDirectoryName(path));

                    break;
                case NodeType.Directory:

                    this.Type = NodeType.Directory;
                    this.Name = System.IO.Path.GetFileName(path);
                    this.Extension = "";
                    this.UpdateDate = updateDate;
                    this.ParentPath = TreeBase.FormatPath(System.IO.Path.GetDirectoryName(path));

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), $"Xb.File.Tree.NodeBase.Constructor: undefined type[{type}]");
            }

            this.FullPath = TreeBase.FormatPath(System.IO.Path.Combine(this.ParentPath, this.Name));
            this.Tree = tree;
            this.ChildPaths = new List<string>();
        }


        /// <summary>
        /// Scan & refresh nodes
        /// 子ノードを走査する
        /// </summary>
        public virtual void Scan()
        {
            throw new NotImplementedException("Xb.File.Tree.NodeBase.Scan: Execute only subclass");

            //自分自身を示すパスが実システム上に存在しなくなったら、破棄する。
            //this.RemoveMeIfNotExists();

            //以下はディレクトリのみ。ファイルは子が居ないので。
            if (this.Type == NodeType.File)
                return;

            //実システム上の自身直下ノードのパス文字列配列を取得する。
            //var nowChildren = this.GetChildrenPaths();
                
            //以前存在していて現在は無いパスのノードを削除する。

            //実システム上に新しく追加されたパスのノードを追加
        }


        /// <summary>
        /// Scan & refresh nodes recursive on async
        /// 子ノードを再帰的に走査する
        /// </summary>
        /// <returns></returns>
        public virtual async Task ScanRecursiveAsync()
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
                await node.ScanRecursiveAsync();
            }
        }


        /// <summary>
        /// Get serializable-object of tree structure
        /// 配下のツリー構造をシリアライズ可能なオブジェクトとして取得する
        /// </summary>
        /// <returns></returns>
        public virtual SerializableNode GetSerializable()
        {
            var result = new SerializableNode(this);

            var children = new List<SerializableNode>();
            foreach (var child in this.Children)
            {
                var childSerializable = child.GetSerializable();
                childSerializable.Parent = result;
                children.Add(childSerializable);
            }
            result.Children = children.ToArray();

            return result;
        }


        /// <summary>
        /// Get all-children recursive
        /// 配下の全ノードを再帰的に取得する
        /// </summary>
        /// <returns></returns>
        public virtual Xb.File.Tree.INode[] GetAllChildrenRecursive()
        {
            var result = new List<Xb.File.Tree.INode>();

            foreach (var path in this.ChildPaths)
            {
                var child = this.Tree.GetNode(path);
                if (child == null)
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
        public virtual Xb.File.Tree.INode Find(string needle)
        {
            foreach (var path in this.ChildPaths)
            {
                var child = this.Tree.GetNode(path);
                if (child == null)
                    continue;

                if (child.FullPath.IndexOf(needle, StringComparison.Ordinal) >= 0)
                    return child;
            }

            foreach (var path in this.ChildPaths)
            {
                var child = this.Tree.GetNode(path);
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
        public virtual Xb.File.Tree.INode[] FindAll(string needle)
        {
            var result = new List<Xb.File.Tree.INode>();

            foreach (var path in this.ChildPaths)
            {
                var child = this.Tree.GetNode(path);
                if (child == null)
                    continue;

                if (child.FullPath.IndexOf(needle, StringComparison.Ordinal) >= 0)
                    result.Add(child);

                result.AddRange(child.FindAll(needle));
            }

            return result.ToArray();
        }


        /// <summary>
        /// Get byte-array of node
        /// ノードのデータをバイト配列で取得する
        /// </summary>
        /// <returns></returns>
        public virtual byte[] GetBytes()
        {
            throw new NotImplementedException("Xb.File.Tree.NodeBase.GetBytes: Execute only subclass");
        }


        /// <summary>
        /// Get byte-array of node on async
        /// ノードのデータをバイト配列で取得する
        /// </summary>
        /// <returns></returns>
        public virtual async Task<byte[]> GetBytesAsync()
        {
            return await Task.Run(() => this.GetBytes());
        }


        /// <summary>
        /// Get byte-array of node
        /// ノードのデータをバイト配列で取得する
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public virtual byte[] GetBytes(long offset, int length)
        {
            throw new NotImplementedException("Xb.File.Tree.NodeBase.GetBytes: Execute only subclass");
        }


        /// <summary>
        /// Get byte-array of node on async
        /// ノードのデータをバイト配列で取得する
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public virtual async Task<byte[]> GetBytesAsync(long offset, int length)
        {
            return await Task.Run(() => this.GetBytes(offset, length));
        }


        /// <summary>
        /// Overwrite data of node
        /// バイト配列データをノードに上書きする。
        /// </summary>
        /// <param name="bytes"></param>
        public virtual void WriteBytes(byte[] bytes)
        {
            throw new NotImplementedException("Xb.File.Tree.NodeBase.WriteBytes: Execute only subclass");
        }


        /// <summary>
        /// Overwrite data of node on async
        /// バイト配列データをノードに上書きする。
        /// </summary>
        /// <param name="bytes"></param>
        public virtual async Task WriteBytesAsync(byte[] bytes)
        {
            await Task.Run(() => this.WriteBytes(bytes));
        }



        /// <summary>
        /// Create child-node
        /// 子ノードを追加する
        /// </summary>
        /// <param name="name"></param>
        public virtual INode CreateChild(string name
                                       , NodeType type = NodeType.File)
        {
            throw new NotImplementedException("Xb.File.Tree.NodeBase.CreateChild: Execute only subclass");

            //1.Validate you need
            //2.Create new INode-instance
            //3.Call this.`AddChild` method
        }

        
        /// <summary>
        /// Append new-node to child-list & tree
        /// 新規ノードを、子リストとTreeインスタンスに追加する
        /// </summary>
        /// <param name="node"></param>
        protected virtual void AppendChild(INode node)
        {
            if (this.Type == NodeType.File)
                throw new InvalidOperationException("Xb.File.Tree.NodeBase.AddChild: Not directory");

            var childPath = TreeBase.FormatPath(System.IO.Path.Combine(this.FullPath, node.Name));

            if (this.ChildPaths.Contains(childPath))
                throw new InvalidOperationException($"Xb.File.Tree.NodeBase.AddChild: Exist node [{childPath}]");

            if(childPath != node.FullPath)
                throw new InvalidOperationException($"Xb.File.Tree.NodeBase.AddChild: Invalid relationship");

            node.Deleted += this.OnChildRemoved;
            this.ChildPaths.Add(childPath);
            this.ChildAdded?.Invoke(this, new NodeEventArgs(node));
        }


        /// <summary>
        /// Delete myself-node from tree
        /// Treeから自分自身を削除する。
        /// </summary>
        public virtual void Delete()
        {
            throw new NotImplementedException("Xb.File.Tree.NodeBase.Delete: Execute only subclass");

            //1.Validate you need
            //2.Call this.`Dispose` method
        }


        /// <summary>
        /// child-node removed event
        /// 子ノードの削除イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnChildRemoved(object sender, NodeEventArgs e)
        {
            if (e.Node.Parent != this)
                throw new InvalidOperationException($"Xb.File.Tree.NodeBase.RemoveChild: Invalid relationship");

            if (!this.ChildPaths.Contains(e.Node.FullPath))
                throw new InvalidOperationException($"Xb.File.Tree.NodeBase.RemoveChild: Child-list broken");

            e.Node.Deleted -= this.OnChildRemoved;
            this.ChildPaths.Remove(e.Node.FullPath);
        }


        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //直下の子ノードを破棄
                    foreach (var node in this.Children)
                        node?.Delete();

                    this.Deleted?.Invoke(this, new NodeEventArgs(this));

                    this.ParentPath = null;

                    for (var i = 0; i < this.ChildPaths.Count; i++)
                        this.ChildPaths[i] = null;

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
