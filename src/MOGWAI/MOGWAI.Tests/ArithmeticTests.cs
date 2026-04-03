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
    public class ArithmeticTests : MogwaiTestBase
    {
        [Fact]
        public async Task Addition_doit_laisser_5_sur_la_pile()
        {
            var engine = CreateEngine(out _);
            var result = await engine.RunAsync("2 3 +", debugMode: false);

            Assert.False(result.IsError);
            Assert.Equal(1, engine.StackSize);
            Assert.Equal(5, engine.StackPopNumber().Value);
        }

        [Fact]
        public async Task Division_par_zero_doit_echouer()
        {
            var engine = CreateEngine(out _);
            var result = await engine.RunAsync("10 0 /", debugMode: false);

            Assert.True(result.IsError);
        }

        [Fact]
        public async Task Variable_store_and_recall()
        {
            var engine = CreateEngine(out _);
            var result = await engine.RunAsync("42 -> 'x'  x", debugMode: false);

            Assert.False(result.IsError);
            Assert.Equal(42, engine.StackPopNumber().Value);
        }

        [Fact]
        public async Task Print_doit_capturer_la_sortie()
        {
            var engine = CreateEngine(out var testDelegate);
            await engine.RunAsync("\"Bonjour MOGWAI\" ?", debugMode: false);

            Assert.Contains("Bonjour MOGWAI", testDelegate.Output);
        }
    }
}
