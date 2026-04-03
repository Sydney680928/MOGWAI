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

using MOGWAI.Objects;

namespace MOGWAI.Tests
{
    public class BagTests : MogwaiTestBase
    {
        [Fact]
        public async Task Bag_champ_calcule_retourne_somme_x_et_y()
        {
            var engine = CreateEngine(out var testDelegate);

            await engine.RunAsync("""
            [x: 50 y: 100 s: «! bag x: get bag y: get + »] -> '$R'
            $R s: get ?
            """, debugMode: false);

            Assert.Single(testDelegate.Output);
            Assert.Equal("150", testDelegate.Output[0]);
        }

        [Fact]
        public async Task Bag_mutation_x_recalcule_s()
        {
            // &$R mute x en place, s doit refléter la nouvelle valeur
            var engine = CreateEngine(out var testDelegate);

            await engine.RunAsync("""
            [x: 50 y: 100 s: «! bag x: get bag y: get + »] -> '$R'
            &$R x: 1000 set
            $R s: get ?
            """, debugMode: false);

            Assert.Single(testDelegate.Output);
            Assert.Equal("1100", testDelegate.Output[0]);
        }

        [Fact]
        public async Task Bag_get_champ_inexistant_retourne_null()
        {
            var engine = CreateEngine(out _);

            var result = await engine.RunAsync("""
            [x: 50] -> '$R'
            $R z: get
            """, debugMode: false);

            Assert.False(result.IsError);
            Assert.Equal(1, engine.StackSize);
            var value = engine.StackPop();
            Assert.IsType<MOGNull>(value);
        }

        [Fact]
        public async Task Bag_set_champ_inexistant_cree_le_champ()
        {
            var engine = CreateEngine(out var testDelegate);

            var result = await engine.RunAsync("""
            [x: 50] -> '$R'
            &$R z: 99 set
            $R z: get ?
            """, debugMode: false);

            Assert.False(result.IsError);
            Assert.Single(testDelegate.Output);
            Assert.Equal("99", testDelegate.Output[0]);
        }
    }
}
