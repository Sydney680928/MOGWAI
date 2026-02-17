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

namespace WinFormsMogwai
{
    internal class ConsoleDisplay : TextBox
    {
        private const int
            WM_MOUSEMOVE = 0x0200,
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONDBLCLK = 0x0203,
            WM_LBUTTONUP = 0x0202,
            WM_CAPTURECHANGED = 0x0215,
            EM_SETSEL = 0x00B1;

        private bool _inputMode = false;
        private int _inputStartPosition = 0;
        private TaskCompletionSource<string>? _inputTaskCompletionSource;

        public ConsoleDisplay() : base()
        {
            Multiline = true;
            BorderStyle = BorderStyle.FixedSingle;
            ReadOnly = true;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_CAPTURECHANGED:
                    break;
                case WM_LBUTTONDOWN:
                    break;
                case WM_LBUTTONUP:
                    break;
                case WM_LBUTTONDBLCLK:
                    break;
                case WM_MOUSEMOVE:
                    break;
                case EM_SETSEL:
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_inputMode)
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                    case Keys.Down:
                    case Keys.Left:
                    case Keys.Right:
                        e.SuppressKeyPress = true;
                        break;

                    case Keys.Back:
                        if (SelectionStart <= _inputStartPosition)
                        {
                            e.SuppressKeyPress = true;
                        }
                        break;

                    case Keys.Enter:
                        var s = Text.Substring(_inputStartPosition, SelectionStart - _inputStartPosition);
                        _inputMode = false;
                        e.SuppressKeyPress = true;
                        ReadOnly = true;
                        WriteLine("");
                        _inputTaskCompletionSource?.SetResult(s);
                        break;
                }
            }
            else
            {
                e.SuppressKeyPress = true;
            }
        }

        public void WriteLine(string text)
        {
            AppendText($"{text}\r\n");
        }

        public void Write(string text)
        {
            AppendText(text);
        }

        public Task<string> Input(string prompt)
        {
            if (_inputMode)
            {
                if (_inputTaskCompletionSource != null)
                {
                    return _inputTaskCompletionSource.Task;
                }
                else
                {
                    throw new Exception("Console internal error !");
                }
            }

            Invoke(() =>
            {
                _inputMode = true;

                Write(prompt);

                _inputStartPosition = SelectionStart;
                ReadOnly = false;

                Focus();
            });

            _inputTaskCompletionSource = new TaskCompletionSource<string>();

            return _inputTaskCompletionSource.Task;
        }

        public void StopInputMode()
        {
            if (_inputMode)
            {
                _inputMode = false;
                ReadOnly = true;
                _inputTaskCompletionSource?.SetResult(string.Empty);
            }
        }
    }
}
