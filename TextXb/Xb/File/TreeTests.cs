using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xb.File;

namespace TextXb
{
    [TestClass()]
    public class TreeTests : FileBase
    {
        [TestMethod()]
        public async Task GetTreeRecursiveAsyncTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");

            Xb.File.Util.Delete(baseDir);
            Assert.IsFalse(Directory.Exists(baseDir));

            var structure = this.BuildDirectoryTree();
            var tree = await Xb.File.Tree.GetTreeRecursiveAsync(baseDir);

            this.OutHighlighted(tree.Paths);

            foreach (var path in structure.Directories)
            {
                Assert.IsTrue(tree.Paths.Contains(path));
                var node = tree[path];

                var info = new System.IO.DirectoryInfo(path);
                Assert.AreEqual(Tree.Node.NodeType.Directory, node.Type);
                Assert.AreEqual(System.IO.Path.GetFileName(path), node.Name);
                Assert.AreEqual("", node.Extension);
                Assert.AreEqual(info.LastWriteTime, node.UpdateDate);

                this.Out(path);
                this.Out(
                    $"Name: {node.Name}, Ext: {node.Extension}, Date: {node.UpdateDate}, ChildCount: {node.Children.Length}");
                this.OutHighlighted(node.Children.Select(n => n.Name).ToArray());
            }

            foreach (var path in structure.Files)
            {
                Assert.IsTrue(tree.Paths.Contains(path));
                var node = tree[path];

                var info = new System.IO.FileInfo(path);
                Assert.AreEqual(Tree.Node.NodeType.File, node.Type);
                Assert.AreEqual(System.IO.Path.GetFileName(path), node.Name);
                Assert.AreEqual(System.IO.Path.GetDirectoryName(path), node.ParentPath);
                Assert.AreEqual(System.IO.Path.GetExtension(path), node.Extension);
                Assert.AreEqual(info.LastWriteTime, node.UpdateDate);

                this.Out(path);
                this.Out(
                    $"Name: {node.Name}, Ext: {node.Extension}, Date: {node.UpdateDate}, ChildCount: {node.Children.Length}");
            }
        }

        [TestMethod()]
        public async Task FindTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");

            Xb.File.Util.Delete(baseDir);
            Assert.IsFalse(Directory.Exists(baseDir));

            //get all recursive
            var structure = this.BuildDirectoryTree();
            var tree = await Xb.File.Tree.GetTreeRecursiveAsync(baseDir);

            this.OutHighlighted(tree.Paths);

            var nodes = tree.FindAll(".txt");
            foreach (var node in nodes)
            {
                var path = node.FullPath;
                Assert.IsTrue(structure.Files.Contains(path));

                var info = new System.IO.FileInfo(path);
                Assert.AreEqual(Tree.Node.NodeType.File, node.Type);
                Assert.AreEqual(System.IO.Path.GetFileName(path), node.Name);
                Assert.AreEqual(System.IO.Path.GetDirectoryName(path), node.ParentPath);
                Assert.AreEqual(System.IO.Path.GetExtension(path), node.Extension);
                Assert.AreEqual(info.LastWriteTime, node.UpdateDate);

                this.Out(path);
                this.Out(
                    $"Name: {node.Name}, Ext: {node.Extension}, Date: {node.UpdateDate}, ChildCount: {node.Children.Length}");
            }

            Assert.AreEqual(7, nodes.Length);


            nodes = tree.RootNode.FindAll(".txt");
            foreach (var node in nodes)
            {
                var path = node.FullPath;
                Assert.IsTrue(structure.Files.Contains(path));

                var info = new System.IO.FileInfo(path);
                Assert.AreEqual(Tree.Node.NodeType.File, node.Type);
                Assert.AreEqual(System.IO.Path.GetFileName(path), node.Name);
                Assert.AreEqual(System.IO.Path.GetDirectoryName(path), node.ParentPath);
                Assert.AreEqual(System.IO.Path.GetExtension(path), node.Extension);
                Assert.AreEqual(info.LastWriteTime, node.UpdateDate);

                this.Out(path);
                this.Out(
                    $"Name: {node.Name}, Ext: {node.Extension}, Date: {node.UpdateDate}, ChildCount: {node.Children.Length}");
            }

            Assert.AreEqual(7, nodes.Length);


            var subdir1 = tree.FindAll("subdir1")
                              .FirstOrDefault(n => n.Type == Tree.Node.NodeType.Directory);

            Assert.AreNotEqual(null, subdir1);
            var path2 = subdir1.FullPath;
            var info2 = new System.IO.DirectoryInfo(path2);
            Assert.AreEqual(Tree.Node.NodeType.Directory, subdir1.Type);
            Assert.AreEqual(System.IO.Path.GetFileName(path2), subdir1.Name);
            Assert.AreEqual("", subdir1.Extension);
            Assert.AreEqual(info2.LastWriteTime, subdir1.UpdateDate);

            nodes = subdir1.FindAll(".txt");
            Assert.AreEqual(2, nodes.Length);

