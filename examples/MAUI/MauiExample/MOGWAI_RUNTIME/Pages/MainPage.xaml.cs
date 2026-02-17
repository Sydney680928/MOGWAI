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
using MOGWAI_RUNTIME.Classes;
using System.Net;

namespace MOGWAI_RUNTIME.Pages
{
    public partial class MainPage : ContentPage, IDelegate
    {
        private const string SCRIPT_EDITOR_FONT_SIZE = "ScriptEditorFontSize";
        private const string SCRIPT_RUN_FONT_SIZE = "ScriptRunFontSize";
        private const string SCRIPT_RUN_SCREEN_WIDTH = "ScriptRunScreenWidth";
        private const string SCRIPT_RUN_SCREEN_HEIGHT = "ScriptRunScreenHeight";

        private MogwaiEngine _engine;
        private bool _debugMode;
        private string _filename = "NO NAME";
        private bool _codeIsSaved;
        private bool _codeIsModified;

        private string _fullPath => Path.Combine(AppGlobal.CodeFolder, _filename) + ".mog";

        public int RunFontSize
        {
            get => Preferences.Default.Get(SCRIPT_RUN_FONT_SIZE, 8);

            set
            {
                Preferences.Default.Set(SCRIPT_RUN_FONT_SIZE, value);
                OnPropertyChanged(nameof(RunFontSize));
            }
        }

        public int EditorFontSize
        {
            get => Preferences.Default.Get(SCRIPT_EDITOR_FONT_SIZE, 10);

            set => Preferences.Default.Set(SCRIPT_EDITOR_FONT_SIZE, value);
        }

        public int RunScreenWidth
        {
            get => Preferences.Default.Get(SCRIPT_RUN_SCREEN_WIDTH, 60);

            set => Preferences.Default.Set(SCRIPT_RUN_SCREEN_WIDTH, value);
        }

        public int RunScreenHeight
        {
            get => Preferences.Default.Get(SCRIPT_RUN_SCREEN_HEIGHT, 30);

            set => Preferences.Default.Set(SCRIPT_RUN_SCREEN_HEIGHT, value);
        }

        public MainPage()
        {
            InitializeComponent();

            // Create MOGWAI engine

            _engine = new("MOGWAI RT", true, false);
            _engine.Delegate = this;

            // Output initialize

            var htmlSource = new HtmlWebViewSource
            {
                Html = Tools.GetStringFromResource("ConsoleWebView.html")
            };

            OutputDisplay.Source = htmlSource;
        }

        private void ShowCodeEditorScreen()
        {
            _debugMode = false;

            // Auto power off -> ON

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                _engine.StopDatagramServer();
                await _engine.StopSocketServer();

                FlagRunPath.IsVisible = false;
                FlagDebugPath.IsVisible = false;
                FlagErrorPath.IsVisible = false;
                FlagPlugPath.IsVisible = false;
                FlagPausePath.IsVisible = false;

                CodeEditorGrid.IsVisible = true;
                RunGrid.IsVisible = false;
            });

#if ANDROID

