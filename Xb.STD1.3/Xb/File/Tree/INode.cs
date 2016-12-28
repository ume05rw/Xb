﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xb.File.Tree
{
    public interface INode : IDisposable
    {
        event NodeBase.ChildAddEventHandler ChildAdded;
        event NodeBase.DeleteEventHandler Deleted;

        /// <summary>
        /// Tree(= node manager) object
        /// ノード管理オブジェクト
        /// </summary>
        Xb.File.Tree.ITree Tree { get; }

        /// <summary>
        /// Parent-node
        /// 親ノード
        /// </summary>
        Xb.File.Tree.INode Parent { get; }

        /// <summary>
        /// Child-Nodes array
        /// 子ノード配列
        /// </summary>
        Xb.File.Tree.INode[] Children { get; }

        /// <summary>
        /// Node-name (not full-path)
        /// ノード名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Extension
        /// 拡張子
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// Node type(file or directory)
        /// </summary>
        Xb.File.Tree.NodeBase.NodeType Type { get; }

        /// <summary>
        /// Full-Path
        /// </summary>
        string FullPath { get; }

        /// <summary>
        /// Last update-date
        /// 最終更新日時
        /// </summary>
        DateTime UpdateDate { get; }

        /// <summary>
        /// ルートノードか否か
        /// is root node?
        /// </summary>
        bool IsRootNode { get; }


        /// <summary>
        /// Scan & refresh nodes
        /// 子ノードを走査する
        /// </summary>
        void Scan();

        /// <summary>
        /// Scan & refresh nodes recursive on async
        /// 子ノードを再帰的に走査する
        /// </summary>
        /// <returns></returns>
        Task ScanRecursiveAsync();

        /// <summary>
        /// Get serializable-object of tree structure
        /// 配下のツリー構造をシリアライズ可能なオブジェクトとして取得する
        /// </summary>
        /// <returns></returns>
        SerializableNode GetSerializable();


        /// <summary>
        /// Get all-children recursive
        /// 配下の全ノードを再帰的に取得する
        /// </summary>
        /// <returns></returns>
        Xb.File.Tree.INode[] GetAllChildrenRecursive();

        /// <summary>
        /// Get first-node of matched needle
        /// 渡し値文字列が合致した最初の子ノードを返す
        /// </summary>
        /// <param name="needle"></param>
        /// <returns></returns>
        Xb.File.Tree.INode Find(string needle);

        /// <summary>
        /// Get all-nodes of matched needle
        /// 渡し値文字列が合致した全ての子ノードを返す
        /// </summary>
        /// <param name="needle"></param>
        /// <returns></returns>
        Xb.File.Tree.INode[] FindAll(string needle);


        /// <summary>
        /// Get byte-array of node
        /// ノードのデータをバイト配列で取得する
        /// </summary>
        /// <returns></returns>
        byte[] GetBytes();

        /// <summary>
        /// Get byte-array of node on async
        /// ノードのデータをバイト配列で取得する
        /// </summary>
        /// <returns></returns>
        Task<byte[]> GetBytesAsync();

        /// <summary>
        /// Get byte-array of node
        /// ノードのデータをバイト配列で取得する
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        byte[] GetBytes(long offset, int length);

        /// <summary>
        /// Get byte-array of node on async
        /// ノードのデータをバイト配列で取得する
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        Task<byte[]> GetBytesAsync(long offset, int length);

        /// <summary>
        /// Overwrite data of node
        /// バイト配列データをノードに上書きする。
        /// </summary>
        /// <param name="bytes"></param>
        void WriteBytes(byte[] bytes);

        /// <summary>
        /// Overwrite data of node on async
        /// バイト配列データをノードに上書きする。
        /// </summary>
        /// <param name="bytes"></param>
        Task WriteBytesAsync(byte[] bytes);
        //want to Stream handling, but impossible to handle with SharpCifs.Std

        /// <summary>
        /// Create child-node
        /// 子ノードを追加する
        /// </summary>
        /// <param name="name"></param>
        INode CreateChild(string name, Xb.File.Tree.NodeBase.NodeType type);

        /// <summary>
        /// Delete myself-node from tree
        /// Treeから自分自身を削除する。
        /// </summary>
        void Delete();
    }
}
