﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using libDatabaseHelper.classes.sqlce;
using libDatabaseHelper.classes.generic;
using libDatabaseHelperUnitTests.forms;
using libDatabaseHelper.forms;
using System.IO;
using System.Data;

namespace libDatabaseHelperUnitTests
{
    public class SampleTable1 : DatabaseEntity, IComboBoxItem
    {
        [TableColumn(IsPrimaryKey = true, ShouldIncludeInTable = true, IsAutogenerated = false, GridDisplayName = "Column 1")]
        public int Column1;

        [TableColumn(IsPrimaryKey = false, ShouldIncludeInTable = true, IsAutogenerated = false, GridDisplayName = "Column 2")]
        public string Column2;

        #region IComboBoxItem Members

        public object GetID()
        {
            return Column1;
        }

        public string GetSelectQueryItems()
        {
            return "[OBJ].Column2";
        }

        #endregion
    }

    public class SampleTable2 : DatabaseEntity
    {
        [TableColumn(IsPrimaryKey = true, ShouldIncludeInTable = true, IsAutogenerated = false, GridDisplayName = "Column 1")]
        public int Column1;

        [TableColumn(IsPrimaryKey = false, ShouldIncludeInTable = true, IsAutogenerated = false, GridDisplayName = "Column 2")]
        public string Column2;

        [TableColumn(IsPrimaryKey = false, ShouldIncludeInTable = true, IsAutogenerated = true, GridDisplayName = "Column 3", ReferencedClass = typeof(SampleTable1), ReferencedField = "Column1")]
        public int Column3;
    }

    public class InvalidSampleTable3_NoPrimaryKey : DatabaseEntity
    {
        [TableColumn(IsPrimaryKey = false, ShouldIncludeInTable = true, IsAutogenerated = false, GridDisplayName = "Column 1")]
        public int Column1;

        [TableColumn(IsPrimaryKey = false, ShouldIncludeInTable = true, IsAutogenerated = false, GridDisplayName = "Column 2")]
        public string Column2;

        [TableColumn(IsPrimaryKey = false, ShouldIncludeInTable = true, IsAutogenerated = true, GridDisplayName = "Column 3", ReferencedClass = typeof(SampleTable1), ReferencedField = "Column1")]
        public int Column3;
    }

    public class InvalidSampleTable4_NoColumns : DatabaseEntity
    {
        public int Column1;
        public string Column2;
        public int Column3;
    }

    public class Database1_NormalTable1 : DatabaseEntity
    {
        [TableColumn(IsPrimaryKey = true, ShouldIncludeInTable = true, IsAutogenerated = false, GridDisplayName = "Column 1")]
        public int Column1;

        [TableColumn(IsPrimaryKey = false, ShouldIncludeInTable = true, IsAutogenerated = false, GridDisplayName = "Column 2")]
        public string Column2;
    }

    public class Database2_NormalTable2 : DatabaseEntity
    {
        [TableColumn(IsPrimaryKey = true, ShouldIncludeInTable = true, IsAutogenerated = false, GridDisplayName = "Column 1")]
        public int Column1;

        [TableColumn(IsPrimaryKey = false, ShouldIncludeInTable = true, IsAutogenerated = false, GridDisplayName = "Column 2")]
        public string Column2;
    }

    public class NonProperlyImplementedClass : GenericDatabaseEntity
    {
        [TableColumn(IsPrimaryKey = true, ShouldIncludeInTable = true, IsAutogenerated = false, GridDisplayName = "Column 1")]
        public int Column1;

        [TableColumn(IsPrimaryKey = false, ShouldIncludeInTable = true, IsAutogenerated = false, GridDisplayName = "Column 2")]
        public string Column2;

        public NonProperlyImplementedClass() { }

        protected override DatabaseType FetchDatbaseType()
        {
            return DatabaseType.MySQL;
        }
    }

    /// <summary>
    /// Summary description for GenericTests
    /// </summary>
    [TestFixture]
    public class SqlCETests
    {
        public SqlCETests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private static int _unique_int_key = 0;

