// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Exceptions;
using ReusableLibraryCode.Settings;

namespace Rdmp.Core.DataLoad.Triggers.Implementations
{
    /// <inheritdoc/>
    internal class MySqlTriggerImplementer:TriggerImplementer
    {
        /// <inheritdoc cref="TriggerImplementer(DiscoveredTable,bool)"/>
        public MySqlTriggerImplementer(DiscoveredTable table, bool createDataLoadRunIDAlso = true) : base(table, createDataLoadRunIDAlso)
        {
        }

        public override void DropTrigger(out string problemsDroppingTrigger, out string thingsThatWorkedDroppingTrigger)
        {
            problemsDroppingTrigger = "";
            thingsThatWorkedDroppingTrigger = "";

            try
            {
                using (var con = _server.GetConnection())
                {
                    con.Open();

                    using(var cmd = _server.GetCommand("DROP TRIGGER " + GetTriggerName(), con))
                    {
                        cmd.CommandTimeout = UserSettings.ArchiveTriggerTimeout;
                        cmd.ExecuteNonQuery();
                    }

                    thingsThatWorkedDroppingTrigger = "Droppped trigger " + GetTriggerName();
                }
            }
            catch (Exception exception)
            {
                //this is not a problem really since it is likely that DLE chose to recreate the trigger because it was FUBARed or missing, this is just belt and braces try and drop anything that is lingering, whether or not it is there
                problemsDroppingTrigger += "Failed to drop Trigger:" + exception.Message + Environment.NewLine;
            }
        }

        public override string CreateTrigger(ICheckNotifier notifier)
        {
            string creationSql = base.CreateTrigger(notifier);

            var sql = string.Format(@"CREATE TRIGGER {0} BEFORE UPDATE ON {1} FOR EACH ROW
{2};", 
       GetTriggerName(),
       _table.GetFullyQualifiedName(),
       CreateTriggerBody());

            using (var con = _server.GetConnection())
            {
                con.Open();

                using(var cmd = _server.GetCommand(sql, con))
                {
                    cmd.CommandTimeout = UserSettings.ArchiveTriggerTimeout;
                    cmd.ExecuteNonQuery();
                }                    
            }

            return creationSql;
        }

        protected override void AddValidFrom(DiscoveredTable table, IQuerySyntaxHelper syntaxHelper)
        {
            // MySql changed how they do default date fields between 5.5 and 5.6
            //https://dba.stackexchange.com/a/132954
 
            if (UseOldDateTimeDefaultMethod(table))
                table.AddColumn(SpecialFieldNames.ValidFrom,"TIMESTAMP DEFAULT CURRENT_TIMESTAMP",true, UserSettings.ArchiveTriggerTimeout);
            else
                table.AddColumn(SpecialFieldNames.ValidFrom,"DATETIME DEFAULT CURRENT_TIMESTAMP",true, UserSettings.ArchiveTriggerTimeout);
        }

        public bool UseOldDateTimeDefaultMethod(DiscoveredTable table)
        {
            using (var con = table.Database.Server.GetConnection())
            {
                con.Open();
                return UseOldDateTimeDefaultMethod(table.GetCommand("SELECT VERSION()", con).ExecuteScalar()?.ToString());
            }
                
        }

        public static bool UseOldDateTimeDefaultMethod(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return false;
            
            var match = Regex.Match(version,@"(\d+)\.(\d+)");

            //If the version string doesn't start with numbers we have bigger problems than creating a default constraint
            if (!match.Success)
                return false;

            var major = int.Parse(match.Groups[1].Value);
            var minor = int.Parse(match.Groups[2].Value);

            return major < 5 || (major == 5 && minor <= 5);
        }

        protected virtual string CreateTriggerBody()
        {
            var syntax = _server.GetQuerySyntaxHelper();
            
            return string.Format(@"BEGIN
    INSERT INTO {0} SET {1},hic_validTo=now(),hic_userID=CURRENT_USER(),hic_status='U';

	SET NEW.{2} = now();
  END", _archiveTable.GetFullyQualifiedName(),
                string.Join(",", _columns.Select(c =>syntax.EnsureWrapped( c.GetRuntimeName()) + "=OLD." + syntax.EnsureWrapped(c.GetRuntimeName()))),
                syntax.EnsureWrapped(SpecialFieldNames.ValidFrom));
        }
        
        public override TriggerStatus GetTriggerStatus()
        {
            return string.IsNullOrWhiteSpace(GetTriggerBody())? TriggerStatus.Missing : TriggerStatus.Enabled;
        }

        protected virtual string GetTriggerBody()
        {
            using (var con = _server.GetConnection())
            {
                con.Open();

                using(var cmd = _server.GetCommand(string.Format("show triggers like '{0}'", _table.GetRuntimeName()), con))
                    using(var r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            if (r["Trigger"].Equals(GetTriggerName()))
                                return (string) r["Statement"];
                        }
            }

            return null;
        }

        protected virtual object GetTriggerName()
        {
            return QuerySyntaxHelper.MakeHeaderNameSensible(_table.GetRuntimeName()) + "_onupdate";
        }

        public override bool CheckUpdateTriggerIsEnabledAndHasExpectedBody()
        {
            if (!base.CheckUpdateTriggerIsEnabledAndHasExpectedBody())
                return false;

            var sqlThen = GetTriggerBody();
            var sqlNow = CreateTriggerBody();

            AssertTriggerBodiesAreEqual(sqlThen,sqlNow);
            
            return true;
        }

        protected virtual void AssertTriggerBodiesAreEqual(string sqlThen, string sqlNow)
        {
            if(!sqlNow.Equals(sqlThen))
                throw new ExpectedIdenticalStringsException("Sql body for trigger doesn't match expcted sql",sqlNow,sqlThen);
        }
    }
}
