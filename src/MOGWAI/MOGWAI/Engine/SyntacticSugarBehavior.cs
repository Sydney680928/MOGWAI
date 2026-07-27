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

namespace MOGWAI.Engine
{
    public class SyntacticSugarBehavior
    {
        public bool AllowForDo { get; set; } = true;

        public bool AllowForStepDo { get; set; } = true;    

        public bool AllowForeverDo { get; set; } = true; 

        public bool AllowDuringDo { get; set; } = true;  

        public bool AllowTask { get; set; } = true;

        public bool AllowTimerDo { get; set; } = true; 

        public bool AllowPost { get; set; } = true;  
        
        public bool AllowAfterDo { get; set; } = true;    

        public bool AllowOnEventDo { get; set; } = true;

        public bool AllowWhileDo { get; set; } = true;

        public bool AllowDoWhile { get; set; } = true;

        public bool AllowForeachDo { get; set; } = true;

        public bool AllowForeachTransformDo { get; set; } = true;

        public bool AllowForeachFilterDo { get; set; } = true;

        public bool AllowClassDo { get; set; } = true;

        public bool AllowToDo { get; set; } = true;

        public bool AllowToWithDo { get; set; } = true;

        public bool AllowToParamsDo { get; set; } = true;

        public bool AllowToReturnsDo { get; set; } = true;

        public bool AllowToWithReturnsDo { get; set; } = true;

        public bool AllowToParamsReturnsDo { get; set; } = true;

        public bool AllowRepeat { get; set; } = true; 

        public bool AllowIfThen { get; set; } = true;   

        public bool AllowIfThenElse { get; set; } = true;   

        public bool AllowTrap { get; set; } = true; 

        public bool AllowGuardElse { get; set; } = true;    

        public bool AllowPipeRef { get; set; } = true;  

        public bool AllowDeclare { get; set; } = true;

        public bool AllowSto { get; set; } = true;

        public bool AllowStoPlus { get; set; } = true;

        public bool AllowStoSubstract { get; set; } = true;

        public bool AllowStoMultiply { get; set; } = true;

        public bool AllowStoDivide { get; set; } = true;

        public bool AllowSwitch { get; set; } = true;   
    }
}
