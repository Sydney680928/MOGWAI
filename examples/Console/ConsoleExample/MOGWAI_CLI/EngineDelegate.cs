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
using System.Net;
using Terminal.Gui;

namespace MOGWAI_CLI
{
    internal class EngineDelegate : IDelegate
    {
        private MogwaiEngine _engine;

        private string _text = string.Empty;
        private string _filename = string.Empty;

        private MenuBar _mainMenu;
        private Window _window;
        private TextView _textView;

        private string Filename
        {
            get => _filename;

            set
            {
                _filename = value ?? string.Empty;
                _window.Title = Path.GetFileName(_filename);
            }
        }

        public EngineDelegate(MogwaiEngine engine)
        {
            _engine = engine;

            _mainMenu = new MenuBar([

                new MenuBarItem("_File", [

                    new MenuItem("_New", "", () =>
                    {
                        if (SaveIfNeeded())
                        {
                            _textView!.Text = string.Empty;
                            _text = string.Empty;
                            _filename = string.Empty;
                        }
                    }),

                    new MenuItem("_Open", "", () =>
                    {
                        if (SaveIfNeeded())
                            OpenFileMenu();
                    }),

                    new MenuItem("_Save", "", () =>
                    {
                        SaveMenu();
                    }),

                    new MenuItem("_Save as...", "", () =>
                    {
                        SaveAsMenu();
                    }),

                    null, // Séparateur
        
                    new MenuItem("_Exit", "",() =>
                    {
                        if (SaveIfNeeded())
                            Application.RequestStop();
                    })
                ]),

                new MenuBarItem("_Edition", [

                    new MenuItem("_Copier", "", () => { /* Action copier */ }),

                    new MenuItem("Co_ller", "", () => { /* Action coller */ })
                ]),

                new MenuBarItem("_Aide", new MenuItem[]
                {
                    new MenuItem("_À propos", "", () =>
                    {
                        MessageBox.Query("À propos", "Mon Application v1.0", "OK");
                    })
                })
            ]);

            _textView = new TextView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                WordWrap = true,
                Text = _text
            };

            _window = new Window()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };

