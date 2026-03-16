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
    internal class PrimitiveHttpGet : PrimitiveParamsRecord
    {
        public PrimitiveHttpGet(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override async Task<EvalResult> PerformOperation(MOGRecord record)
        {
            // record http.get
            // le record passé en paramètre contient toutes les informations pour effectuer la requête
            // Le résultat est posé sur la pile sous la forme d'un record

            // record input
            // [
            // uri: "https://api.github.com/orgs/dotnet/repos" 
            // requestHeaders: [User-Agent: ".NET Foundation Repository Reporter" token: "XXXXX"]
            // ]

            // record output
            // [
            // state: true 
            // response: data
            // ]       
            // Il faut une valeur pour la clé url:
            // Ce doit être une string

            if (record.GetItem("uri") is MOGString uri)
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Clear();

                try
                {
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

                    var r = await client.GetByteArrayAsync(uri.Value);

                    var record2 = new MOGRecord(Engine);
                    record2.Items["state"] = new MOGBoolean(Engine, true);
                    record2.Items["statusCode"] = new MOGNumber(Engine, 200);
                    record2.Items["response"] = new MOGData(Engine, r);

                    Engine.StackPush(record2);
                }
                catch (HttpRequestException ex)
                {
                    // Erreur !

                    var record2 = new MOGRecord(Engine);
                    record2.Items["state"] = new MOGBoolean(Engine, false);
                    record2.Items["statusCode"] = new MOGNumber(Engine, (int)(ex.StatusCode!.Value));
                    record2.Items["error"] = new MOGString(Engine, ex.Message);

                    Engine.StackPush(record2);
                }
                catch (Exception ex)
                {
                    // Erreur !

                    var record2 = new MOGRecord(Engine);
                    record2.Items["state"] = new MOGBoolean(Engine, false);
                    record2.Items["error"] = new MOGString(Engine, ex.Message);

                    Engine.StackPush(record2);
                }

                return EvalResult.NoError;
            }

            return EvalResult.Failure(Engine, Error.BadArgumentValueError, "uri: key is mandatory");
        }
    }
}