        public static object GenerateStringKey(string table, string field)
        {
            return DateTime.Now.Ticks.ToString();
        }

        public static int GenerateIntKey(string table, string field)
        {
            return ++_unique_int_key;
        }

        public static void PerformCleanUp()
        {
            Console.Write("- Performing Cleanup... ");
            try
            {
                if (File.Exists("dblocaldata.sdf"))
                {
                    File.Delete("dblocaldata.sdf");
                }

                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\libDBHelderSampleFolder1\\SampleDatabase1.sdf"))
                {
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\libDBHelderSampleFolder1\\SampleDatabase1.sdf");
                }

                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\libDBHelderSampleFolder1\\TestDatabase1.sdf"))
                {
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\libDBHelderSampleFolder1\\TestDatabase1.sdf");
                }

                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\libDBHelderSampleFolder1\\TestDatabase2.sdf"))
                {
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\libDBHelderSampleFolder1\\TestDatabase2.sdf");
                }
                Console.WriteLine("[OK]");
            }
            catch 
            {
                Console.WriteLine("[FAILED]");
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [SetUp]
        public void TEST_Setup()
        {
            Console.WriteLine("======================================== [TEST START] ========================================");
            GenericConnectionManager.CloseAllConnections();
            PerformCleanUp();
            UniversalDataCollector.CleanUp();

            GenericConnectionManager.RegisterConnectionManager<ConnectionManager>();
            GenericDatabaseManager.RegisterDatabaseManager<DatabaseManager>(true);

            var dbFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\libDBHelderSampleFolder1\\SampleDatabase1.sdf";
            GenericUtils.CreateFolderStructure(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\libDBHelderSampleFolder1");
            GenericConnectionManager.GetConnectionManager(DatabaseType.SqlCE).SetConnectionString("Data Source=" + dbFile + ";Persist Security Info=False;");

            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).DropTable<SampleTable1>();
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).DropTable<SampleTable2>();
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).DropTable<InvalidSampleTable3_NoPrimaryKey>();

            Assert.IsTrue(GenericConnectionManager.GetConnectionManager(DatabaseType.SqlCE).GetConnection() != null);
        }

