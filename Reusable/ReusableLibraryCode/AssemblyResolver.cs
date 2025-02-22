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
using System.Runtime.Loader;

namespace ReusableLibraryCode
{
    /// <summary>
    /// Create a resolver for when assemblies cannot be properly loaded through the usual mechanism
    /// and the bidingredirect is not available.
    /// </summary>
    public class AssemblyResolver
    {
        private static Dictionary<string,Assembly> assemblyResolveAttempts = new Dictionary<string, Assembly>(); 

        public static void SetupAssemblyResolver(params DirectoryInfo[] dirs)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, resolveArgs) =>
            {
                string assemblyInfo = resolveArgs.Name;
                var parts = assemblyInfo.Split(',');
                string name = parts[0];

                if (assemblyResolveAttempts.ContainsKey(assemblyInfo))
                    return assemblyResolveAttempts[assemblyInfo];

                //start out assuming we cannot load it
                assemblyResolveAttempts.Add(assemblyInfo,null);
                
                foreach(DirectoryInfo dir in dirs)
                {
                    var dll = dir.EnumerateFiles(name + ".dll").SingleOrDefault();
                    if(dll != null)
                        return assemblyResolveAttempts[assemblyInfo] = LoadFile(dll); //cache and return answer
                }
                
                var assembly = System.AppContext.BaseDirectory;
                if (string.IsNullOrWhiteSpace(assembly))
                    return null;

                var directoryInfo = new DirectoryInfo(assembly);
                var file = directoryInfo?.EnumerateFiles(name + ".dll").FirstOrDefault();
                if (file == null)
                    return null;

                return assemblyResolveAttempts[assemblyInfo] = Assembly.LoadFile(file.FullName); //cache and return answer
            };
        } 
 
        public static Assembly LoadFile(FileInfo f)
        {
            try
            {
                return F1(f);

            }catch(FileLoadException)
            {
                //AssemblyLoadContext.Default.LoadFromAssemblyPath causes this Exception at runtime only
                return F2(f);
            }
        }

        private static Assembly F2(FileInfo f)
        {
            return Assembly.LoadFile(f.FullName);
        }

        private static Assembly F1(FileInfo f)
        {
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(f.FullName);
        }

    }
}