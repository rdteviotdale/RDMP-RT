﻿using System;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.FilterImporting;
using CatalogueLibrary.Spontaneous;
using MapsDirectlyToDatabaseTable;
using ReusableUIComponents;

namespace CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs.Options
{
    /// <summary>
    /// Handles renaming a parameter in the WHERE SQL of it's parent (if it has one).  Use this when you want the user to be able to change the name of a parameter and for this
    /// to be carried through to the parent without having any knowledge available to what that parent is or even if it has one
    /// </summary>
    public class ParameterRefactorer :IParameterRefactorer
    {
        public HashSet<IFilter> RefactoredFilters { get; private set; }

        private YesNoYesToAllDialog _yesNoToAll;
        
        public ParameterRefactorer()
        {
            _yesNoToAll = new YesNoYesToAllDialog();
            RefactoredFilters = new HashSet<IFilter>();
        }

        public bool HandleRename(ISqlParameter parameter, string oldName, string newName)
        {
            if(string.IsNullOrWhiteSpace(newName))
                return false;

            if(string.IsNullOrWhiteSpace(oldName))
                return false;

            //they are the same name!
            if (oldName.Equals(newName))
                return false;

            if(!parameter.ParameterName.Equals(newName ))
                throw new ArgumentException("Expected parameter " + parameter + " to have name '" + newName + "' but it's value was " + parameter.ParameterName + ", this means someone was lying about the rename event");

            var owner = parameter.GetOwnerIfAny();

            var filter = owner as IFilter;

            if(filter == null || filter is SpontaneousObject)
                return false;

            //There is no WHERE SQL anyway
            if (string.IsNullOrWhiteSpace(filter.WhereSQL))
                return false;

            if (_yesNoToAll.ShowDialog("Would you like to rename Parameter " + oldName + " to " + newName + " in Filter " + filter + "?",
                    "Rename parameter") == DialogResult.Yes)
            {
                string before = filter.WhereSQL;
                string after = ParameterCreator.RenameParameterInSQL(before,oldName,newName);

                //no change was actually made
                if (before.Equals(after))
                    return false;

                filter.WhereSQL = after;
                filter.SaveToDatabase();

                if (!RefactoredFilters.Contains(filter))
                    RefactoredFilters.Add(filter);

                return true;
            }

            return false;
        }

    }
}