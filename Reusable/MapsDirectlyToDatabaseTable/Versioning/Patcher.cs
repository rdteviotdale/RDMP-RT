// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FAnsi;
using FAnsi.Discovery;

namespace MapsDirectlyToDatabaseTable.Versioning
{
    /// <inheritdoc/>
    public abstract class Patcher:IPatcher
    {
        public const string InitialScriptName = "Initial Create";

        /// <inheritdoc/>
        public bool SqlServerOnly { get; protected set; }= true;

        /// <inheritdoc/>
        public virtual Assembly GetDbAssembly()
        {
            return GetType().Assembly;
        }

        /// <inheritdoc/>
        public string ResourceSubdirectory { get; private set; }

        /// <inheritdoc/>
        public int Tier { get; }

        public string Name => GetDbAssembly().GetName().Name + (string.IsNullOrEmpty(ResourceSubdirectory) ? "" : "/" + ResourceSubdirectory);
        public string LegacyName { get; protected set; }

        protected Patcher(int tier,string resourceSubdirectory)
        {
            Tier = tier;
            ResourceSubdirectory = resourceSubdirectory;
        }

        /// <summary>
        /// Generates a properly formatted header for <see cref="Patch"/> creation when you only know the SQL you want to execute
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="description"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        protected string GetHeader(DatabaseType dbType, string description,Version version)
        {
            string header = CommentFor(dbType,Patch.VersionKey + version.ToString()) + Environment.NewLine;
            header += CommentFor(dbType,Patch.DescriptionKey + description) + Environment.NewLine;

            return header;
        }

        private string CommentFor(DatabaseType dbType, string sql)
        {
            if (dbType == DatabaseType.MicrosoftSQLServer)
                return sql;

            // some DBMS don't like the -- notation so we need to wrap with C style comments
            return $"/*{sql}*/";
        }

        public virtual Patch GetInitialCreateScriptContents(DiscoveredDatabase db)
        {
            var assembly = GetDbAssembly();
            var subdirectory = ResourceSubdirectory;
            Regex initialCreationRegex;

            if (string.IsNullOrWhiteSpace(subdirectory))
                initialCreationRegex = new Regex(@".*\.runAfterCreateDatabase\..*\.sql");
            else
                initialCreationRegex = new Regex(@".*\." + Regex.Escape(subdirectory) + @"\.runAfterCreateDatabase\..*\.sql");

            var candidates = assembly.GetManifestResourceNames().Where(r => initialCreationRegex.IsMatch(r)).ToArray();
            
            if (candidates.Length == 1)
            {
                var sr = new StreamReader(assembly.GetManifestResourceStream(candidates[0]));

                string sql = sr.ReadToEnd();

                if (!sql.Contains(Patch.VersionKey))
                    sql = GetHeader(db.Server.DatabaseType, InitialScriptName, new Version(1, 0, 0)) + sql;


                return new Patch(InitialScriptName,  sql);
            }

            if (candidates.Length == 0)
                throw new FileNotFoundException("Could not find an initial create database script in dll " + assembly.FullName + ".  Make sure it is marked as an Embedded Resource and that it is in a folder called 'runAfterCreateDatabase' (and matches regex " + initialCreationRegex + "). And make sure that it is marked as 'Embedded Resource' in the .csproj build action");

            throw new Exception("There are too many create scripts in the assembly " + assembly.FullName + " only 1 create database script is allowed, all other scripts must go into the up folder");

        }

        /// <inheritdoc/>
        public virtual SortedDictionary<string, Patch> GetAllPatchesInAssembly(DiscoveredDatabase db)
        {
            var assembly = GetDbAssembly();
            var subdirectory = ResourceSubdirectory;
            Regex upgradePatchesRegexPattern;

            if (string.IsNullOrWhiteSpace(subdirectory))
                upgradePatchesRegexPattern = new Regex(@".*\.up\.(.*\.sql)");
            else
                upgradePatchesRegexPattern = new Regex(@".*\." + Regex.Escape(subdirectory) + @"\.up\.(.*\.sql)");

            var files = new SortedDictionary<string, Patch>();

            //get all resources out of 
            foreach (string manifestResourceName in assembly.GetManifestResourceNames())
            {
                var match = upgradePatchesRegexPattern.Match(manifestResourceName);
                if (match.Success)
                {
                    string fileContents = new StreamReader(assembly.GetManifestResourceStream(manifestResourceName)).ReadToEnd();
                    files.Add(match.Groups[1].Value, new Patch(match.Groups[1].Value, fileContents));
                }
            }

            return files;
        }
    }

}