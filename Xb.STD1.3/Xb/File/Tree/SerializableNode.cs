using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xb.File.Tree
{
    public class SerializableNode
    {
        public SerializableNode Parent { get; set; }
        public SerializableNode[] Children { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Type { get; set; }
        public string FullPath { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool IsRootNode { get; set; }

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
