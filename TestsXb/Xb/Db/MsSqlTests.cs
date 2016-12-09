using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsXb.Db
{
    [TestClass()]
    public class MsSqlTests : MsSqlBase, IDisposable
    {
        [TestMethod()]
        public void CreateTest()
        {
            this.Out("CreateTest Start.");
            Xb.Db.MsSql db;

            try
            {
                db = new Xb.Db.MsSql("MsSqlTests", "sa", "sa", "localhost", true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
            Assert.IsTrue(db.IsConnected);

            Assert.AreEqual(Server, db.Address);
            Assert.AreEqual(NameTarget, db.Name);
            Assert.AreEqual(UserId, db.User);
            Assert.AreEqual(Password, db.Password);

            Assert.AreEqual(Encoding.GetEncoding("Shift_JIS"), db.Encoding);
            Assert.IsFalse(db.IsInTransaction);

            Assert.AreEqual(3, db.Models.Count);
            Assert.AreEqual(3, db.TableNames.Count);

            db.Dispose();

            this.Out("CreateTest Emd.");
        }

        [TestMethod()]
        public void ValidateTest()
        {
            this.Out("ValidateTest Start.");

            var db = new Xb.Db.MsSql("MsSqlTests");
            Xb.Db.Model model;
            try
            {
                model = db.GetModel("Test");
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Assert.AreEqual("Test", model.TableName);

            Assert.AreEqual(4, model.Columns.Count);

            var row = model.NewRow();
            row["COL_STR"] = "1234567890";
            row["COL_DEC"] = 12.345;
            row["COL_INT"] = 2147483647;
            row["COL_DATETIME"] = new DateTime(2016, 1, 1, 19, 59, 59);

            var errors = model.Validate(row);
            if (errors.Length > 0)
            {
                foreach (var error in errors)
                {
                    this.Out(error.Name + ": " + error.Message);
                }
                Assert.Fail("エラーの値の検証でエラーが発生した。");
            }

            row = model.NewRow();
            errors = model.Validate(row);
            Assert.AreEqual(1, errors.Length);

            var err = errors[0];
            Assert.AreEqual("COL_STR", err.Name);
            this.Out(err.Name + ": " + err.Message);

            row = model.NewRow();
            row["COL_STR"] = "12345678901";
            errors = model.Validate(row);
            Assert.AreEqual(1, errors.Length);
            err = errors[0];
            Assert.AreEqual("COL_STR", err.Name);
            this.Out(err.Name + ": " + err.Message);

            row = model.NewRow();
            row["COL_STR"] = "NOT NULL";
            row["COL_DEC"] = 1.1234;
            errors = model.Validate(row);
            Assert.AreEqual(1, errors.Length);
            err = errors[0];
            Assert.AreEqual("COL_DEC", err.Name);
            this.Out(err.Name + ": " + err.Message);

            row = model.NewRow();
            row["COL_STR"] = "NOT NULL";
            row["COL_DEC"] = 123.123;
            errors = model.Validate(row);
            Assert.AreEqual(1, errors.Length);
            err = errors[0];
            Assert.AreEqual("COL_DEC", err.Name);
            this.Out(err.Name + ": " + err.Message);

            row = model.NewRow();
            row["COL_STR"] = "NOT NULL";
            row["COL_INT"] = 21474836471;
            errors = model.Validate(row);
            Assert.AreEqual(1, errors.Length);
            err = errors[0];
            Assert.AreEqual("COL_INT", err.Name);
            this.Out(err.Name + ": " + err.Message);

            row = model.NewRow();
            row["COL_STR"] = "NOT NULL";
            row["COL_DATETIME"] = "12/99/99";
            errors = model.Validate(row);
            Assert.AreEqual(1, errors.Length);
            err = errors[0];
            Assert.AreEqual("COL_DATETIME", err.Name);
            this.Out(err.Name + ": " + err.Message);


            model = db.GetModel("Test3");
            row = model.NewRow();
            errors = model.Validate(row);
            Assert.AreEqual(2, errors.Length);

            var errorColumns = errors.Select(col => col.Name).ToArray();
            Assert.IsTrue(errorColumns.Contains("COL_STR"));
            Assert.IsTrue(errorColumns.Contains("COL_INT"));
            this.Out(errors[0].Name + ": " +  errors[0].Message + "  /  " + errors[1].Name + ": " + errors[1].Message);
            

            db.Dispose();

            this.Out("ValidateTest End.");
        }

        [TestMethod()]
        public void ReadWriteTest()
        {
            this.Out("ReadWriteTest Start.");

            var db = new Xb.Db.MsSql("MsSqlTests");
            var model = db.GetModel("Test");

            var row = model.NewRow();
            row["COL_STR"] = "1234567890";
            row["COL_DEC"] = 12.345;
            row["COL_INT"] = 2147483647;
            row["COL_DATETIME"] = new DateTime(2016, 1, 1, 19, 59, 59);

            var errors = model.Insert(row);

            if (errors.Length > 0)
            {
                foreach (var error in errors)
                {
                    this.Out(error.Name + " /" + error.Message);
                }
                Assert.Fail("エラーの値の登録でエラーが発生した。");
            }


            var dt = this.Query("SELECT * FROM Test");
            Assert.AreEqual(1, dt?.Rows.Count ?? 0);

            var res = dt.Rows[0];
            Assert.AreEqual("1234567890", res["COL_STR"]);
            Assert.AreEqual((Decimal)12.345, res["COL_DEC"]);
            Assert.AreEqual((int)2147483647, res["COL_INT"]);
            Assert.AreEqual(new DateTime(2016, 1, 1, 19, 59, 59), res["COL_DATETIME"]);
            
            db.Dispose();

            this.Out("ReadWriteTest End.");
        }


        [TestMethod()]
        public void TransactionTest()
        {
            this.Out("TransactionTest Start.");

            var db = new Xb.Db.MsSql("MsSqlTests");

            //Assert.Fail();

            db.Dispose();

            this.Out("TransactionTest End.");
        }

        //[TestMethod()]
        //public void ExecuteTest()
        //{
        //    Assert.Fail();
        //}
    }
}