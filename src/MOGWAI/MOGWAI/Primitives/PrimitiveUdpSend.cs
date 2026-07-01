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
using System.Net.Sockets;

namespace MOGWAI.Primitives
{
    internal class PrimitiveUdpSend : PrimitiveParamsRecord
    {
        public override Version Birth => new(8, 14, 0);

        public PrimitiveUdpSend(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveUdpSend(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> PerformOperation(MOGRecord record)
        {
            // record udp.send
            //
            // record input
            // [
            // host: "192.168.1.100"
            // port: 5000
            // data: myData
            // localPort: 5001      # optional, ephemeral port if absent
            // ]
            //
            // record output
            // [
            // state: true
            // ]

            if (record.GetItem("host") is not MOGString host)
                return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "host: key is mandatory");

            if (record.GetItem("port") is not MOGNumber port)
                return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "port: key is mandatory");

            if (record.GetItem("data") is not MOGData data)
                return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "data: key is mandatory");

            // TODO (sandbox profile B): validate host/port against a whitelist
            // before performing the request, to prevent unauthorized network access.

            var responseRecord = new MOGRecord(Engine);

            try
            {
                // If localPort is specified, bind to it; otherwise use an ephemeral port (0)

                var localPort = record.GetItem("localPort") is MOGNumber lp ? (int)lp.Value : 0;
                using var udpClient = new UdpClient(localPort);

                var bytes = data.Items.ToArray();

                await udpClient.SendAsync(bytes, bytes.Length, host.Value, (int)port.Value);

                responseRecord.Items["state"] = new MOGBoolean(Engine, true);
                responseRecord.SetBoolean("state", true);
            }
            catch (Exception ex)
            {
                responseRecord.SetBoolean("state", false);
                responseRecord.SetString("error", ex.Message);
            }

            Engine.StackPush(responseRecord);

            return EvalResult.NoError;
        }
    }
}
