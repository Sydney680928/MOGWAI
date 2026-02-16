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

using MOGWAI.Objects;

namespace MOGWAI.Engine
{
    public class MogwaiExecutionContext
    {
        public string CodeFilename { get; set; }

        public string Code { get; set; }

        public MOGFunction? Function { get; set; }

        public int Hash { get; set; }

        public bool AllowDebugMode { get; set; }

        public MogwaiExecutionContext(string codeFilename, string code, int hash, bool allowDebugMode = true)
        {
            CodeFilename = codeFilename;
            Code = code;
            Hash = hash;
            AllowDebugMode = allowDebugMode;
        }
    }
}
