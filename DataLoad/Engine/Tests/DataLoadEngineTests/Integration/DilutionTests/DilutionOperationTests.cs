﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using LoadModules.Generic.Mutilators.Dilution.Operations;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Rhino.Mocks;
using Tests.Common;

namespace DataLoadEngineTests.Integration.DilutionTests
{
    public class DilutionOperationTests:DatabaseTests
    {
        [TestCase("2001-01-03", "2001-02-15")]
        [TestCase("2001-03-31", "2001-02-15")]
        [TestCase("2001-04-01", "2001-05-15")]
        [TestCase("2001-03-31 23:59:59", "2001-02-15")]
        [TestCase("2001-04-01 01:15:00", "2001-05-15")]
        [TestCase(null, null)]
        public void TestRoundDateToMiddleOfQuarter(string input, string expectedDilute)
        {
            var col = MockRepository.GenerateMock<IPreLoadDiscardedColumn>();
            
            var tbl = MockRepository.GenerateStrictMock<ITableInfo>();
            tbl.Expect(m => m.GetRuntimeName(LoadStage.AdjustStaging)).Return("DateRoundingTests").Repeat.Once();
            
            col.Stub(p => p.TableInfo).Return(tbl);
            col.Stub(m => m.GetRuntimeName()).Return("TestField");

            var o = new RoundDateToMiddleOfQuarter();
            o.ColumnToDilute = col;
            var sql = o.GetMutilationSql();

            var server = DiscoveredDatabaseICanCreateRandomTablesIn.Server;
            using (var con = server.BeginNewTransactedConnection())
            {
                try
                {
                    string insert = input != null ? "'" + input + "'" : "NULL";

                    server.GetCommand(@"CREATE TABLE DateRoundingTests(TestField datetime)
INSERT INTO DateRoundingTests VALUES (" + insert + ")", con).ExecuteNonQuery();

                    UsefulStuff.ExecuteBatchNonQuery(sql, con.Connection, con.Transaction);

                    var result = server.GetCommand("SELECT * from DateRoundingTests", con).ExecuteScalar();

                    if (expectedDilute == null)
                        Assert.AreEqual(DBNull.Value, result);
                    else
                        Assert.AreEqual(DateTime.Parse(expectedDilute), result);
                }
                finally  
                {
                    con.ManagedTransaction.AbandonAndCloseConnection();
                }
            }

            tbl.VerifyAllExpectations();
        }

        [TestCase("DD3 9TA", "DD3")]
        [TestCase("DD03 9TA", "DD03")]
        [TestCase("EC4V 2AU", "EC4V")] //London postcodes have extra digits
        [TestCase("EC4V", "EC4V")] //Already is a prefix
        [TestCase("DD3", "DD3")] //Already is a prefix
        [TestCase("DD3_5L1", "DD3")] //Makey upey suffix
        [TestCase("DD3_XXX", "DD3")] //Makey upey suffix
        [TestCase("!D!D!3!9TA!", "DD3")] //Random garbage
        [TestCase("EC4V_2AU", "EC4V")] //underscore instead of space
        [TestCase("EC4V2AU   ", "EC4V")] //Trailing whitespace
        [TestCase("??", "??")] //It's short and it's complete garbage but this is the kind of thing research datasets have :)
        [TestCase("???????", "????")] //Return type is varchar(4) so while we reject the original value we still end up truncating it
        [TestCase("I<3Coffee Yay", "I3Co")] //What can you do?!, got to return varchar(4)
        [TestCase("D3 9T", "D39T")]//39T isn't a valid suffix and the remainder (D) wouldn't be enough for a postcode prefix anyway so just return the original input minus dodgy characters
        [TestCase("G    9TA", "G")]//9TA is the correct suffix pattern (Numeric Alpha Alpha) so can be chopped off and the remainder returned (G)
        [TestCase("DD3 9T", "DD")] //Expected to get it wrong because the suffix check sees 39T but the remainder is long enough to make a legit postcode (2).  We are currently deciding not to evaluate spaces/other dodgy characters when attempting to resolve postcodes
        [TestCase(null,null)]
        public void TestExcludeRight3OfUKPostcodes(string input, string expectedDilute)
        {
            var col = MockRepository.GenerateMock<IPreLoadDiscardedColumn>();

            var tbl = MockRepository.GenerateStrictMock<ITableInfo>();
            tbl.Expect(m => m.GetRuntimeName(LoadStage.AdjustStaging)).Return("ExcludeRight3OfPostcodes").Repeat.Once();

            col.Stub(p => p.TableInfo).Return(tbl);
            col.Stub(m => m.GetRuntimeName()).Return("TestField");

            var o = new ExcludeRight3OfUKPostcodes();
            o.ColumnToDilute = col;
            var sql = o.GetMutilationSql();

            var server = DiscoveredDatabaseICanCreateRandomTablesIn.Server;
            using (var con = server.BeginNewTransactedConnection())
            {
                try
                {
                    string insert = input != null ? "'" + input + "'" : "NULL";

                    server.GetCommand(@"CREATE TABLE ExcludeRight3OfPostcodes(TestField varchar(15))
    INSERT INTO ExcludeRight3OfPostcodes VALUES (" + insert + ")", con).ExecuteNonQuery();

                    UsefulStuff.ExecuteBatchNonQuery(sql, con.Connection, con.Transaction);

                    var result = server.GetCommand("SELECT * from ExcludeRight3OfPostcodes", con).ExecuteScalar();
             
                    if(expectedDilute == null)
                        Assert.AreEqual(DBNull.Value, result);
                    else
                        Assert.AreEqual(expectedDilute, result);
                }
                finally
                {
                    con.ManagedTransaction.AbandonAndCloseConnection();
                }
                
            }

            tbl.VerifyAllExpectations();
        }

        [TestCase("2001-01-03","datetime", true)]
        [TestCase("2001-01-03", "varchar(50)", true)]
        [TestCase(null,"varchar(50)", false)]
        [TestCase(null, "bit", false)]
        [TestCase("1", "bit", true)]
        [TestCase("0", "bit", true)]
        [TestCase("","varchar(1)", true)]//This data exists regardless of if it is blank so it still gets the 1
        public void DiluteToBitFlag(string input,string inputDataType, bool expectedDilute)
        {
            var col = MockRepository.GenerateMock<IPreLoadDiscardedColumn>();

            var tbl = MockRepository.GenerateStrictMock<ITableInfo>();
            tbl.Expect(m => m.GetRuntimeName(LoadStage.AdjustStaging)).Return("DiluteToBitFlagTests").Repeat.Once();

            col.Stub(p => p.TableInfo).Return(tbl);
            col.Stub(m => m.GetRuntimeName()).Return("TestField");

            var o = new CrushToBitFlag();
            o.ColumnToDilute = col;
            var sql = o.GetMutilationSql();

            var server = DiscoveredDatabaseICanCreateRandomTablesIn.Server;
            using (var con = server.BeginNewTransactedConnection())
            {
                try
                {
                    string insert = input != null ? "'" + input + "'" : "NULL";

                    server.GetCommand(@"CREATE TABLE DiluteToBitFlagTests(TestField "+inputDataType+@")
INSERT INTO DiluteToBitFlagTests VALUES (" + insert + ")", con).ExecuteNonQuery();

                    UsefulStuff.ExecuteBatchNonQuery(sql, con.Connection, con.Transaction);

                    var result = server.GetCommand("SELECT * from DiluteToBitFlagTests", con).ExecuteScalar();
                    
                    Assert.AreEqual(expectedDilute, Convert.ToBoolean(result));
                }
                finally
                {
                    con.ManagedTransaction.AbandonAndCloseConnection();
                }
            }

            tbl.VerifyAllExpectations();
        }
    }
}
