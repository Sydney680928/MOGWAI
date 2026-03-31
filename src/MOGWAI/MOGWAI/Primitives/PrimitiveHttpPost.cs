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
    internal class PrimitiveHttpPost : PrimitiveParamsRecord
    {
        public PrimitiveHttpPost(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveHttpPost(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> PerformOperation(MOGRecord record)
        {
            // record http.post
            // le record passé en paramètre contient toutes les informations pour effectuer la requête
            // Le résultat est posé sur la pile sous la forme d'un record

            // record input
            // [
            // uri: "https://api.github.com/orgs/dotnet/repos" 
            // requestHeaders: [ ]
            // contentHeaders: [ ]
            // content: data
            // ]

            // record output
            // [
            // state: true 
            // response: data
            // ]

            // Il faut une valeur pour la clé uri:
            // Ce doit être une string
            // Il faut une valeur pour la clé content:
            // Ce doit être un data

            if (record.GetItem("uri") is MOGString uri && record.GetItem("content") is MOGData content)
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Clear();

                var record2 = new MOGRecord(Engine);

                try
                {
                    var httpContent = new ByteArrayContent(content.Items.ToArray());
                    httpContent.Headers.Clear();

                    if (record.GetItem("requestHeaders") is MOGRecord requestHeaders)
                    {
                        foreach (var key in requestHeaders.Items.Keys)
                        {
                            // On ne prend en compte que les valeurs de type MOGString
                            // Les autres sont ignorées

                            var value = requestHeaders.Items[key];

                            if (value is MOGString ms)
                                client.DefaultRequestHeaders.Add(key, ms.Value);
                        }
                    }

                    if (record.GetItem("contentHeaders") is MOGRecord contentHeaders)
                    {
                        foreach (var key in contentHeaders.Items.Keys)
                        {
                            // On ne prend en compte que les valeurs de type MOGString
                            // Les autres sont ignorées

                            var value = contentHeaders.Items[key];

                            if (value is MOGString ms)
                                httpContent.Headers.Add(key, ms.Value);
                        }
                    }

                    var response = await client.PostAsync(uri.Value, httpContent);

                    record2.Items["statusCode"] = new MOGNumber(Engine, (int)response.StatusCode);

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsByteArrayAsync();

                        record2.Items["state"] = new MOGBoolean(Engine, true);
                        record2.Items["response"] = new MOGData(Engine, data);
                    }
                    else
                    {
                        record2.Items["state"] = new MOGBoolean(Engine, false);
                    }
                }
                catch
                {
                    // Erreur !

                    record2.Items["state"] = new MOGBoolean(Engine, false);
                }

                Engine.StackPush(record2);
                return EvalResult.NoError;
            }

            return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "uri: and content: keys are mandatory");
        }
    }
}