            var node3 = subdir1.Find("xt");
            Assert.IsTrue((new string[] { "subFile1.txt", "subFile2.txt" }).Contains(node3.Name));

            //get direct-child only
            tree = Xb.File.Tree.GetTree(baseDir);

            this.OutHighlighted(tree.Paths);

            nodes = tree.FindAll(".txt");
            foreach (var node in nodes)
            {
                var path = node.FullPath;
                Assert.IsTrue(structure.Files.Contains(path));

                var info = new System.IO.FileInfo(path);
                Assert.AreEqual(Tree.Node.NodeType.File, node.Type);
                Assert.AreEqual(System.IO.Path.GetFileName(path), node.Name);
                Assert.AreEqual(System.IO.Path.GetDirectoryName(path), node.ParentPath);
                Assert.AreEqual(System.IO.Path.GetExtension(path), node.Extension);
                Assert.AreEqual(info.LastWriteTime, node.UpdateDate);

                this.Out(path);
                this.Out(
                    $"Name: {node.Name}, Ext: {node.Extension}, Date: {node.UpdateDate}, ChildCount: {node.Children.Length}");
            }
            Assert.AreEqual(3, nodes.Length);
        }

        [TestMethod()]
        public async Task ScanTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");

            Xb.File.Util.Delete(baseDir);
            Assert.IsFalse(Directory.Exists(baseDir));

            //get all recursive
            var structure = this.BuildDirectoryTree();
            var tree = await Xb.File.Tree.GetTreeRecursiveAsync(baseDir);

            this.OutHighlighted(tree.Paths);


            //get subdir, child file
            var subDir1 = tree.FindAll("subdir1")
                  .FirstOrDefault(n => n.Type == Tree.Node.NodeType.Directory);
            Assert.AreNotEqual(null, subDir1);

            var subFile1 = subDir1.Find("subFile1");
            Assert.AreNotEqual(null, subFile1);

            //before remove
            Assert.IsTrue(subDir1.Children.Any(n => n.FullPath.IndexOf("subFile1.txt") >= 0));
            Assert.AreEqual(2, subDir1.Children.Length);
            Assert.AreNotEqual(null, tree.Find("subFile1"));
            Assert.IsTrue(tree.Nodes.Any(n => n.FullPath.IndexOf("subFile1.txt") >= 0));
            Assert.AreEqual(14, tree.Nodes.Length);

            //remove file
            Xb.File.Util.Delete(subFile1.FullPath);
            subFile1.Scan();

            //sync children
            Assert.IsFalse(subDir1.Children.Any(n => n.FullPath.IndexOf("subFile1.txt") >= 0));
            Assert.AreEqual(1, subDir1.Children.Length);
            Assert.AreEqual(null, tree.Find("subFile1"));
            Assert.IsFalse(tree.Nodes.Any(n => n.FullPath.IndexOf("subFile1.txt") >= 0));
            Assert.AreEqual(13, tree.Nodes.Length);

            //remove dir and child-file
            Xb.File.Util.Delete(subDir1.FullPath);
            subDir1.Scan();

            Assert.AreEqual(11, tree.Nodes.Length);
            Assert.AreEqual(null, tree.Find("subdir1"));
            Assert.IsFalse(tree.Nodes.Any(n => n.FullPath.IndexOf("subFile1.txt") >= 0));
            Assert.IsFalse(tree.Nodes.Any(n => n.FullPath.IndexOf("subFile2.txt") >= 0));
            Assert.IsFalse(tree.Nodes.Any(n => n.FullPath.IndexOf("subdir1") >= 0));



            //
            Xb.File.Util.Delete(baseDir);
            structure = this.BuildDirectoryTree();
            tree = await Xb.File.Tree.GetTreeRecursiveAsync(baseDir);

            subDir1 = tree.FindAll("subdir1")
                          .FirstOrDefault(n => n.Type == Tree.Node.NodeType.Directory);

            Assert.AreEqual(14, tree.Nodes.Length);
            Assert.AreNotEqual(null, tree.Find("subdir1"));
            Assert.IsTrue(tree.Nodes.Any(n => n.FullPath.IndexOf("subFile1.txt") >= 0));
            Assert.IsTrue(tree.Nodes.Any(n => n.FullPath.IndexOf("subFile2.txt") >= 0));
            Assert.IsTrue(tree.Nodes.Any(n => n.FullPath.IndexOf("subdir1") >= 0));

            Xb.File.Util.Delete(subDir1.FullPath);
            await tree.ScanRecursiveAsync();

            Assert.AreEqual(11, tree.Nodes.Length);
            Assert.AreEqual(null, tree.Find("subdir1"));
            Assert.IsFalse(tree.Nodes.Any(n => n.FullPath.IndexOf("subFile1.txt") >= 0));
            Assert.IsFalse(tree.Nodes.Any(n => n.FullPath.IndexOf("subFile2.txt") >= 0));
            Assert.IsFalse(tree.Nodes.Any(n => n.FullPath.IndexOf("subdir1") >= 0));
        }
    }
}