            _window.Add(_textView);
        }

        private void StartEditor()
        {
            _textView.ColorScheme = new ColorScheme()
            {
                Normal = new Terminal.Gui.Attribute(Color.White, Color.Black),
                Focus = new Terminal.Gui.Attribute(Color.White, Color.Black),
            };

            _window.Border = new Border()
            {
                BorderStyle = BorderStyle.Rounded,
                BorderBrush = Terminal.Gui.Color.White,
            };

            Application.Init();
            Application.Top.Add(_mainMenu);
            Application.Top.Add(_window);
            Application.Run();
            Application.Shutdown();
        }

        private void OpenFileMenu()
        {
            var openDialog = new OpenDialog("Open", "");
            openDialog.DirectoryPath = _engine.ProgramsDirectory;
            openDialog.AllowsMultipleSelection = false;

            Application.Run(openDialog);

            if (!openDialog.Canceled && openDialog.FilePaths.Count > 0)
            {
                string filename = openDialog.FilePaths[0];

                try
                {
                    _text = File.ReadAllText(filename);
                    Filename = filename;
                    _textView.Text = _text;
                    _window.Title = Path.GetFileName(_filename);
                }
                catch
                {
                    MessageBox.Query("Open", "Unabled to open the file !", "OK");
                }
            }
        }

        private bool SaveAsMenu()
        {
            var saveDialog = new SaveDialog("Save as...", "")
            {
                DirectoryPath = _engine.ProgramsDirectory,
                FilePath = Filename == string.Empty ? "new program.mog" : _filename,
                AllowedFileTypes = [".mog"]
            };

            Application.Run(saveDialog);

            if (!saveDialog.Canceled)
            {
                string? fichier = saveDialog.FilePath.ToString();

                if (fichier != null)
                {
                    string text = _textView.Text.ToString() ?? string.Empty;

                    try
                    {

                        if (File.Exists(fichier))
                        {
                            int result = MessageBox.Query("Save", "This file already exists. Save ?", "yes", "no");

                            if (result == 0)
                            {
                                // YES

                                File.WriteAllText(fichier, text);

                                Filename = fichier;
                                _text = text;

                                return true;
                            }
                        }
                        else
                        {
                            File.WriteAllText(fichier, text);

                            Filename = fichier;
                            _text = text;

                            return true;
                        }
                    }
                    catch
                    {

                    }
                }
            }

            return false;
        }

        private bool SaveMenu()
        {
            if (_filename == string.Empty)
            {
                return SaveAsMenu();
            }
            else
            {
                string? text = _textView.Text.ToString();

                if (text != null)
                {
                    try
                    {
                        File.WriteAllText(_filename, text);
                        return true;
                    }
                    catch
                    {

                    }
                }
            }

            return false;
        }

        private bool SaveIfNeeded()
        {
            // Return false if cancelled

            string? text = _textView.Text.ToString();

            if (text != null)
            {
                var t1 = _text.GetHashCode();
                var t2 = text.GetHashCode();

                if (t1 != t2)
                {
                    int result = MessageBox.Query("Save",
                        "Modifications are not saved. Save it ?",
                        "yes", "no", "cancel");

                    if (result == 0)
                    {
                        // OUI

                        return SaveMenu();
                    }
                    else if (result == 1)
                    {
                        // NON

                        return true;
                    }
                    else
                    {
                        // CANCEL

                        return false;
                    }
                }
            }

            return true;
        }


        private object _ConsoleAccessLocker = new();

        public Task ProgramStart(MogwaiEngine engine, string code)
        {
            return Task.CompletedTask;
        }

        public Task ProgramEnd(MogwaiEngine engine, EvalResult result)
        {
            return Task.CompletedTask;
        }

        public Task<EvalResult> ConsoleClearScreen(MogwaiEngine engine)
        {
            lock (_ConsoleAccessLocker)
                Console.Clear();

            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message)
        {
            lock (_ConsoleAccessLocker)
                Console.WriteLine(message);

            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> ConsolePrint(MogwaiEngine engine, string message)
        {
            lock (_ConsoleAccessLocker)
                Console.Write(message);

            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> ConsoleShow(MogwaiEngine engine)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> ConsoleHide(MogwaiEngine engine)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> ConsoleLocate(MogwaiEngine engine, int x, int y)
        {
            lock (_ConsoleAccessLocker)
                Console.SetCursorPosition(x, y);

            return Task.FromResult(EvalResult.NoError);
        }

        public Task<(EvalResult result, int x, int y)> ConsoleGetCursorPosition(MogwaiEngine engine)
        {
            var r = Console.GetCursorPosition();
            return Task.FromResult((EvalResult.NoError, r.Left, r.Top));
        }

        public Task<EvalResult> ConsoleSetForegroundColor(MogwaiEngine engine, string color)
        {
            lock (_ConsoleAccessLocker)
                switch (color.ToLower())
                {
                    case "black":
                        Console.ForegroundColor = ConsoleColor.Black;
                        break;
                    case "blue":
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;
                    case "cyan":
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case "gray":
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case "green":
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case "magenta":
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    case "red":
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case "white":
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case "yellow":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    default:
                        break;
                }

            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> ConsoleSetBackgroundColor(MogwaiEngine engine, string color)
        {
            lock (_ConsoleAccessLocker)
                switch (color.ToLower())
                {
                    case "black":
                        Console.BackgroundColor = ConsoleColor.Black;
                        break;
                    case "blue":
                        Console.BackgroundColor = ConsoleColor.Blue;
                        break;
                    case "cyan":
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        break;
                    case "gray":
                        Console.BackgroundColor = ConsoleColor.Gray;
                        break;
                    case "green":
                        Console.BackgroundColor = ConsoleColor.Green;
                        break;
                    case "magenta":
                        Console.BackgroundColor = ConsoleColor.Magenta;
                        break;
                    case "red":
                        Console.BackgroundColor = ConsoleColor.Red;
                        break;
                    case "white":
                        Console.BackgroundColor = ConsoleColor.White;
                        break;
                    case "yellow":
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        break;
                    default:
                        break;
                }

            return Task.FromResult(EvalResult.NoError);
        }

        public Task<(EvalResult result, int key)> ConsoleGetInputKey(MogwaiEngine engine)
        {
            int key = -1;

            lock (_ConsoleAccessLocker)
            {
                if (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(true);
                    key = (int)keyInfo.Key;
                }
            }

            return Task.FromResult((EvalResult.NoError, key));

        }

        public Task<(EvalResult result, string? value)> Prompt(MogwaiEngine engine, string message)
        {
            lock (_ConsoleAccessLocker)
            {
                Console.Write(message);
                var r = Console.ReadLine();
                return Task.FromResult((EvalResult.NoError, r));
            }
        }

        public string[] HostFunctions(MogwaiEngine engine) => ["?s", "run", "edit", "file.edit", "file.select"];

        public async Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word)
        {
            if (word == "?s")
            {
                if (engine.StackSize == 0)
                {
                    lock (_ConsoleAccessLocker)
                    {
                        Console.WriteLine("empty stack");
                        Console.WriteLine();
                    }
                }
                else
                {
                    var s = (int)(Math.Log10(engine.StackSize) + 1);
                    var format = new string('0', (int)s);
                    var items = engine.StackArray();

                    lock (_ConsoleAccessLocker)
                    {
                        for (int i = 0; i < items.Length; i++)
                        {
                            Console.Write((i + 1).ToString(format));
                            Console.Write(" ");
                            Console.WriteLine(items[i].ToString());
                        }

                        Console.WriteLine();
                    }
                }

                return EvalResult.NoError;
            }
            else if (word == "run")
            {
                var s = engine.StackSign(1);

                if (s.Count == 0)
                    return EvalResult.Failure(engine, Error.TooFewArgumentsError, word);

                if (s[0] == typeof(MOGString))
                {
                    var codeFile = engine.StackPop() as MOGString;

                    try
                    {
                        var bytes = File.ReadAllBytes(codeFile!.Value);
                        var result = engine.GetCodeFormBytes(bytes);

                        if (result.code != null)
                        {
                            return await engine.RunAsync(result.code, false);
                        }
                        else
                        {
                            return EvalResult.Failure(engine, Error.ParseError, word);
                        }
                    }
                    catch
                    {
                        return EvalResult.Failure(engine, Error.FileOperationError, word);
                    }
                }

                return EvalResult.Failure(engine, Error.BadArgumentTypeError, word);
            }
            else if (word == "edit")
            {
                StartEditor();
                return EvalResult.NoError;
            }
            else if (word == "file.edit")
            {
                var s = engine.StackSign(1);

                if (s.Count == 0)
                    return EvalResult.Failure(engine, Error.TooFewArgumentsError, word);

                if (s[0] == typeof(MOGString))
                {
                    var @string = engine.StackPopString();
                    var filename = string.Empty;

                    try
                    {
                        filename = Path.GetFullPath(@string.Value);

                        _text = File.ReadAllText(filename);
                        _filename = filename;

                        StartEditor();

                        return EvalResult.NoError;
                    }
                    catch
                    {
                        return EvalResult.Failure(engine, Error.FileOperationError, word, filename);
                    }
                }

                return EvalResult.Failure(engine, Error.BadArgumentTypeError, word);
            }
            else if (word == "file.select")
            {
                Application.Init();

                var openDialog = new OpenDialog("Open", "");
                openDialog.DirectoryPath = _engine.ProgramsDirectory;
                openDialog.AllowsMultipleSelection = false;

                Application.Run(openDialog);

                if (!openDialog.Canceled && openDialog.FilePaths.Count > 0)
                {
                    string filename = openDialog.FilePaths[0];
                    engine.StackPushString(filename);

                    Application.Shutdown();

                    return EvalResult.NoError;
                }

                engine.StackPush(new MOGNull(engine));

                Application.Shutdown();

                return EvalResult.NoError;
            }

            return EvalResult.NoExternalFunction;
        }

        public Task<EvalResult> MessageReceivedFromRuntime(MogwaiEngine engine, string message, MOGObject parameter)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> EngineDidPause(MogwaiEngine engine)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> EngineDidResume(MogwaiEngine engine)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> StudioDidConnect(MogwaiEngine engine)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> StudioDidDisconnect(MogwaiEngine engine)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> SocketServerDidStart(MogwaiEngine engine, IPAddress address, int port)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> SocketServerDidStop(MogwaiEngine engine)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> DebugMessage(MogwaiEngine engine, string message)
        {
            return Task.FromResult(EvalResult.NoError);
        }

        public Task<EvalResult> DebugClear(MogwaiEngine engine)
        {
            return Task.FromResult(EvalResult.NoError);
        }
    }
}
