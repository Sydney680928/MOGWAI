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

using MOGWAI.Engine;
using MOGWAI.Objects;

namespace MOGWAI.Primitives
{
    internal class PrimitiveFileFileInfo : PrimitiveParamsString
    {
        public PrimitiveFileFileInfo(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveFileFileInfo(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> PerformOperation(MOGString @string)
        {
            if (@string.Value.Length == 0)
                return Task.FromResult(EvalResult.Failure(Engine, Error.BadArgumentValueError, Name));

            try
            {
                var path = Path.GetFullPath(@string.Value);
                var info = new FileInfo(path);

                if (info.Exists)
                {
                    var record = new MOGRecord(Engine);

                    record.SetItem("name", new MOGString(Engine, info.Name));
                    record.SetItem("fullName", new MOGString(Engine, info.FullName));
                    record.SetItem("directoryName", new MOGString(Engine, info.DirectoryName ?? ""));
                    record.SetItem("extension", new MOGString(Engine, info.Extension));
                    record.SetItem("modifiedTime", new MOGNumber(Engine, info.LastWriteTime.Ticks));
                    record.SetItem("lastAccessTime", new MOGNumber(Engine, info.LastAccessTime.Ticks));
                    record.SetItem("length", new MOGNumber(Engine, info.Length));
                    record.SetItem("isReadOnly", new MOGBoolean(Engine, info.IsReadOnly));
                    record.SetItem("isArchive", new MOGBoolean(Engine, (info.Attributes & FileAttributes.Archive) != 0));
                    record.SetItem("isHidden", new MOGBoolean(Engine, (info.Attributes & FileAttributes.Hidden) != 0));
                    record.SetItem("isSystem", new MOGBoolean(Engine, (info.Attributes & FileAttributes.System) != 0));

                    Engine.StackPush(record);

                    return Task.FromResult(EvalResult.NoError);
                }
                else
                {
                    return Task.FromResult(EvalResult.Failure(Engine, Error.FileOperationError, Name, path,"File does not exist"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(EvalResult.Failure(Engine, Error.FileOperationError, Name, ex.Message));
            }
        }
    }
}
