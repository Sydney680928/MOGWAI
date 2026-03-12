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
    public class Error
    {
        public static readonly Dictionary<string, Error> Errors = new();

        public static readonly Error None = RegisterError("MW.0", "OK");
        public static readonly Error ParseError = RegisterError("MW.1", "parse error");
        public static readonly Error HaltEncountedError = RegisterError("MW.2", "halt encounted error");
        public static readonly Error EmptyCodeError = RegisterError("MW.3", "empty code error");
        public static readonly Error InternalError = RegisterError("MW.4", "internal error");
        public static readonly Error PlatformNotSupportedError = RegisterError("MW.5", "platform not supported error");
        public static readonly Error UnabledToFireEventError = RegisterError("MW.6", "unabled to fire event error");
        public static readonly Error OperationNotSupportedError = RegisterError("MW.7", "operation not supported error");

        public static readonly Error GenericError = RegisterError("MW.10", "generic error");
        public static readonly Error PrimitiveSearchError = RegisterError("MW.11", "primitive not found error");

        public static readonly Error TooFewArgumentsError = RegisterError("MW.20", "too few arguments error");
        public static readonly Error BadArgumentTypeError = RegisterError("MW.21", "bad argument type error");
        public static readonly Error BadArgumentValueError = RegisterError("MW.22", "bad argument value error");
        public static readonly Error StackSizeError = RegisterError("MW.23", "stack size error");
        public static readonly Error StackCorruptionError = RegisterError("MW.24", "stack corruption error");

        public static readonly Error DivisionByZeroError = RegisterError("MW.30", "division by zero error");
        public static readonly Error MathematicalError = RegisterError("MW.31", "mathematical error");

        public static readonly Error UnknownNameError = RegisterError("MW.40", "unknown name error");
        public static readonly Error NameAlreadyExistsError = RegisterError("MW.41", "name already exits error");
        public static readonly Error FunctionAlreadyExistsError = RegisterError("MW.42", "function already exists error");
        public static readonly Error NameAlreadyUsedByFunctionError = RegisterError("MW.43", "name already used by function error");
        public static readonly Error NameAlreadyUsedByVarError = RegisterError("MW.44", "name already used by var error");
        public static readonly Error UnknownKeyError = RegisterError("MW.45", "unknown key error");
        public static readonly Error InvalidNameError = RegisterError("MW.46", "invalid name error");
        public static readonly Error UnabledToWriteValueError = RegisterError("MW.47", "unabled to write value in var");
        public static readonly Error UnabledToWriteValueInUndeclaredVarError = RegisterError("MW.48", "unabled to write value in undeclared var");

        public static readonly Error UnknownWordError = RegisterError("MW.50", "unknown word error");

        public static readonly Error TaskCreationError = RegisterError("MW.60", "task creation error");
        public static readonly Error UnabledToStartTaskError = RegisterError("MW.61", "unabled to start task error");
        public static readonly Error InvalidOutsideOfaTaskError = RegisterError("MW.62", "invalid outside of a task error");

        public static readonly Error InvalidPathError = RegisterError("MW.70", "invalid path error");
        public static readonly Error PathDoesNotExistsError = RegisterError("MW.71", "path does not exists error");
        public static readonly Error FileOperationError = RegisterError("MW.72", "file operation error");
        public static readonly Error UnknownFileError = RegisterError("MW.73", "unknown file error");

        public static readonly Error UsingError = RegisterError("MW.80", "using error");
        public static readonly Error UsingAlreadyExistsError = RegisterError("MW.81", "using already exists error");

        public static readonly Error FatalError = RegisterError("MW.!!!", "fatal error");

        public enum ErrorType
        {
            Internal,
            Using,
            Host,
            User
        }

        public static Error GetError(string code)
        {              
            if (Errors.TryGetValue(code, out var error))
                return error;

            return new Error(code, "user error", ErrorType.User);
        }

        public static Error RegisterError(string code, string message, ErrorType type = ErrorType.Internal)
        {
            var error = new Error(code, message, type);
            Errors.Add(code, error);
            return error;
        }

        public static void ClearUsingsErrors()
        {
            foreach (var key in Errors.Keys)
            {
                if (Errors[key].Type == ErrorType.Using)
                    Errors.Remove(key);
            }
        }

        public static void ClearHostErrors()
        {
            foreach (var key in Errors.Keys)
            {
                if (Errors[key].Type == ErrorType.Host)
                    Errors.Remove(key);
            }
        }

        public string Code { get; set; }

        public string Message { get; set; }

        public bool IsOK => Code == None.Code;

        public readonly ErrorType Type;

        public Error(string code, string message, ErrorType type)
        {
            Code = code;
            Message = message;
            Type = type;
        }

        public override string ToString()
        {
            return $"{Message} ({Code})";
        }
    }
}
