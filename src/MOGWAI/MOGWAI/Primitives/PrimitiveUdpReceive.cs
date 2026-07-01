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
    internal class PrimitiveUdpReceive : PrimitiveParamsRecord
    {
        public override Version Birth => new(8, 14, 0);

        public PrimitiveUdpReceive(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveUdpReceive(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> PerformOperation(MOGRecord record)
        {
            // record udp.receive
            //
            // record input
            // [
            // localPort: 5001
            // timeout: 3000        # ms
            // ]
            //
            // record output
            // [
            // state: true
            // data: receivedData
            // remoteHost: "192.168.1.100"
            // remotePort: 5000
            // ]

            if (record.GetItem("localPort") is not MOGNumber localPort)
                return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "localPort: key is mandatory");

            if (record.GetItem("timeout") is not MOGNumber timeout)
                return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, "timeout: key is mandatory");

            var responseRecord = new MOGRecord(Engine);

            try
            {
                using var udpClient = new UdpClient((int)localPort.Value);
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeout.Value));

                var result = await udpClient.ReceiveAsync(cts.Token);

                responseRecord.SetBoolean("state", true);
                responseRecord.SetItem("data", new MOGData(Engine, result.Buffer));
                responseRecord.SetString("remoteHost", result.RemoteEndPoint.Address.ToString());
                responseRecord.SetNumber("remotePort", result.RemoteEndPoint.Port);
            }
            catch (OperationCanceledException)
            {
                // Timeout expired

                responseRecord.SetBoolean("state", false);
                responseRecord.SetString("error", "timeout");
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
