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

using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MOGWAI.Engine
{
    internal class DatagramManager
    {
        public string Name { get; private set; } = "NO NAME";
        
        public bool IsRunning => _managerRun;

        public delegate void ManagerDidStartEventHandler();
        public event ManagerDidStartEventHandler? ManagerDidStart;

        public delegate void ManagerDidStopEventHandler();
        public event ManagerDidStopEventHandler? ManagerDidStop;

        public delegate void DatagramDidReceiveEventHandler(IPEndPoint from, byte[] data);
        public event DatagramDidReceiveEventHandler? DatagramDidReceive;

        private Task? _ReceiverTask;
        private int _port;
        private bool _managerRun;
        private UdpClient? _udpClient;

        public void Start(string name, int port)
        {
            // On lance la lecture des datagrams

            Name = name;
            _port = port;

            _managerRun = true;
            _udpClient = new UdpClient(port);

            _ReceiverTask = Task.Run(async () =>
            {
                try
                {
                    while (_managerRun)
                    {
                        if (_udpClient.Available > 0)
                        {
                            var from = new IPEndPoint(IPAddress.Any, _port);
                            byte[] bytes = _udpClient.Receive(ref from);

                            DatagramDidReceive?.Invoke(from, bytes);
                        }
                        else
                        {
                            await Task.Delay(10);
                        }
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    _udpClient.Close();
                    ManagerDidStop?.Invoke();
                }
            });

            ManagerDidStart?.Invoke();
        }
        public void Stop()
        {
            _managerRun = false;
        }

        public bool SendDatagram(string hostname, int port, byte[] data)
        {
            if (_udpClient != null)
            {
                _udpClient.Send(data, data.Length, hostname, port);
                return true;
            }

            return false;
        }

        public bool SendDatagram(string hostname, int port, string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            return SendDatagram(hostname, port, bytes);
        }
    }
}
