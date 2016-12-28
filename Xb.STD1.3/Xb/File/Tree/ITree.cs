using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xb.File.Tree
{
    public interface ITree : IDisposable
    {
        Xb.File.Tree.INode RootNode { get; }
        string[] Paths { get; }
        Xb.File.Tree.INode[] Nodes { get; }

        Xb.File.Tree.INode GetNode(string path);
        Xb.File.Tree.INode[] GetNodes(ICollection<string> paths);
        Xb.File.Tree.INode Find(string needle);
        Xb.File.Tree.INode[] FindAll(string needle);
        Task ScanRecursiveAsync();
    }
}