        [TearDown]
        public void TEST_Cleanup()
        {
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).DropTable<SampleTable1>();
            Assert.IsFalse(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable1>());

            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).DropTable<SampleTable2>();
            Assert.IsFalse(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable2>());

            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).DropTable<InvalidSampleTable3_NoPrimaryKey>();
            Assert.IsFalse(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<InvalidSampleTable3_NoPrimaryKey>());

            UniversalDataCollector.CleanUp();
            GenericConnectionManager.CloseAllConnections();
            PerformCleanUp();
            Console.WriteLine("========================================= [TEST END] =========================================");
        }

        [Test]
        public void CreateAndDrop_SampleTable1()
        {
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable1>();
            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable1>());

            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).DropTable<SampleTable1>();
            Assert.IsFalse(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable1>());
        }

        [Test]
        public void CreateInsertAndDeleteSingle_SampleTable1()
        {
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable1>();
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable2>();
            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable1>());

            var inserted_entry = new SampleTable1 { Column1 = 1, Column2 = "Sample Data" };
            inserted_entry.Add();

            var fetched_entries = GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<SampleTable1>();
            Assert.IsTrue(fetched_entries.Length == 1);

            var fetched_entry = fetched_entries.First() as SampleTable1;

            Assert.IsTrue(fetched_entry != null);
            Assert.IsTrue(fetched_entry.Column1 == inserted_entry.Column1);
            Assert.IsTrue(fetched_entry.Column2 == inserted_entry.Column2);

            Assert.IsTrue(fetched_entry.Remove());

            fetched_entries = GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<SampleTable1>();
            Assert.IsTrue(fetched_entries.Length == 0);
        }

        [Test]
        public void CreateInsertAndDeleteMultiple1_SampleTable1()
        {
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable1>();
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable2>();
            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable1>());

            for (int i = 0; i < 100; i++)
            {
                var inserted_entry = new SampleTable1 { Column1 = 1, Column2 = "Sample Data" };
                inserted_entry.Add();

                var fetched_entries = GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<SampleTable1>();
                Assert.IsTrue(fetched_entries.Length == 1);

                var fetched_entry = fetched_entries.First() as SampleTable1;

                Assert.IsTrue(fetched_entry != null);
                Assert.IsTrue(fetched_entry.Column1 == inserted_entry.Column1);
                Assert.IsTrue(fetched_entry.Column2 == inserted_entry.Column2);

                Assert.IsTrue(fetched_entry.Remove());

                fetched_entries = GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<SampleTable1>();
                Assert.IsTrue(fetched_entries.Length == 0);
            }
        }

        [Test]
        public void CreateInsertAndDeleteMultiple2_SampleTable1()
        {
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable1>();
            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable1>());

            for (int i = 0; i < 20; i++)
            {
                var inserted_entry = new SampleTable1 { Column1 = i * 10, Column2 = "Sample Data" };
                inserted_entry.Add();

                var fetched_entries = GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<SampleTable1>();
                Assert.IsTrue(fetched_entries.Length == i + 1);
            }

            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).DeleteAll<SampleTable1>());

            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<SampleTable1>().Length == 0);
        }

        [Test]
        public void CreateInsertUpdateAndDeleteMultiple_SampleTable1()
        {
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable1>();
            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable1>());

            for (int i = 0; i < 20; i++)
            {
                var inserted_entry = new SampleTable1 { Column1 = i * 10, Column2 = "Sample Data" };
                inserted_entry.Add();

                var fetched_entries = GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<SampleTable1>(new[] { new Selector("Column1", inserted_entry.Column1) });
                Assert.IsTrue(fetched_entries.Length == 1);

                var fetched_entry = fetched_entries.First() as SampleTable1;
                Assert.IsNotNull(fetched_entry);
                var sample_text = fetched_entry.Column2 = "This is the modified value " + DateTime.Now.Ticks;
                Assert.IsTrue(fetched_entry.Update());

                fetched_entries = GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<SampleTable1>(new[] { new Selector("Column1", inserted_entry.Column1) });
                Assert.IsTrue(fetched_entries.Length == 1);

                var newly_fetched_entry = fetched_entries.First() as SampleTable1;
                Assert.IsNotNull(newly_fetched_entry);
                Assert.AreEqual(newly_fetched_entry.Column1, inserted_entry.Column1);
                Assert.AreEqual(newly_fetched_entry.Column2, sample_text);
            }

            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).DeleteAll<SampleTable1>());

            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<SampleTable1>().Length == 0);
        }

        [Test]
        public void CreateDuplicateInsert_SampleTable1()
        {
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable1>();
            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable1>());

            var inserted_entry = new SampleTable1 { Column1 = 1, Column2 = "Sample Data" };
            inserted_entry.Add();
            try
            {
                var clone_entry = new SampleTable1 { Column1 = 1, Column2 = "Sample Data" };
                clone_entry.Add();
                Assert.Fail("No Error was Thrown !");
            }
            catch (DatabaseException ex)
            {
                if (ex.GetErrorType() != DatabaseException.ErrorType.RecordAlreadyExists &&
                    ex.GetErrorType() != DatabaseException.ErrorType.AlreadyExistingUnqiueField)
                {
                    Assert.Fail("The Correct Exception was Thrown, but not the Correct Error Type !");
                }
            }
            catch (System.Exception)
            {
                Assert.Fail("Invalid Exception was Thrown !");
            }

            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).DeleteAll<SampleTable1>());

            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<SampleTable1>().Length == 0);

            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).DropTable<SampleTable1>();
            Assert.IsFalse(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable1>());
        }

        [Test]
        public void SelectStringGenerationTest_SampleTable1_2()
        {
            var generated_string1 = DatabaseEntity.GetSelectCommandString<SampleTable1>();
            var generated_string2 = DatabaseEntity.GetSelectCommandString<SampleTable2>();

            Assert.AreEqual(generated_string1, "SELECT SampleTable1_1.Column1 as [Column 1], SampleTable1_1.Column2 as [Column 2] FROM SampleTable1 SampleTable1_1");
            Assert.AreEqual(generated_string2, "SELECT SampleTable2_1.Column1 as [Column 1], SampleTable2_1.Column2 as [Column 2], (CASE WHEN SampleTable2_1.Column3 IN ( SELECT Column1 FROM SampleTable1 ) THEN (SELECT SampleTable1.Column2 FROM SampleTable1 WHERE SampleTable2_1.Column3 = SampleTable1.Column1 LIMIT 1) ELSE \"\" END) as [Column 3] FROM SampleTable2 SampleTable2_1");
        }

        [Test]
        public void CheckEqualObjects()
        {
            var entry_1 = new SampleTable1 { Column1 = 1, Column2 = "Sample Data" };
            var entry_2 = new SampleTable1 { Column1 = 1, Column2 = "Sample Data" };

            Assert.IsTrue(entry_1.Equals(entry_2));
        }

        [Test]
        public void ReferenceCheckValidation_SampleTable2()
        {
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable1>();
            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable1>());

            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable2>();
            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable2>());

            var inserted_entry = new SampleTable1 { Column1 = 1, Column2 = "Sample Data" };
            inserted_entry.Add();

            var inserted_sample_table2_entry = new SampleTable2 { Column1 = 10, Column2 = "The Next Set of Data", Column3 = inserted_entry.Column1 };
            inserted_sample_table2_entry.Add();

            Assert.IsTrue(Relationship.CheckReferences(inserted_entry));

            inserted_sample_table2_entry.Remove();

            Assert.IsFalse(Relationship.CheckReferences(inserted_entry));
        }

        [Test]
        public void ReferenceKeyViolationException_SampleTable1_2()
        {
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable1>();
            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable1>());

            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable2>();
            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable2>());

            var inserted_entry = new SampleTable1 { Column1 = 1, Column2 = "Sample Data" };
            inserted_entry.Add();

            var inserted_sample_table2_entry = new SampleTable2 { Column1 = 10, Column2 = "The Next Set of Data", Column3 = inserted_entry.Column1 };
            inserted_sample_table2_entry.Add();

            Assert.IsTrue(Relationship.CheckReferences(inserted_entry));

            try
            {
                inserted_entry.Remove();
                Assert.Fail("No Error was Thrown !");
            }
            catch (DatabaseException ex)
            {
                if (ex.GetErrorType() != DatabaseException.ErrorType.ReferenceKeyViolation)
                {
                    Assert.Fail("The Correct Exception was Thrown, but not the Correct Error Type !");
                }
                else
                {
                    Assert.IsNotNull(ex.GetAdditionalData());
                }
            }
            catch (System.Exception)
            {
                Assert.Fail("Invalid Exception was Thrown !");
            }

            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<SampleTable1>(new[] { new Selector("Column1", inserted_entry.Column1) }).Length == 1);
        }

        [Test]
        public void AttemptActionsOnTableWithNoPrimaryKey_SampleTable3()
        {
            try
            {
                GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<InvalidSampleTable3_NoPrimaryKey>();
                Assert.Fail("No Error was Thrown !");
            }
            catch (System.Exception ex)
            {
                DatabaseException db_exception = null;
                if (ex is DatabaseException)
                {
                    db_exception = ex as DatabaseException;
                }
                else if (ex.InnerException is DatabaseException)
                {
                    db_exception = ex.InnerException as DatabaseException;
                }
                else
                {
                    Assert.Fail("Invalid Exception was Thrown !");
                }

                if (db_exception != null)
                {
                    if (db_exception.GetErrorType() != DatabaseException.ErrorType.NoPrimaryKeyColumnsFound)
                    {
                        Assert.Fail("The Correct Exception was Thrown, but not the Correct Error Type !");
                    }
                }
            }
        }

        [Test]
        public void AttemptOnCreatingTableWithNoColumns_SampleTable4()
        {
            try
            {
                GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<InvalidSampleTable4_NoColumns>();
                Assert.Fail("No Error was Thrown !");
            }
            catch (System.Exception ex)
            {
                DatabaseException db_exception = null;
                if (ex is DatabaseException)
                {
                    db_exception = ex as DatabaseException;
                }
                else if (ex.InnerException is DatabaseException)
                {
                    db_exception = ex.InnerException as DatabaseException;
                }
                else
                {
                    Assert.Fail("Invalid Exception was Thrown !");
                }

                if (db_exception != null)
                {
                    if (db_exception.GetErrorType() != DatabaseException.ErrorType.NoColumnsFound)
                    {
                        Assert.Fail("The Correct Exception was Thrown, but not the Correct Error Type !" + db_exception);
                    }
                }
            }
        }

        [Test]
        public void LoadingItemToDbEntityForm()
        {
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable1>();
            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable1>());

            var inserted_entry = new SampleTable1 { Column1 = 1, Column2 = "Sample Data" };
            var frmSampleTable = DatabaseEntityForm.ShowWindow<frmSampleTable1>(inserted_entry);
            var retrieved_entry = frmSampleTable.GetUpdatedEntity();
            frmSampleTable.Close();

            Assert.IsTrue(inserted_entry.Equals(retrieved_entry));
        }

        [Test]
        public void LoadingItemsToDatabaseEntityViewer()
        {
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable1>();
            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable1>());

            for (int i = 0; i < 20; i++)
            {
                var inserted_entry = new SampleTable1 { Column1 = i * 10, Column2 = "Sample Data" };
                inserted_entry.Add();

                var fetched_entries = GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<SampleTable1>(new[] { new Selector("Column1", inserted_entry.Column1) });
                Assert.IsTrue(fetched_entries.Length == 1);

                var fetched_entry = fetched_entries.First() as SampleTable1;
                Assert.IsNotNull(fetched_entry);
                var sample_text = fetched_entry.Column2 = "This is the modified value " + DateTime.Now.Ticks;
                Assert.IsTrue(fetched_entry.Update());

                fetched_entries = GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<SampleTable1>(new[] { new Selector("Column1", inserted_entry.Column1) });
                Assert.IsTrue(fetched_entries.Length == 1);

                var newly_fetched_entry = fetched_entries.First() as SampleTable1;
                Assert.IsNotNull(newly_fetched_entry);
                Assert.AreEqual(newly_fetched_entry.Column1, inserted_entry.Column1);
                Assert.AreEqual(newly_fetched_entry.Column2, sample_text);
            }

            var form = frmDatabaseEntityViewer.ShowNonModalWindow<SampleTable1>(null);
            form.Close();

            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).DeleteAll<SampleTable1>());

            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<SampleTable1>().Length == 0);
        }

        [Test]
        public void RegisteringEntityTypeOnUniversalDataCollector()
        {
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable1>();
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable2>();

            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable1>());
            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable2>());

            UniversalDataCollector.Register<SampleTable1>();
            UniversalDataCollector.Register<SampleTable2>();

            for (int i = 0; i < 20; i++)
            {
                var inserted_entry1 = new SampleTable1 { Column1 = i * 10, Column2 = "Sample Data" };
                inserted_entry1.Add();

                var inserted_entry2 = new SampleTable2 { Column1 = i * 10, Column2 = "Sample Data" };
                inserted_entry2.Add();
            }

            Assert.IsTrue(UniversalDataCollector.Select<SampleTable1>().Count == 20);
            Assert.IsTrue(UniversalDataCollector.Select<SampleTable2>().Count == 20);
        }

        [Test]
        public void UniversalDataCollectorDataModelUpdateCheck()
        {
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable1>();
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable2>();

            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable1>());
            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable2>());

            UniversalDataCollector.Register<SampleTable1>();
            UniversalDataCollector.Register<SampleTable2>();

            for (int i = 0; i < 20; i++)
            {
                var inserted_entry1 = new SampleTable1 { Column1 = i * 10, Column2 = "Sample Data" };
                inserted_entry1.Add();

                var inserted_entry2 = new SampleTable2 { Column1 = i * 10, Column2 = "Sample Data" };
                inserted_entry2.Add();
            }

            Assert.IsTrue(UniversalDataCollector.Select<SampleTable1>().Count == 20);
            Assert.IsTrue(UniversalDataCollector.Select<SampleTable2>().Count == 20);

            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).DeleteMatching<SampleTable1>(new[] { new Selector("Column1", 80, Selector.Operator.LessThan) });

            Assert.IsTrue(UniversalDataCollector.Select<SampleTable2>().Count == 20);
            var other_results = UniversalDataCollector.Select<SampleTable1>();
            Assert.IsTrue(other_results.Count == 12);

            var second_entry = other_results[1] as SampleTable1;

            Assert.IsTrue(other_results.First().Remove());

            Assert.IsTrue(UniversalDataCollector.Select<SampleTable1>().Count == 11);
            Assert.IsTrue(UniversalDataCollector.Select<SampleTable2>().Count == 20);

            second_entry.Column2 = "Does this work";
            Assert.IsTrue(second_entry.Update());

            Assert.IsTrue(UniversalDataCollector.Select<SampleTable1>().Count == 11);
            Assert.IsTrue(UniversalDataCollector.Select<SampleTable2>().Count == 20);

            var grabbed_entry = UniversalDataCollector.Select<SampleTable1>(new []{new Selector("Column1", 90)})[0] as SampleTable1;

            Assert.IsTrue(second_entry.Equals(grabbed_entry));

            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).DeleteAll<SampleTable1>();
            Assert.IsTrue(UniversalDataCollector.Select<SampleTable1>().Count == 0);

            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).DeleteAll<SampleTable2>();
            Assert.IsTrue(UniversalDataCollector.Select<SampleTable2>().Count == 0);
        }

        [Test]
        public void HavingTwoDifferentDatabasesForASingleDatabaseType()
        {
            var databaseFile1 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\libDBHelderSampleFolder1\\TestDatabase1.sdf";
            var databaseFile2 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\libDBHelderSampleFolder1\\TestDatabase2.sdf";
            try
            {
                if (File.Exists(databaseFile1))
                {
                    File.Delete(databaseFile1);
                }

                if (File.Exists(databaseFile2))
                {
                    File.Delete(databaseFile2);
                }
            }
            catch (System.Exception) { }

            GenericConnectionManager.GetConnectionManager(DatabaseType.SqlCE).SetConnectionString<Database1_NormalTable1>("Data Source=" + databaseFile1 + ";Persist Security Info=False;");
            GenericConnectionManager.GetConnectionManager(DatabaseType.SqlCE).SetConnectionString<Database2_NormalTable2>("Data Source=" + databaseFile2 + ";Persist Security Info=False;");

            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<Database1_NormalTable1>();
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<Database2_NormalTable2>();

            var inserted_entry1 = new Database1_NormalTable1 { Column1 = 1, Column2 = "Sample Data" };
            var inserted_entry2 = new Database2_NormalTable2 { Column1 = 1, Column2 = "Sample Data" };

            inserted_entry1.Add();
            inserted_entry2.Add();

            var fetched_entries1 = GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<Database1_NormalTable1>();
            var fetched_entries2 = GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<Database2_NormalTable2>();
            Assert.IsTrue(fetched_entries1.Length == 1);
            Assert.IsTrue(fetched_entries2.Length == 1);

            var fetched_entry1 = fetched_entries1.First() as Database1_NormalTable1;
            var fetched_entry2 = fetched_entries2.First() as Database2_NormalTable2;

            Assert.IsTrue(fetched_entry1 != null);
            Assert.IsTrue(fetched_entry1.Column1 == inserted_entry1.Column1);
            Assert.IsTrue(fetched_entry1.Column2 == inserted_entry1.Column2);

            Assert.IsTrue(fetched_entry2 != null);
            Assert.IsTrue(fetched_entry2.Column1 == inserted_entry2.Column1);
            Assert.IsTrue(fetched_entry2.Column2 == inserted_entry2.Column2);

            Assert.IsTrue(fetched_entry1.Remove());
            Assert.IsTrue(fetched_entry2.Remove());

            fetched_entries1 = GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<Database1_NormalTable1>();
            Assert.IsTrue(fetched_entries1.Length == 0);

            fetched_entries2 = GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<Database2_NormalTable2>();
            Assert.IsTrue(fetched_entries2.Length == 0);
        }

        [Test]
        public void TestingDataTableLoading()
        { 
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable1>();
            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable1>());

            for (int i = 0; i < 20; i++)
            {
                var inserted_entry = new SampleTable1 { Column1 = i * 10, Column2 = "Sample Data" };
                inserted_entry.Add();
            }

            DataTable dtblMain = new DataTable();
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).FillDataTable<SampleTable1>(ref dtblMain, new []{new Selector("Column1", 100, Selector.Operator.LessThan)});

            Assert.IsTrue(dtblMain.Rows.Count == 10);
        }

        [Test]
        public void LoadingSaveConnectionData()
        {
            GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<SampleTable1>();
            Assert.IsTrue(GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).TableExist<SampleTable1>());

            GenericConnectionManager.RegisterConnectionManager<ConnectionManager>();
            GenericDatabaseManager.RegisterDatabaseManager<DatabaseManager>(true);

            var connectionString = GenericConnectionManager.GetConnectionManager(DatabaseType.SqlCE).GetConnectionString<SampleTable1>();
            var connectionData = GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<GenericConnectionDetails>(new Selector[] { new Selector("TypeName", "libDatabaseHelper.classes.generic.NullType") }).FirstOrDefault() as GenericConnectionDetails;
            var expectedConnectionString = "Data Source=" + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\libDBHelderSampleFolder1\\SampleDatabase1.sdf;Persist Security Info=False;";

            Assert.IsTrue(connectionString == expectedConnectionString);
            Assert.IsTrue(connectionString == connectionData.ConnectionString);
        }

        [Test]
        public void NonProperImplementationOfConnectionManager()
        {
            var connectionManager = new GenericConnectionManager(DatabaseType.SqlCE);
            var exception1 = Assert.Throws<DatabaseConnectionException>(() => connectionManager.GetConnectionString());
            Assert.IsTrue(exception1.GetErrorType() == DatabaseConnectionException.ConnectionErrorType.NoConnectionStringFound);

            Assert.Throws<NotImplementedException>(() => connectionManager.SetConnectionString(""));
            Assert.Throws<NotImplementedException>(() => connectionManager.CheckConnectionString(""));

            var exception2 = Assert.Throws<DatabaseConnectionException>(() => GenericConnectionManager.GetConnectionManager(DatabaseType.Generic));
            Assert.IsTrue(exception2.GetErrorType() == DatabaseConnectionException.ConnectionErrorType.NoConnectionManagerFound);

            var nonExistantDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\sampleDir_" + DateTime.Now.Ticks;
            Assert.Throws<DirectoryNotFoundException>(() => GenericConnectionManager.SetLocalDataDirectory(nonExistantDir));

            try
            {
                Directory.CreateDirectory(nonExistantDir);
            }
            catch { Assert.Fail("Unable to create directory : " + nonExistantDir); }

            Assert.DoesNotThrow(() => GenericConnectionManager.SetLocalDataDirectory(nonExistantDir));

            try
            {
                Directory.CreateDirectory(nonExistantDir);
            }
            catch { Console.WriteLine("Unable to delete directory : " + nonExistantDir); }
        }

        [Test]
        public void NonProperlyImplementedDatabaseEntity()
        { 
            
        }
    }
}