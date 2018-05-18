﻿using System.Data.SqlClient;
using CatalogueLibrary.Triggers;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    internal class HousekeepingTests : DatabaseTests
    {
        [Test]
        public void TestCheckUpdateTrigger()
        {
            
            // set up a test database
            const string tableName = "TestTable";

            var databaseName = DiscoveredDatabaseICanCreateRandomTablesIn.GetRuntimeName();
            var table = DiscoveredDatabaseICanCreateRandomTablesIn.CreateTable(tableName,new[] {new DatabaseColumnRequest("Id", "int"),});

            var server = DiscoveredDatabaseICanCreateRandomTablesIn.Server;
            using (var con = server.GetConnection())
            {
                con.Open();
                var cmd = server.GetCommand(
                    "CREATE TRIGGER dbo.[TestTable_OnUpdate] ON [dbo].[" + tableName +
                    "] AFTER DELETE AS RAISERROR('MESSAGE',16,10)", con);

                cmd.ExecuteNonQuery();
            }

            var dbInfo = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(databaseName);

            var triggerImplementer = new TriggerImplementer(dbInfo, tableName);
            var isEnabled = triggerImplementer.CheckUpdateTriggerIsEnabledOnServer();
            Assert.AreEqual(TriggerImplementer.TriggerStatus.Enabled, isEnabled);

            
            // disable the trigger and test correct reporting
            using (var con = new SqlConnection(dbInfo.Server.Builder.ConnectionString))
            {
                con.Open();
                var cmd =
                    new SqlCommand(
                        "USE [" + databaseName + "]; DISABLE TRIGGER TestTable_OnUpdate ON [" + databaseName + "]..[" +
                        tableName + "]", con);
                cmd.ExecuteNonQuery();
            }

            isEnabled = triggerImplementer.CheckUpdateTriggerIsEnabledOnServer();
            Assert.AreEqual(TriggerImplementer.TriggerStatus.Disabled, isEnabled);
        }
    }
}

