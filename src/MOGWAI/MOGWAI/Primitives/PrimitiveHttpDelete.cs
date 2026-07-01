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
    internal class PrimitiveHttpDelete : PrimitiveParamsRecord
    {
        public override Version Birth => new(8, 14, 0);

        public PrimitiveHttpDelete(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveHttpDelete(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> PerformOperation(MOGRecord record)
        {
            // record http.delete
            //
            // record input
            // [
            // uri: "https://api.github.com/repos/dotnet/runtime/issues/1"
            // requestHeaders: [ ]
            // ]
            //
            // record output
            // [
            // state: true
            // statusCode: 200
            // response: data
            // responseHeaders: (...)
            // ]

            if (record.GetItem("uri") is not MOGString uri)
                return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "uri: key is mandatory");

            // TODO (sandbox profile B): validate/filter uri.Value against a host whitelist
            // before performing the request, to prevent SSRF in non-trusted mode.

            var responseRecord = new MOGRecord(Engine);

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Delete, uri.Value);

                // Headers are built on the HttpRequestMessage rather than on
                // DefaultRequestHeaders, to remain thread-safe with a shared client

                if (record.GetItem("requestHeaders") is MOGRecord requestHeaders)
                {
                    foreach (var key in requestHeaders.Items.Keys)
                    {
                        if (requestHeaders.Items[key] is MOGString ms)
                            request.Headers.TryAddWithoutValidation(key, ms.Value);
                    }
                }

                // TODO: propagate a CancellationToken tied to the watchdog (time counter)
                // once it's in place, in addition to the HttpClient's Timeout.

                using var response = await Engine.HttpClient.SendAsync(request);

                // We always read the body, even on HTTP error (4xx/5xx),
                // since it often contains useful info (JSON error message, etc.)
                // Note: many DELETE endpoints return an empty body (204 No Content),
                // in which case this simply yields an empty MOGData.

                var body = await response.Content.ReadAsByteArrayAsync();

                // Building the response headers record
                // We merge the "protocol" headers (response.Headers) and the "content"
                // headers (response.Content.Headers, e.g. Content-Type, Content-Length)
                // since HttpClient splits them into two separate collections, which
                // doesn't make sense from the MOGWAI script's point of view.

                var responseHeaders = new MOGRecord(Engine);

                foreach (var header in response.Headers.Concat(response.Content.Headers))
                {
                    var values = new MOGList(Engine);

                    foreach (var v in header.Value)
                        values.Items.Add(new MOGString(Engine, v));

                    responseHeaders.Items[header.Key] = values;
                }

                responseRecord.Items["state"] = new MOGBoolean(Engine, response.IsSuccessStatusCode);
                responseRecord.Items["statusCode"] = new MOGNumber(Engine, (int)response.StatusCode);
                responseRecord.Items["response"] = new MOGData(Engine, body);
                responseRecord.Items["responseHeaders"] = responseHeaders;

                if (!response.IsSuccessStatusCode)
                    responseRecord.Items["error"] = new MOGString(Engine, $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
            }
            catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
            {
                // SendAsync timed out (HttpClient's Timeout), distinct from a real cancellation

                responseRecord.Items["state"] = new MOGBoolean(Engine, false);
                responseRecord.Items["error"] = new MOGString(Engine, "Request timed out");
            }
            catch (HttpRequestException ex)
            {
                // Network error (DNS, connection refused, SSL...): StatusCode is often null here

                responseRecord.Items["state"] = new MOGBoolean(Engine, false);

                if (ex.StatusCode.HasValue)
                    responseRecord.Items["statusCode"] = new MOGNumber(Engine, (int)ex.StatusCode.Value);

                responseRecord.Items["error"] = new MOGString(Engine, ex.Message);
            }
            catch (Exception ex)
            {
                responseRecord.Items["state"] = new MOGBoolean(Engine, false);
                responseRecord.Items["error"] = new MOGString(Engine, ex.Message);
            }

            Engine.StackPush(responseRecord);

            return EvalResult.NoError;
        }
    }
}
