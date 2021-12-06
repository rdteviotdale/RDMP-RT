// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.Rules
{
    class NotNullRule<T> : BinderRule<T> where T : IMapsDirectlyToDatabaseTable
    {
        public NotNullRule(IActivateItems activator, T databaseObject, Func<T, object> getter, Control control,string propertyToCheckName) : base(activator,databaseObject,getter,control, propertyToCheckName)
        {
            
        }

        protected override string IsValid(object currentValue, Type typeToTest)
        {
            if (currentValue == null || string.IsNullOrWhiteSpace(currentValue.ToString()))
                return "Value cannot be null";

            return null;
        }
    }
}