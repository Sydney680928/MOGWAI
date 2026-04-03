// Copyright 2026 Stéphane Sibué
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

using MOGWAI.Engine;

namespace MOGWAI.Tests
{
    public abstract class MogwaiTestBase
    {
        protected MogwaiEngine CreateEngine(out TestDelegate testDelegate)
        {
            testDelegate = new TestDelegate();
            var engine = new MogwaiEngine("Test", keepAlive: false, useDefaultFolders: false);
            engine.Delegate = testDelegate;
            return engine;
        }
    }
}