            if (Platform.CurrentActivity is MainActivity activity)
            {
                activity.LeaveScreenOff();
            }

#endif

        }

        private async Task ShowRunScreenAsync()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (_debugMode)
                {
                    FlagDebugPath.IsVisible = true;
                }
                else
                {
                    FlagDebugPath.IsVisible = false;
                }

                FlagRunPath.IsVisible = _engine.IsRunning;
                FlagErrorPath.IsVisible = false;
                FlagPlugPath.IsVisible = _engine.IsSocketServerRunning;
                FlagPausePath.IsVisible = _engine.IsPaused;

                await OutputDisplay.EvaluateJavaScriptAsync($"setSize({RunFontSize});");

                CodeEditorGrid.IsVisible = false;
                RunGrid.IsVisible = true;
            });
        }

        private async Task ConsoleClearScreenAsync()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await OutputDisplay.EvaluateJavaScriptAsync("consoleClearScreen();");
            });
        }

        private async Task SaveAs()
        {
            var r = await DisplayPromptAsync("Save as...", "Please enter the name of the script", "OK", "CANCEL", null, -1, null, _filename);

            if (!string.IsNullOrEmpty(r))
            {
                var path = System.IO.Path.Combine(AppGlobal.CodeFolder, r);
                if (!path.ToUpper().EndsWith(".mog")) path += ".mog";

                if (File.Exists(path))
                {
                    var r1 = await DisplayAlert("Save as...", $"The script '{r}' already exists.\nDo you want to replace it?", "YES", "NO");
                    if (!r1) return;
                }

                _filename = r;
                FilenameLabel.Text = _filename;

                _codeIsSaved = true;

                await Save();
            }
        }

        private async Task Save()
        {
            if (!_codeIsSaved)
            {
                await SaveAs();
            }
            else
            {
                try
                {
                    using var writer = new StreamWriter(_fullPath);
                    writer.Write(CodeEditor.Text);
                    writer.Flush();
                    writer.Close();

                    _codeIsSaved = true;
                    _codeIsModified = false;
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Save Script", $"Unable to save the script !\n\n{ex.Message}", "OK");
                }
            }
        }

        protected override bool OnBackButtonPressed()
        {
            if (RunGrid.IsVisible)
            {
                // If a program is running, we stop it

                if (_engine.IsRunning)
                {
                    _engine.Halt();
                }

                // We switch back to editor mode

                ShowCodeEditorScreen();

                return true;
            }

            return base.OnBackButtonPressed();
        }

        private async void ContentView_Loaded(object sender, EventArgs e)
        {
            if (!AppGlobal.CreateDataStructure())
            {
                await DisplayAlert("MOGWAI RUNTIME", "Unable to create the structure required for the application !", "OK");
                return;
            }

            NavigationPage.SetHasNavigationBar(this, false);

            FilenameLabel.Text = _filename;

            // We set the editor font size

            CodeEditor.FontSize = EditorFontSize;

            // By default we display the code editor unless code is running

            if (_engine.IsRunning || _engine.IsPaused)
            {
                await ShowRunScreenAsync();
            }
            else
            {
                ShowCodeEditorScreen();
            }

#if WINDOWS

            await DisplayAlert("MOGWAI RUNTIME", "In Windows, to open the [script] and [files] menus you must right-click.", "OK");
#endif
        }

        private async Task<bool> OpenFileAsync()
        {
            if (await CheckIfSaveIsRequested()) return false;

            var f = await SelectScripFile("Open Script");

            if (f != null)
            {
                try
                {
                    var fullpath = System.IO.Path.Combine(AppGlobal.CodeFolder, f) + ".mog";
                    using var reader = new StreamReader(fullpath);
                    var code = reader.ReadToEnd();
                    reader.Close();

                    _filename = f;
                    _codeIsSaved = true;

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        CodeEditor.Text = code;
                        _codeIsModified = false;
                        FilenameLabel.Text = _filename;
                    });

                    return true;
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Open Script", $"Unable to load this script !\n\n{ex.Message}", "OK");
                }
            }

            return false;
        }

        private async Task<bool> CheckIfSaveIsRequested()
        {
            if (_codeIsModified)
            {
                var r = await DisplayAlert("Script", "The code has been modified and has not been saved.\n\nDo you still want to continue?", "CONTINUE (!)", "CANCEL");
                return !r;
            }

            return false;
        }

        private async Task<string?> SelectScripFile(string title)
        {
            // We list the .mog files from the code scripts folder

            var files = Directory.GetFiles(AppGlobal.CodeFolder, "*.mog");

            // We keep only the file names without the full path

            var items = new List<string>();
            foreach (var file in files)
            {
                items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
            }

            var d = new BasicSelectorPage(title, items);
            await Navigation.PushModalAsync(d);

            while (!d.Done)
            {
                await Task.Delay(500);
            }

            return d.SelectedItem;
        }

        private async Task<string?> SelectDataFile(string title)
        {
            // We list the files from the scripts data folder

            var files = Directory.GetFiles(AppGlobal.DataFolder);

            // We keep only the file names without the full path

            var items = new List<string>();
            foreach (var file in files)
            {
                items.Add(System.IO.Path.GetFileName(file));
            }

            var d = new BasicSelectorPage(title, items);
            await Navigation.PushModalAsync(d);

            while (!d.Done)
            {
                await Task.Delay(500);
            }

            return d.SelectedItem;
        }

        private async Task<string?> SelectFile(string title)
        {
            var options = new PickOptions
            {
                PickerTitle = title
            };

            var result = await FilePicker.Default.PickAsync(options);
            return result?.FullPath ?? null;
        }

        private async Task ConsoleWriteAsync(string message)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (!string.IsNullOrEmpty(message))
                {
                    var lines = message
                    .Replace("\r", "")
                    .Replace(" ", @"\xa0")
                    .Split("\n");

                    for (int i = 0; i < lines.Length - 1; i++)
                    {
                        await OutputDisplay.EvaluateJavaScriptAsync($"consoleWriteLine(`{lines[i]}`);");
                    }

                    await OutputDisplay.EvaluateJavaScriptAsync($"consoleWrite(`{lines[lines.Length - 1]}`);");
                }
            });
        }

        private async Task ConsoleWriteLineAsync(string message)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (!string.IsNullOrEmpty(message))
                {
                    var lines = message
                    .Replace("\r", "")
                    .Replace(" ", @"\xa0")
                    .Split("\n");

                    foreach (var line in lines)
                    {
                        await OutputDisplay.EvaluateJavaScriptAsync($"consoleWriteLine(`{line}`);");
                    }
                }
                else
                {
                    await OutputDisplay.EvaluateJavaScriptAsync("consoleWriteLine(``);");
                }
            });
        }

        private async Task ConsoleStartInputMode()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await OutputDisplay.EvaluateJavaScriptAsync("startInputMode();");
            });
        }

        private async Task ConsoleExitInputMode()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await OutputDisplay.EvaluateJavaScriptAsync("exitInputMode();");
            });
        }

        private async Task<string> ConsolePrompt(string prompt)
        {
            await ConsoleWriteAsync(prompt);
            return await ConsoleInput();
        }

        private async Task<string> ConsoleInput()
        {
            return await MainThread.InvokeOnMainThreadAsync<string>(async () =>
            {
                OutputDisplay.Focus();

                await ConsoleStartInputMode();

                while (true)
                {
                    var r = await OutputDisplay.EvaluateJavaScriptAsync("inputModeInProgress");

                    if (r != null && r == "true")
                    {
                        break;
                    }
                }

                while (true)
                {
                    var r = await OutputDisplay.EvaluateJavaScriptAsync("inputModeInProgress");

                    if (r != null && r == "false")
                    {
                        // Input completed.
                        // We exit

                        break;
                    }

                    await Task.Delay(500);
                }

                // We retrieve the entered value

                var v = await OutputDisplay.EvaluateJavaScriptAsync("lastInputValue");
                return v ?? "";
            });
        }

        private async void NewFileMenu_ItemTapped(Plugin.ContextMenuContainer.ContextMenuItem item)
        {
            if (!await CheckIfSaveIsRequested())
            {
                _filename = "NO NAME";
                _codeIsSaved = false;
                CodeEditor.Text = string.Empty;
                _codeIsModified = false;
                FilenameLabel.Text = _filename;
                CodeEditor.Focus();
            }
        }

        private async void OpenFileMenu_ItemTapped(Plugin.ContextMenuContainer.ContextMenuItem item)
        {
            await OpenFileAsync();
        }

        private async void SaveFileMenu_ItemTapped(Plugin.ContextMenuContainer.ContextMenuItem item)
        {
            await Save();
        }

        private async void SaveAsFileMenu_ItemTapped(Plugin.ContextMenuContainer.ContextMenuItem item)
        {
            await SaveAs();
        }

        private async void RenameFileMenu_ItemTapped(Plugin.ContextMenuContainer.ContextMenuItem item)
        {
            var f = await SelectScripFile("Rename Script");

            if (f != null)
            {
                if (f == _filename)
                {
                    await DisplayAlert("Rename Script", "You cannot rename the script that is currently being edited!", "OK");
                    return;
                }

                var r = await DisplayPromptAsync("Renommer Script", $"What new name do you want to give the script '{f}' ?", "RENAME", "CANCEL", null, -1, null, f);

                if (!string.IsNullOrEmpty(r))
                {
                    try
                    {
                        var oldName = System.IO.Path.Combine(AppGlobal.CodeFolder, f) + ".mog";
                        var newName = System.IO.Path.Combine(AppGlobal.CodeFolder, r) + ".mog";

                        File.Move(oldName, newName);

                        await DisplayAlert("Rename Script", "Script renamed.", "OK");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Rename Script", $"Unable to rename this script!\n\n{ex.Message}", "OK");
                    }
                }
            }
        }

        private async void DeleteFileMenu_ItemTapped(Plugin.ContextMenuContainer.ContextMenuItem item)
        {
            var f = await SelectScripFile("Delete Script");

            if (f != null)
            {
                if (f == _filename)
                {
                    await DisplayAlert("Delete Script", "You cannot delete the script that is currently being edited!", "OK");
                    return;
                }

                var r = await DisplayAlert("Delete Script", $"Are you sure you want to delete the script '{f}'?", "DELETE", "CANCEL");

                if (r)
                {
                    var path = System.IO.Path.Combine(AppGlobal.CodeFolder, f) + ".mog";

                    try
                    {
                        File.Delete(path);
                        await DisplayAlert("Delete Script", "Script deleted.", "OK");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Delete Script", $"Unable to delete this script!\n\n{ex.Message}", "OK");
                    }
                }
            }
        }

        private async void ShareFileMenu_ItemTapped(Plugin.ContextMenuContainer.ContextMenuItem item)
        {
            if (await CheckIfSaveIsRequested()) return;

            var f = await SelectScripFile("Share Script");

            if (f != null)
            {
                var path = System.IO.Path.Combine(AppGlobal.CodeFolder, f) + ".mog";

                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = $"Share le script {f}",
                    File = new ShareFile(path)
                });
            }
        }

        private async void ImportFileMenu_ItemTapped(Plugin.ContextMenuContainer.ContextMenuItem item)
        {
            if (await CheckIfSaveIsRequested()) return;

            var f = await SelectFile("Import Script");

            if (f != null)
            {
                // The file must end with .mog

                if (!System.IO.Path.GetFileName(f).ToUpper().EndsWith(".MOG"))
                {
                    await DisplayAlert("Import Script", "You must select a valid script (.mog)!", "OK");
                    return;
                }

                var filename = System.IO.Path.GetFileName(f);
                var destination = System.IO.Path.Combine(AppGlobal.CodeFolder, filename);

                if (File.Exists(destination))
                {
                    var r = await DisplayAlert("Importer Script", $"This script already exists. Do you want to replace it?", "REPLACE", "CANCEL");
                    if (!r) return;
                }

                try
                {
                    File.Copy(f, destination, true);
                    await DisplayAlert("Import Script", "Script importé.", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Import Script", $"Unable to import this script!\n\n{ex.Message}", "OK");
                }
            }
        }

        private async void ShareDataFileMenu_ItemTapped(Plugin.ContextMenuContainer.ContextMenuItem item)
        {
            var f = await SelectDataFile("Share File");

            if (f != null)
            {
                var path = System.IO.Path.Combine(AppGlobal.DataFolder, f);

                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = $"Share the file {f}",
                    File = new ShareFile(path)
                });
            }
        }

        private async void ImportDataFileMenu_ItemTapped(Plugin.ContextMenuContainer.ContextMenuItem item)
        {
            var f = await SelectFile("Import File");

            if (f != null)
            {
                var filename = System.IO.Path.GetFileName(f);
                var destination = System.IO.Path.Combine(AppGlobal.DataFolder, filename);

                if (File.Exists(destination))
                {
                    var r = await DisplayAlert("Import File", $"This file already exists. Do you want to replace it?", "REPLACE", "CANCEL");
                    if (!r) return;
                }

                try
                {
                    File.Copy(f, destination, true);
                    await DisplayAlert("Import File", "File imported.", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Import File", $"Unable to import this file!\n\n{ex.Message}", "OK");
                }
            }
        }

        private async void RenameDataFileMenu_ItemTapped(Plugin.ContextMenuContainer.ContextMenuItem item)
        {
            var f = await SelectDataFile("Rename File");

            if (f != null)
            {
                var r = await DisplayPromptAsync("Rename File", $"What new name do you want to give the file '{f}'?", "RENAME", "CANCEL", null, -1, null, f);

                if (!string.IsNullOrEmpty(r))
                {
                    try
                    {
                        var oldName = System.IO.Path.Combine(AppGlobal.DataFolder, f);
                        var newName = System.IO.Path.Combine(AppGlobal.DataFolder, r);

                        File.Move(oldName, newName);

                        await DisplayAlert("Rename File", "File renamed.", "OK");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Rename File", $"Unable to rename this file!\n\n{ex.Message}", "OK");
                    }
                }
            }
        }

        private async void DeleteDataFileMenu_ItemTapped(Plugin.ContextMenuContainer.ContextMenuItem item)
        {
            var f = await SelectDataFile("Delete File");

            if (f != null)
            {
                var r = await DisplayAlert("Delete File", $"Are you sure you want to delete the file '{f}'?", "DELETE", "CANCEL");

                if (r)
                {
                    var path = System.IO.Path.Combine(AppGlobal.DataFolder, f);

                    try
                    {
                        File.Delete(path);
                        await DisplayAlert("Delete File", "File deleted.", "OK");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Delete File", $"Unable to delete this file!\n\n{ex.Message}", "OK");
                    }
                }
            }
        }

        private void CodeEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            _codeIsModified = true;
        }

        private async void RunTapGesture_Tapped(object sender, TappedEventArgs e)
        {
            if (await CheckIfSaveIsRequested()) return;

            _debugMode = false;

            await ShowRunScreenAsync();

            _ = _engine.RunAsync(CodeEditor.Text, false);
        }

        private async void DebugTapGesture_Tapped(object sender, TappedEventArgs e)
        {
            if (await CheckIfSaveIsRequested()) return;

            _debugMode = true;

            await ShowRunScreenAsync();

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                _engine.StartDatagramServer(1968);
                await _engine.StartSocketServerAsync(IPAddress.Any.ToString());
            });
        }

        private async void HaltTapGesture_Tapped(object sender, TappedEventArgs e)
        {
            await ConsoleExitInputMode();
            _engine.Halt();
        }

        private async void FontPlusTapGesture_Tapped(object sender, TappedEventArgs e)
        {
            if (CodeEditorGrid.IsVisible)
            {
                if (EditorFontSize < 30)
                {
                    EditorFontSize += 1;
                    CodeEditor.FontSize = EditorFontSize;
                }
            }
            else
            {
                if (RunFontSize < 24)
                {
                    RunFontSize += 1;
                    await OutputDisplay.EvaluateJavaScriptAsync($"setSize({RunFontSize});");
                }
            }
        }

        private async void FontMinusGesture_Tapped(object sender, TappedEventArgs e)
        {
            if (CodeEditorGrid.IsVisible)
            {
                if (EditorFontSize > 10)
                {
                    EditorFontSize -= 1;
                    CodeEditor.FontSize = EditorFontSize;
                }
            }
            else
            {
                if (RunFontSize > 8)
                {
                    RunFontSize -= 1;
                    await OutputDisplay.EvaluateJavaScriptAsync($"setSize({RunFontSize});");
                }
            }
        }

        private void BackGesture_Tapped(object sender, TappedEventArgs e)
        {
            // If a program is running, we stop it

            if (_engine.IsRunning)
            {
                _engine.Halt();
            }

            // We switch back to editor mode

            ShowCodeEditorScreen();
        }

        private async void OutputDisplay_Unfocused(object sender, FocusEventArgs e)
        {
            var r = await OutputDisplay.EvaluateJavaScriptAsync("inputModeInProgress");

            if (r == "true")
            {
                OutputDisplay.Focus();
            }
        }


        #region MOGWAI DELEGATE

        public async Task ProgramStart(MogwaiEngine engine, string code)
        {
            // Called when program starts

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                CodeEditor.Text = code;
                FlagRunPath.IsVisible = true;
                FlagErrorPath.IsVisible = false;
            });

            // Auto power off -> OFF

            Tools.SuspendAutoPowerOff();
        }

        public async Task ProgramEnd(MogwaiEngine engine, EvalResult result)
        {
            // Called when program ends
            // result parameter contains status (ok or error)

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                FlagRunPath.IsVisible = false;
                FlagErrorPath.IsVisible = result != EvalResult.NoError;

                await ConsoleWriteLineAsync("");
                await ConsoleWriteLineAsync(result.ToString());
                await ConsoleWriteLineAsync(" ");

                Tools.ResumeAutoPowerOff();
            });
        }

        public async Task<EvalResult> ConsoleClearScreen(MogwaiEngine engine)
        {
            // MOGWAI console.clear function

            await ConsoleClearScreenAsync();
            return EvalResult.NoError;
        }

        public async Task<EvalResult> ConsolePrintLn(MogwaiEngine engine, string message)
        {
            await ConsoleWriteLineAsync(message);
            return EvalResult.NoError;
        }

        public async Task<EvalResult> ConsolePrint(MogwaiEngine engine, string message)
        {
            await ConsoleWriteAsync(message);
            return EvalResult.NoError;
        }

        public async Task<EvalResult> ConsoleShow(MogwaiEngine engine)
        {
            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<EvalResult> ConsoleHide(MogwaiEngine engine)
        {
            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<EvalResult> ConsoleLocate(MogwaiEngine engine, int x, int y)
        {
            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<(EvalResult result, int x, int y)> ConsoleGetCursorPosition(MogwaiEngine engine)
        {
            await Task.CompletedTask;
            return (EvalResult.NoError, 0, 0);
        }

        public async Task<EvalResult> ConsoleSetForegroundColor(MogwaiEngine engine, string color)
        {
            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<EvalResult> ConsoleSetBackgroundColor(MogwaiEngine engine, string color)
        {
            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<(EvalResult result, int key)> ConsoleGetInputKey(MogwaiEngine engine)
        {
            await Task.CompletedTask;
            return (EvalResult.NoError, 0);
        }

        public async Task<(EvalResult result, string? value)> Prompt(MogwaiEngine engine, string message)
        {
            var v = await ConsolePrompt(message);
            return (EvalResult.NoError, v);
        }

        public string[] HostFunctions(MogwaiEngine engine) => [];

        public async Task<EvalResult> ExecuteHostFunction(MogwaiEngine engine, string word)
        {
            await Task.CompletedTask;
            return EvalResult.NoExternalFunction;
        }

        public async Task<EvalResult> MessageReceivedFromRuntime(MogwaiEngine engine, string message, MOGObject parameter)
        {
            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<EvalResult> DebugMessage(MogwaiEngine engine, string message)
        {
            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<EvalResult> DebugClear(MogwaiEngine engine)
        {
            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<EvalResult> EngineDidPause(MogwaiEngine engine)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                FlagPausePath.IsVisible = true;
                FlagRunPath.IsVisible = false;
            });

            return EvalResult.NoError;
        }

        public async Task<EvalResult> EngineDidResume(MogwaiEngine engine)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                FlagPausePath.IsVisible = false;
                FlagRunPath.IsVisible = true;
            });

            return EvalResult.NoError;
        }

        public async Task<EvalResult> StudioDidConnect(MogwaiEngine engine)
        {
            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<EvalResult> StudioDidDisconnect(MogwaiEngine engine)
        {
            await Task.CompletedTask;
            return EvalResult.NoError;
        }

        public async Task<EvalResult> SocketServerDidStart(MogwaiEngine engine, IPAddress address, int port)
        {
            // Called when debug server starts

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                FlagPlugPath.IsVisible = true;
            });

            return EvalResult.NoError;
        }

        public async Task<EvalResult> SocketServerDidStop(MogwaiEngine engine)
        {
            // We switch back to editor mode

            await MainThread.InvokeOnMainThreadAsync(ShowCodeEditorScreen);
            return EvalResult.NoError;
        }

        #endregion

    }
}