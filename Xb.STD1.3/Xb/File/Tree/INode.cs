using System;
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

        Xb.File.Tree.ITree Tree { get; }
        Xb.File.Tree.INode Parent { get; }
        Xb.File.Tree.INode[] Children { get; }
        string Name { get; }
        string Extension { get; }
        Xb.File.Tree.NodeBase.NodeType Type { get; }
        string FullPath { get; }
        DateTime UpdateDate { get; }
        bool IsRootNode { get; }

        void Scan();
        Task ScanRecursiveAsync();
        SerializableNode GetSerializable();

        Xb.File.Tree.INode[] GetAllChildrenRecursive();
        Xb.File.Tree.INode Find(string needle);
        Xb.File.Tree.INode[] FindAll(string needle);

        byte[] GetBytes();
        void WriteBytes(byte[] bytes);
        //want to Stream handling, but impossible to handle with SharpCifs.Std

        INode CreateChild(string name
                        , Xb.File.Tree.NodeBase.NodeType type);
        void Delete();
    }
}
