using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TextXb
{
    [TestClass()]
    public class ZipTests : FileBase
    {
        [TestMethod()]
        public void ToZipUnzipTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            var zipFileName = Path.Combine(curDir, "baseDir.zip");

            Xb.File.Util.Delete(zipFileName);
            Xb.File.Util.Delete(baseDir);

            Assert.IsFalse(File.Exists(zipFileName));
            Assert.IsFalse(Directory.Exists(baseDir));

            var structure = this.BuildDirectoryTree();

            foreach (var directory in structure.Directories)
            {
                Assert.IsTrue(Directory.Exists(directory));
            }
            foreach (var file in structure.Files)
            {
                Assert.IsTrue(File.Exists(file));
            }

            Xb.File.Zip.ToZip(baseDir);

            Assert.IsTrue(File.Exists(zipFileName));

            Xb.File.Util.Delete(baseDir);

            foreach (var directory in structure.Directories)
            {
                Assert.IsFalse(Directory.Exists(directory));
            }
            foreach (var file in structure.Files)
            {
                Assert.IsFalse(File.Exists(file));
            }

            Assert.IsFalse(Directory.Exists(baseDir));

            Xb.File.Zip.Unzip(zipFileName);

            foreach (var directory in structure.Directories)
            {
                Assert.IsTrue(Directory.Exists(directory));
            }
            foreach (var file in structure.Files)
            {
                Assert.IsTrue(File.Exists(file));
            }
        }

        [TestMethod()]
        public void ConstructorTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            var zipFileName = Path.Combine(curDir, "baseDir.zip");

            //create empty zip
            Xb.File.Util.Delete(zipFileName);
            var zip = new Xb.File.Zip(zipFileName, false);
            zip.Dispose();
            Assert.IsTrue(File.Exists(zipFileName));

            //open exist-zip readonly
            Xb.File.Util.Delete(zipFileName);
            Xb.File.Util.Delete(baseDir);
            this.BuildDirectoryTree();
            Xb.File.Zip.ToZip(baseDir);
            zip = new Xb.File.Zip(zipFileName);
            zip.Dispose();
            Assert.IsTrue(File.Exists(zipFileName));
        }


        [TestMethod()]
        public void GetBytesTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            var zipFileName = Path.Combine(curDir, "baseDir.zip");

            //open exist-zip readonly
            Xb.File.Util.Delete(zipFileName);
            Xb.File.Util.Delete(baseDir);
            this.BuildDirectoryTree();
            Xb.File.Zip.ToZip(baseDir);
            var zip = new Xb.File.Zip(zipFileName);

            var entry = zip.Entries
                           .First(ent => ent.FullName
                                            .IndexOf("file2.txt"
                                                   , StringComparison.Ordinal) >= 0);

            var bytes = zip.GetBytes(entry);
            Assert.AreEqual("中身を書き込んであるんだよ", Encoding.UTF8.GetString(bytes));
            Assert.AreEqual(Xb.File.Util.GetText(@"baseDir\dir1\file2.txt"), Encoding.UTF8.GetString(bytes));

            entry = zip.Entries
                       .First(ent => ent.FullName
                                        .IndexOf("file1.txt"
                                               , StringComparison.Ordinal) >= 0);
            bytes = zip.GetBytes(entry);
            Assert.AreEqual(0, bytes.Length);

            entry = zip.Entries
                       .First(ent => ent.FullName
                                        .IndexOf("マルチバイトЙ"
                                               , StringComparison.Ordinal) >= 0);
            bytes = zip.GetBytes(entry);
            Assert.AreEqual(0, bytes.Length);

            zip.Dispose();

            Xb.File.Util.Delete(zipFileName);
            Xb.File.Util.Delete(baseDir);
        }

        [TestMethod()]
        public void DeleteTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            var zipFileName = Path.Combine(curDir, "baseDir.zip");

            //open exist-zip readonly
            Xb.File.Util.Delete(zipFileName);
            Xb.File.Util.Delete(baseDir);
            this.BuildDirectoryTree();
            Xb.File.Zip.ToZip(baseDir);
            Xb.File.Util.Delete(baseDir);
            var zip = new Xb.File.Zip(zipFileName, false);

            //update exist-entry
            var entry = zip.Entries
                           .First(ent => ent.FullName
                                            .IndexOf("file2.txt"
                                                   , StringComparison.Ordinal) >= 0);

            zip.Delete(entry);
            Assert.IsFalse(zip.Entries.Any(ent => ent.FullName.IndexOf("file2.txt", StringComparison.Ordinal) >= 0));
            zip.Dispose();

            Xb.File.Zip.Unzip(zipFileName);
            Assert.IsFalse(System.IO.File.Exists(@"baseDir\dir1\file2.txt"));

            Xb.File.Util.Delete(zipFileName);
            Xb.File.Util.Delete(baseDir);
        }

        [TestMethod()]
        public void WriteBytesTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            var zipFileName = Path.Combine(curDir, "baseDir.zip");

            //open exist-zip readonly
            Xb.File.Util.Delete(zipFileName);
            Xb.File.Util.Delete(baseDir);
            this.BuildDirectoryTree();
            Xb.File.Zip.ToZip(baseDir);
            var zip = new Xb.File.Zip(zipFileName, false);

            //update exist-entry
            var entry = zip.Entries
                .First(ent => ent.FullName
                                 .IndexOf("file2.txt"
                                        , StringComparison.Ordinal) >= 0);

            entry = zip.WriteBytes(entry, Encoding.UTF8.GetBytes("中身を更新してみたよ"));

            var bytes = zip.GetBytes(entry);
            Assert.AreEqual("中身を更新してみたよ", Encoding.UTF8.GetString(bytes));
            Assert.AreNotEqual(Xb.File.Util.GetText(@"baseDir\dir1\file2.txt"), Encoding.UTF8.GetString(bytes));

            //create new entry
            var newEntryString = @"baseDir/dir1/newFile.txt";
            entry = zip.GetNewEntry(newEntryString);
            entry = zip.WriteBytes(entry, Encoding.UTF8.GetBytes("新しいファイルですよ"));
            bytes = zip.GetBytes(entry);
            Assert.AreEqual("新しいファイルですよ", Encoding.UTF8.GetString(bytes));

            zip.Dispose();
            Xb.File.Util.Delete(baseDir);
            Xb.File.Zip.Unzip(zipFileName);
            Assert.IsTrue(System.IO.File.Exists(@"baseDir\dir1\file2.txt"));
            Assert.IsTrue(System.IO.File.Exists(newEntryString));

            Assert.AreEqual("中身を更新してみたよ", Xb.File.Util.GetText(@"baseDir\dir1\file2.txt"));
            Assert.AreEqual("新しいファイルですよ", Xb.File.Util.GetText(newEntryString));

            Xb.File.Util.Delete(zipFileName);
            Xb.File.Util.Delete(baseDir);
        }
    }
}
