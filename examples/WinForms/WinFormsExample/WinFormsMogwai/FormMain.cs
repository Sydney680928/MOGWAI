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
using MOGWAI.Interfaces;
using MOGWAI.Objects;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsMogwai
{
    public partial class FormMain : Form, IDelegate
    {
        private MogwaiEngine _engine;

        private float _currentWidth = 1;
        private Color _currentColor = Color.Yellow;
        private Pen _currentPen;
        private Bitmap? _currentImage;
        private bool _penIsDown = true;

        private double _turtleX;
        private double _turtleY;
        private double _turtleAngle;
        private Color _turtleColor = Color.Yellow;
        private bool _turtleIsVisible = true;
        private string? _rootFolder;

        private const int EM_SETTABSTOPS = 0x00CB;

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr h, int msg, int wParam, int[] lParam);

        public FormMain()
        {
            InitializeComponent();

            // Creating the scripting engine.

            _engine = new MogwaiEngine("MOGWAI WinForms", true, true);

            // Add his delegate (this window) to the engine.
            // The delegate is the link between the engine and its host (this window).
            // It must implement the IDelegate interface and allows the engine to call functions in the host (this window) and reciprocally.

            _engine.Delegate = this;

            // Initialization of the pen used by the turtle.

            _currentPen = new Pen(_currentColor, _currentWidth);

            // Centering the turtle

            TurtleClear();

            // We set the tab offset size in the code

            SendMessage(CodeTextBox.Handle, EM_SETTABSTOPS, 1, [15]);

            // Load samples in the combo box

            _rootFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            try
            {
                if (_rootFolder != null)
                {
                    var examplesFolder = Path.Combine(_rootFolder, "CodeExample");
                    var examples = Directory.GetFiles(examplesFolder, "*.mog");

                    foreach (var example in examples)
                    {
                        var name = Path.GetFileNameWithoutExtension(example);
                        SamplesComboBox.Items.Add(name);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Unable to load code examples from CodeExample folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Private functions

        private void LoadCode(string filename)
        {
            if (_rootFolder != null)
            {
                try
                {
                    var path = Path.Combine(_rootFolder, "CodeExample", $"{filename}.mog");
                    var code = File.ReadAllText(path);
                    CodeTextBox.Text = code;
                }
                catch
                {
                    MessageBox.Show($"Unable to load code example {filename}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region UI

        private void FormMain_Load(object sender, EventArgs e)
        {

        }

        private async void ExecuteButton_Click(object sender, EventArgs e)
        {
            // We send the code to MOGWAI

            ExecuteButton.Enabled = false;
            CodeTextBox.Enabled = false;

            var r = await _engine.RunAsync(CodeTextBox.Text, false);

            ConsoleWriteLine("");
            ConsoleWriteLine(r.ToString());

            CodeTextBox.Enabled = true;
            ExecuteButton.Enabled = true;
        }

        private void HaltButton_Click(object sender, EventArgs e)
        {
            OutputTextBox.StopInputMode();
            _engine.Halt();
        }

        private void SamplesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SamplesComboBox.SelectedItem is string filename)
                LoadCode(filename);
        }

        private void DrawTurtlePictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (_currentImage != null)
            {
                e.Graphics.DrawImage(_currentImage, new Point(0, 0));
            }

            if (_turtleIsVisible) DrawTurtle(e.Graphics, false);
        }

        private void FormMain_SizeChanged(object sender, EventArgs e)
        {
            TurtleClear();
            DrawTurtlePictureBox.Invalidate();
        }

        #endregion

        #region Turtle-related functions

        private double DegToRad(double angle)
        {
            return angle * Math.PI / 180.0;
        }

        private void RefreshGraphAreaIfNeed()
        {
            if (!_turtleIsVisible)
            {
                DrawTurtlePictureBox.Invalidate();
            }
        }

        public void ShowTurtle(bool value)
        {
            if (InvokeRequired)
            {
                Invoke(() => { ShowTurtle(value); });

            }
            else
            {
                _turtleIsVisible = value;
                DrawTurtlePictureBox.Invalidate();
            }
        }

        private void DrawTurtle(bool invalidate)
        {
            if (InvokeRequired)
            {
                Invoke(() => { DrawTurtle(invalidate); });
            }
            else
            {
                DrawTurtle(_turtleX, _turtleY, _turtleAngle, _turtleColor, invalidate);
            }
        }

        private void DrawTurtle(Graphics graphics, bool invalidate)
        {
            if (InvokeRequired)
            {
                Invoke(() => { DrawTurtle(graphics, invalidate); });
            }
            else
            {
                DrawTurtle(graphics, _turtleX, _turtleY, _turtleAngle, _turtleColor);
                if (invalidate) RefreshTurtlePictureBox();
            }
        }

        private void DrawTurtle(double x, double y, double angle, Color color, bool invalidate)
        {
            if (_turtleIsVisible)
            {
                using (var g = DrawTurtlePictureBox.CreateGraphics())
                {
                    DrawTurtle(g, x, y, angle, color);
                }

                if (invalidate) DrawTurtlePictureBox.Invalidate();
            }
        }

        private void DrawTurtle(Graphics graphics, double x, double y, double angle, Color color)
        {
            angle = 180 - angle;

            double r = 40.0;
            double xd = (double)x;
            double yd = (double)y;
            double a1 = DegToRad(angle);
            double a2 = DegToRad(angle + 135);
            double a3 = DegToRad(angle + 225);

            Point p0 = new Point((int)x, (int)y);
            Point p1 = new Point((int)(xd + r * Math.Sin(a1)), (int)(yd + r * Math.Cos(a1)));
            Point p2 = new Point((int)(xd + r * Math.Sin(a2)), (int)(yd + r * Math.Cos(a2)));
            Point p3 = new Point((int)(xd + r * Math.Sin(a3)), (int)(yd + r * Math.Cos(a3)));

            Pen pen = new Pen(color, 2F);

            graphics.DrawLine(pen, p1, p2);
            graphics.DrawLine(pen, p2, p0);
            graphics.DrawLine(pen, p0, p3);
            graphics.DrawLine(pen, p3, p1);
        }

        private void TurtleClear()
        {
            if (InvokeRequired)
            {
                Invoke(TurtleClear);
            }
            else
            {
                _turtleX = DrawTurtlePictureBox.Width / 2;
                _turtleY = DrawTurtlePictureBox.Height / 2;
                _turtleAngle = 0;

                _currentImage = new Bitmap(DrawTurtlePictureBox.Width, DrawTurtlePictureBox.Height);

                DrawTurtlePictureBox.Invalidate();
            }
        }

        public void TurtleForward(int distance)
        {
            if (InvokeRequired)
            {
                Invoke(() => { TurtleForward(distance); });
            }
            else
            {
                double a = DegToRad(180 - _turtleAngle);
                double x = _turtleX + distance * Math.Sin(a);
                double y = _turtleY + distance * Math.Cos(a);

                if (_penIsDown)
                {
                    TurtleDrawLine(_turtleX, _turtleY, x, y);
                }

                _turtleX = x;
                _turtleY = y;

                if (_turtleIsVisible)
                {
                    DrawTurtlePictureBox.Invalidate();
                }
            }
        }

        public void TurtleRotate(double angle)
        {
            if (InvokeRequired)
            {
                Invoke((Action)delegate { TurtleRotate(angle); });
                return;
            }

            _turtleAngle += angle;
            if (_turtleAngle > 360) { _turtleAngle %= 360; }

            if (_turtleIsVisible)
            {
                DrawTurtlePictureBox.Invalidate();
            }
        }

        public void TurtlePenDown(bool value)
        {
            _penIsDown = value;
        }

        public void RefreshTurtlePictureBox()
        {
            DrawTurtlePictureBox.Invalidate();
        }

        public void TurtleSetColor(double alpha, double red, double green, double blue)
        {
            if (alpha > -1 && alpha < 256 &&
                red > -1 && red < 256 &&
                green > -1 && green < 256 &&
                blue > -1 && blue < 256)
            {
                _currentColor = Color.FromArgb((int)alpha, (int)red, (int)green, (int)blue);
                _currentPen = new Pen(_currentColor, _currentWidth);
            }
        }

        public void TurtleDrawPlot(double x, double y)
        {
            if (_currentImage != null)
            {
                if (InvokeRequired)
                {
                    Invoke(() => { TurtleDrawPlot(x, y); });
                }
                else
                {
                    using (var g = Graphics.FromImage(_currentImage))
                    {
                        g.DrawLine(_currentPen, (float)x, (float)y, (float)x, (float)y);
                    }
                }
            }
        }

        public void TurtleDrawLine(double x1, double y1, double x2, double y2)
        {
            if (_currentImage != null)
            {
                if (InvokeRequired)
                {
                    Invoke((Action)delegate { TurtleDrawLine(x1, y1, x2, y2); });
                }
                else
                {
                    using (var g = Graphics.FromImage(_currentImage))
                    {
                        g.DrawLine(_currentPen, (float)x1, (float)y1, (float)x2, (float)y2);
                    }
                }
            }
        }

        public void TurtleDrawRect(double x, double y, double w, double h)
        {
            if (_currentImage != null)
            {
                if (InvokeRequired)
                {
                    Invoke(() => { TurtleDrawRect(x, y, w, h); });
                }
                else
                {
                    using (var g = Graphics.FromImage(_currentImage))
                    {
                        g.DrawRectangle(_currentPen, (float)x, (float)y, (float)w, (float)h);
                    }
                }
            }
        }

        public void TurtleDrawEllipse(double x, double y, double w, double h)
        {
            if (_currentImage != null)
            {
                if (InvokeRequired)
                {
                    Invoke((Action)delegate { TurtleDrawEllipse(x, y, w, h); });
                }
                else
                {
                    using (var g = Graphics.FromImage(_currentImage))
                    {
                        g.DrawEllipse(_currentPen, (float)x, (float)y, (float)w, (float)h);
                    }
                }
            }
        }

        public void TurtleDrawCircle(double x, double y, double r)
        {
            if (_currentImage != null)
            {
                if (InvokeRequired)
                {
                    Invoke((Action)delegate { TurtleDrawCircle(x, y, r); });
                }
                else
                {
                    using (var g = Graphics.FromImage(_currentImage))
                    {
                        double left = x - r;
                        double top = y - r;
                        double width = r * 2;
                        double height = width;

                        g.DrawEllipse(_currentPen, (float)left, (float)top, (float)width, (float)height);
                    }
                }
            }
        }

        #endregion

        #region Functions related to the output console

        private void ConsoleWrite(string? text)
        {
            if (InvokeRequired)
            {
                Invoke(() => { ConsoleWrite(text); });
            }
            else
            {
                OutputTextBox.Write(text ?? "");
            }
        }

        private void ConsoleWriteLine(string? text)
        {
            if (InvokeRequired)
            {
                Invoke(() => { ConsoleWriteLine(text); });
            }
            else
            {
                OutputTextBox.WriteLine(text ?? "");
            }
        }

        #endregion

        #region MOGWAI.IDelegate

        // All these functions are the link between the engine and its host (this window).
        // They are all called from a thread other than the UI thread.

        public async Task ProgramStart(MogwaiEngine engine, string code)
        {
            // Called when program starts
            // code parameter contains the code to execute

            await Task.CompletedTask;

            Invoke(() =>
            {
                HaltButton.Enabled = true;
            });
        }

        public async Task ProgramEnd(MogwaiEngine engine, EvalResult result)
        {
            // Called when program ends
            // result parameter contains status (ok or error)

            await Task.CompletedTask;

            Invoke(() =>
            {
                HaltButton.Enabled = false;
            });
        }

        public async Task<EvalResult> ConsoleClearScreen(MogwaiEngine engine)
        {
            // MOGWAI console.clear function
            // We clear the output console

            await Task.CompletedTask;
            Invoke(() => { OutputTextBox.Clear(); });
            return EvalResult.NoError;
        }

        public async Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message)
        {
            // MOGWAI console.printLn or ? function
            // We print a line to the output console

            await Task.CompletedTask;
            Invoke(() => { OutputTextBox.WriteLine(message); });
            return EvalResult.NoError;
        }

        public async Task<EvalResult> ConsolePrint(MogwaiEngine engine, string message)
        {
            // MOGWAI console.print or ?? function
            // We print a line to the output console

            await Task.CompletedTask;
            Invoke(() => { OutputTextBox.Write(message); });
            return EvalResult.NoError;
        }

        public async Task<EvalResult> ConsoleShow(MogwaiEngine engine)
        {
            // MOGWAI console.hide function
            // Not implemented in this example.

            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<EvalResult> ConsoleHide(MogwaiEngine engine)
        {
            // MOGWAI console.hide function
            // Not implemented in this example.

            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<EvalResult> ConsoleLocate(MogwaiEngine engine, int x, int y)
        {
            // MOGWAI console.locate function
            // Not implemented in this example.

            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<(EvalResult result, int x, int y)> ConsoleGetCursorPosition(MogwaiEngine engine)
        {
            // MOGWAI console.cursor function
            // Not implemented in this example.

            await Task.CompletedTask;
            return (EvalResult.NoError, 0, 0);
        }

        public async Task<EvalResult> ConsoleSetForegroundColor(MogwaiEngine engine, string color)
        {
            // MOGWAI console.setForegroundColor function
            // Not implemented in this example.

            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<EvalResult> ConsoleSetBackgroundColor(MogwaiEngine engine, string color)
        {
            // MOGWAI console.setBackgroundColor function
            // Not implemented in this example.

            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<(EvalResult result, int key)> ConsoleGetInputKey(MogwaiEngine engine)
        {
            // MOGWAI console.getInputKey function
            // Not implemented in this example.

            await Task.CompletedTask;
            return (EvalResult.NoError, -1);
        }

        public async Task<(EvalResult result, string? value)> Prompt(MogwaiEngine engine, string message)
        {
            // MOGWAI prompt function
            // Get string from console with prompt

            var v = await OutputTextBox.Input(message);
            return (EvalResult.NoError, v);
        }

        public string[] HostFunctions(MogwaiEngine engine)
        {
            // Return all additionnals keywords powered by the host

            return [
                "clg",
                "refresh",
                "turtle.penDown",
                "turtle.penUp",
                "turtle.show",
                "turtle.hide",
                "turtle.move",
                "turtle.turn"
                ];
        }

        public async Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word)
        {
            // Called when MOGWAI encounters a keyword it doesn't know.
            // In this case, it asks the host if it can respond.

            switch (word)
            {
                case "clg":
                    return await ClgExtension(engine, word);

                case "refresh":
                    return await RefreshExtension(engine, word);

                case "turtle.penDown":
                    return await PenDownExtension(engine, word);

                case "turtle.penUp":
                    return await PenUpExtension(engine, word);

                case "turtle.show":
                    return await ShowTurtleExtension(engine, word);

                case "turtle.hide":
                    return await HideTurtleExtension(engine, word);

                case "turtle.move":
                    return await MoveExtension(engine, word);

                case "turtle.turn":
                    return await TurnExtension(engine, word);
            }

            return EvalResult.NoExternalFunction;
        }

        public async Task<EvalResult> MessageReceivedFromRuntime(MogwaiEngine engine, string message, MOGObject parameter)
        {
            // Called when runtime send message to host

            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<EvalResult> DebugMessage(MogwaiEngine engine, string message)
        {
            // MOGWAI debug.write function

            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<EvalResult> DebugClear(MogwaiEngine engine)
        {
            // MOGWAI debug.clear function

            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<EvalResult> EngineDidPause(MogwaiEngine engine)
        {
            // Called when runtime is paused

            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<EvalResult> EngineDidResume(MogwaiEngine engine)
        {
            // Called when runtime is resumed

            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<EvalResult> StudioDidConnect(MogwaiEngine engine)
        {
            // Called when MOGWAI STUDIO connects

            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<EvalResult> StudioDidDisconnect(MogwaiEngine engine)
        {
            // Called when MOGWAI STUDIO disconnects

            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<EvalResult> SocketServerDidStart(MogwaiEngine engine, IPAddress address, int port)
        {
            // Called when debug server starts

            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<EvalResult> SocketServerDidStop(MogwaiEngine engine)
        {
            // Called when debug server stops

            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        #endregion

        #region MOGWAI additional functions

        private async Task<EvalResult> ClgExtension(MogwaiEngine engine, string word)
        {
            await Task.CompletedTask;
            TurtleClear();
            return EvalResult.NoError;
        }

        private async Task<EvalResult> RefreshExtension(MogwaiEngine engine, string word)
        {
            await Task.CompletedTask;
            RefreshGraphAreaIfNeed();
            return EvalResult.NoError;
        }

        private async Task<EvalResult> PenDownExtension(MogwaiEngine engine, string word)
        {
            await Task.CompletedTask;
            TurtlePenDown(true);
            return EvalResult.NoError;
        }

        private async Task<EvalResult> PenUpExtension(MogwaiEngine engine, string word)
        {
            await Task.CompletedTask;
            TurtlePenDown(false);
            return EvalResult.NoError;
        }

        private async Task<EvalResult> ShowTurtleExtension(MogwaiEngine engine, string word)
        {
            await Task.CompletedTask;
            ShowTurtle(true);
            return EvalResult.NoError;
        }

        private async Task<EvalResult> HideTurtleExtension(MogwaiEngine engine, string word)
        {
            await Task.CompletedTask;
            ShowTurtle(false);
            return EvalResult.NoError;
        }

        private async Task<EvalResult> MoveExtension(MogwaiEngine engine, string word)
        {
            await Task.CompletedTask;

            var sign = engine.StackSign(1);

            if (sign.Count == 0)
                return EvalResult.Failure(engine, Error.TooFewArgumentsError, word);

            if (sign[0] != typeof(MOGNumber))
                return EvalResult.Failure(engine, Error.BadArgumentTypeError, word);

            var n0 = engine.StackPopNumber();
            TurtleForward(n0.IntValue);

            return EvalResult.NoError;
        }

        private async Task<EvalResult> TurnExtension(MogwaiEngine engine, string word)
        {
            await Task.CompletedTask;

            var sign = engine.StackSign(1);

            if (sign.Count == 0)
                return EvalResult.Failure(engine, Error.TooFewArgumentsError, word);

            if (sign[0] != typeof(MOGNumber))
                return EvalResult.Failure(engine, Error.BadArgumentTypeError, word);

            var n0 = engine.StackPopNumber();
            TurtleRotate(n0.Value);

            return EvalResult.NoError;
        }

        #endregion
    }
}