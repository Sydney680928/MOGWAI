// Copyright 2015-2026 Stéphane Sibué
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Diagnostics;

namespace MOGWAI_RUNTIME.Classes
{
    internal class AppGlobal
    {
        public static string RootFolder => FileSystem.Current.AppDataDirectory;

        public static string CodeFolder => Path.Combine(RootFolder, "Code");

        public static string DataFolder => Path.Combine(RootFolder, "Data");

        public static bool CreateDataStructure()
        {
            Debug.WriteLine($"AppDataDirectory = {FileSystem.Current.AppDataDirectory}");

            try
            {
                Directory.CreateDirectory(RootFolder);
                Directory.CreateDirectory(CodeFolder);
                Directory.CreateDirectory(DataFolder);

                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
