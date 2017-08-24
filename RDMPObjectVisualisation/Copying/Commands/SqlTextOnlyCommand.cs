﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReusableUIComponents.Copying;

namespace RDMPObjectVisualisation.Copying.Commands
{
    public class SqlTextOnlyCommand:ICommand
    {
        private readonly string _sql;

        public SqlTextOnlyCommand(string sql)
        {
            _sql = sql;
        }
        public string GetSqlString()
        {
            return _sql;
        }
    }
}
