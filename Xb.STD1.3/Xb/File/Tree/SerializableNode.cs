using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xb.File.Tree
{
    public class SerializableNode
    {
        /// <summary>
        /// Parent-node
        /// 親ノード
        /// </summary>
        public SerializableNode Parent { get; set; }

        /// <summary>
        /// Child-Nodes array
        /// 子ノード配列
        /// </summary>
        public SerializableNode[] Children { get; set; }

        /// <summary>
        /// Node-name (not full-path)
        /// ノード名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Extension
        /// 拡張子
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Node type(file or directory)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Full-Path
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Last update-date
        /// 最終更新日時
        /// </summary>
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// ルートノードか否か
        /// is root node?
        /// </summary>
        public bool IsRootNode { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node"></param>
        public SerializableNode(INode node)
        {
            this.Parent = null;
            this.Children = new SerializableNode[]{};
            this.Name = node.Name;
            this.Extension = node.Extension;
            this.Type = (node.Type == NodeBase.NodeType.File)
                            ? "File"
                            : "Directory";
            this.FullPath = node.FullPath;
            this.UpdateDate = node.UpdateDate;
            this.IsRootNode = node.IsRootNode;
        }
    }
}
