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
    internal class PrimitiveToUri : PrimitiveParamsRecord
    {
        public PrimitiveToUri(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveToUri(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override Task<EvalResult> PerformOperation(MOGRecord record)
        {
            // [url: "https://cloude.olnica.com" path: "api/v0/login" query: [id: "50" name: "SIBUE"]] ->uri


            UriBuilder? ub = null;

            if (record.GetItem("url") is MOGString url)
            {
                ub = new UriBuilder(url.Value);
            }
            else
            {
                ub = new UriBuilder();
            }

            if (record.GetItem("path") is MOGString path)
                ub.Path = path.Value;

            if (record.GetItem("scheme") is MOGString scheme)
                ub.Scheme = scheme.Value;

            if (record.GetItem("host") is MOGString host)
                ub.Host = host.Value;

            if (record.GetItem("port") is MOGNumber port)
                ub.Port = port.IntValue;

            if (record.GetItem("username") is MOGString username)
                ub.UserName = username.Value;

            if (record.GetItem("fragment") is MOGString fragment)
                ub.Fragment = fragment.Value;

            if (record.GetItem("query") is MOGRecord recQuery)
            {
                var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

                foreach (var key in recQuery.Items.Keys)
                {
                    var value = recQuery.Items[key];
                    if (value is MOGString s) queryString.Add(key, s.Value);
                }

                ub.Query = queryString.ToString();
            }

            Engine.StackPushString(ub.ToString());
            return Task.FromResult(EvalResult.NoError);
        }
    }
}
