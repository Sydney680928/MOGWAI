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

namespace MOGWAI.Tests
{
    public class ControlFlowTests : MogwaiTestBase
    {
        [Fact]
        public async Task For_nominal_doit_iterer_de_1_a_3()
        {
            var engine = CreateEngine(out var testDelegate);

            await engine.RunAsync("1 3 for 'i' do { i ? }", debugMode: false);

            Assert.Equal(3, testDelegate.Output.Count);
            Assert.Equal("1", testDelegate.Output[0]);
            Assert.Equal("2", testDelegate.Output[1]);
            Assert.Equal("3", testDelegate.Output[2]);
        }

        [Fact]
        public async Task For_start_equals_end_doit_iterer_une_fois()
        {
            // Régression bug #4 : bouclait indéfiniment quand start == end

            var engine = CreateEngine(out var testDelegate);
            await engine.RunAsync("1 1 for 'i' do { i ? }", debugMode: false);

            Assert.Single(testDelegate.Output);
            Assert.Equal("1", testDelegate.Output[0]);
        }
    }
}
