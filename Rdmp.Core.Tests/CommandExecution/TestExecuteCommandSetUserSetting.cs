﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rdmp.Core.Tests.CommandExecution
{
    class TestExecuteCommandSetUserSetting : CommandCliTests
    {
        [Test]
        public void Test_CatalogueDescription_Normal()
        {
            UserSettings.Wait5SecondsAfterStartupUI = false;

            GetInvoker().ExecuteCommand(typeof(ExecuteCommandSetUserSetting),new CommandLineObjectPicker(new []{ "Wait5SecondsAfterStartupUI", "true"}, GetActivator()));

            Assert.IsTrue(UserSettings.Wait5SecondsAfterStartupUI);

            GetInvoker().ExecuteCommand(typeof(ExecuteCommandSetUserSetting),new CommandLineObjectPicker(new []{ "Wait5SecondsAfterStartupUI", "false"}, GetActivator()));
            
            Assert.IsFalse(UserSettings.Wait5SecondsAfterStartupUI);

        }

        [Test]
        public void TestSettingErrorCodeValue_InvalidValue()
        {
            var cmd = new ExecuteCommandSetUserSetting(GetActivator(), "R001", "foo");
            Assert.IsTrue(cmd.IsImpossible);
            Assert.AreEqual(cmd.ReasonCommandImpossible, "Invalid enum value.  When setting an error code you must supply a value of one of :Success,Warning,Fail");
        }

        [Test]
        public void TestSettingErrorCodeValue_Success()
        {
            Assert.AreEqual("R001", ErrorCodes.ExistingExtractionTableInDatabase.Code);
            var before = UserSettings.GetErrorReportingLevelFor(ErrorCodes.ExistingExtractionTableInDatabase);
            Assert.AreNotEqual(CheckResult.Success, before);

            var cmd = new ExecuteCommandSetUserSetting(GetActivator(), "R001", "Success");
            Assert.IsFalse(cmd.IsImpossible,cmd.ReasonCommandImpossible);
            cmd.Execute();

            var after = UserSettings.GetErrorReportingLevelFor(ErrorCodes.ExistingExtractionTableInDatabase);
            Assert.AreEqual(CheckResult.Success, after);

            //reset the original state of the system (the default)
            UserSettings.SetErrorReportingLevelFor(ErrorCodes.ExistingExtractionTableInDatabase,before);
        }
    }
}
