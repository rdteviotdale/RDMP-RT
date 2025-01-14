// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace ReusableLibraryCode.Checks
{
    /// <summary>
    /// ICheckNotifier which ignores all check messages completely (including failures) but responds true to any ProposedFixes.  Use this ICheckNotifier
    /// when you want to run the Check method on an ICheckable but don't care whether it passes or not.
    /// </summary>
    public class IgnoreAllErrorsCheckNotifier : ICheckNotifier
    {
        public bool OnCheckPerformed(CheckEventArgs args)
        {
            return true;
        }
    }
}