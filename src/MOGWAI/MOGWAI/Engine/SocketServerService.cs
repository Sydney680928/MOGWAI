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

using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace MOGWAI.Engine
{
    internal class SocketServerService
    {
        public delegate void MessageDidReceiveEventHandler(object sender, ServerMessage serverMessage);       
        public event MessageDidReceiveEventHandler? MessageDidReceive;

        public delegate void ServerDidDisconnectEventHandler(object sender);
        public event ServerDidDisconnectEventHandler? ServerDidDisconnect;

        public IPAddress? IpAddress { get; private set; }
        
        public int Port { get; private set; }

        public bool IsRunning => _processMessageTask != null && !_processMessageTask.IsCompleted;

        private StreamReader? _socketReader = null;
        private StreamWriter? _socketWriter = null;
        private bool _requestStopServer = false;
        private Task? _processMessageTask;
        private MogwaiEngine? _engine = null;

        public SocketServerService()
        {

        }

        public async Task<bool> StartServerAsync(MogwaiEngine engine, string address, int port)
        {
            _engine = engine;   

            var fields = address.Split('.');

            if (fields.Length == 4)
            {
                var bytes = new byte[4];

                bytes[0] = byte.Parse(fields[0]);
                bytes[1] = byte.Parse(fields[1]);
                bytes[2] = byte.Parse(fields[2]);
                bytes[3] = byte.Parse(fields[3]);

                IpAddress = new IPAddress(bytes);
            }
            else
            {
                return false;
            }

            Port = port;

            TcpListener listener = new TcpListener(IpAddress!, Port);
            listener.Start(1);

            engine.Delegate?.ConsoleClearScreen(engine);
            engine.Delegate?.ConsolePrintLn(engine, MogwaiEngine.RuntimePrompt);
            engine.Delegate?.ConsolePrintLn(engine, "\r\n\r\nWAIT FOR MOGWAI STUDIO CONNECTION...");

            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var ni in networkInterfaces)
            {
                if (ni.NetworkInterfaceType != NetworkInterfaceType.Loopback && ni.NetworkInterfaceType != NetworkInterfaceType.Unknown && ni.OperationalStatus == OperationalStatus.Up)
                {
                    var ipp = ni.GetIPProperties();

                    if (ipp.GatewayAddresses.Count > 0)
                    {
                        foreach (var addr in ipp.UnicastAddresses)
                        {
                            if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                engine.Delegate?.ConsolePrintLn(engine, $"{addr.Address}:{Port}");  
                            }
                        }
                    }
                }
            }

            try
            {
                TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                listener.Stop();

                engine.Delegate?.ConsoleClearScreen(engine);
                engine.Delegate?.ConsolePrintLn(engine, "MOGWAI RUNTIME");
                engine.Delegate?.ConsolePrintLn(engine, "DEBUG MODE WITH MOGWAI STUDIO");

                if (engine.Delegate != null)
                    engine.Delegate?.StudioDidConnect(engine);

                _requestStopServer = false;
                _processMessageTask = Task.Run(async () => { await ProcessMessagesAsync(tcpClient); });

                return true;
            }
            catch (Exception ex)
            {
                engine.Delegate?.ConsolePrintLn(engine, "UNABLE TO WAIT FOR DEBUG STUDIO !");
                engine.Delegate?.ConsolePrintLn(engine, ex.Message);
                // engine.Delegate?.ConsoleReadLine(engine);
                return false;
            }         
        }

        public void StopServer()
        {
            _requestStopServer = true;
        }

        private void OnMessageReceived(ServerMessage message)
        {
            MessageDidReceive?.Invoke(this, message);
        }

        private async Task ProcessMessagesAsync(TcpClient tcpClient)
        {
            try
            {
                NetworkStream networkStream = tcpClient!.GetStream();

                _socketReader = new StreamReader(networkStream);

                _socketWriter = new StreamWriter(networkStream);
                _socketWriter.AutoFlush = true;

                while (!_requestStopServer)
                {
                    string? request = await _socketReader.ReadLineAsync();

                    if (request != null)
                    {
                        // Traduction du message json reçu en objet ServerMessage

                        try
                        {
                            // var message = JsonConvert.DeserializeObject<ServerMessage>(request);
                            // var message = System.Text.Json.JsonSerializer.Deserialize<ServerMessage>(request);
                            var message = System.Text.Json.JsonSerializer.Deserialize(request, MogwaiJsonContext.Default.ServerMessage);

                            if (message != null)
                            {
                                // Déclenchement événement de message reçu

                                Debug.WriteLine($"MOGWAI MESSAGE DID RECEIVE = {message.Function}");
                                OnMessageReceived(message);
                            }
                        }
                        catch
                        {
                            
                        }
                    }
                    else
                    {
                        // Déconnexion

                        break;
                    }
                }
            }
            catch
            {
         
            }

            // On a terminé (on a demandé un arrêt du serveur ou une erreur s'est produite)

            _socketReader?.Close();
            _socketReader = null;

            _socketWriter?.Close(); 
            _socketWriter = null;
            
            tcpClient.Close();

            ServerDidDisconnect?.Invoke(this);

            if (_engine != null && _engine?.Delegate != null)
                await _engine.Delegate.StudioDidDisconnect(_engine);
        }

        public async Task SendToClientAsync(string function, params string[] parameters)
        {
            var message = new ServerMessage("MOGWAI RUNTIME", function, parameters);
            await SendToClientAsync(message);
        }

        private async Task<bool> SendToClientAsync(ServerMessage message)
        {
            // var msg = System.Text.Json.JsonSerializer.Serialize(message);
            var msg = System.Text.Json.JsonSerializer.Serialize(message, MogwaiJsonContext.Default.ServerMessage);

            return await SendToClientAsync(msg);
        }

        private async Task<bool> SendToClientAsync(string message)
        {
            if (_socketWriter != null)
            {
                try
                {
                    await _socketWriter.WriteLineAsync(message);
                    return true;
                }
                catch
                {

                }
            }

            return false;
        }
    }
}
