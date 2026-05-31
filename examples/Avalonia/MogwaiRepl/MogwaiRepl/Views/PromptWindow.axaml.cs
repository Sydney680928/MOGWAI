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

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MogwaiRepl.Views;

public partial class PromptWindow : Window
{
    public string? Result { get; private set; }

    public PromptWindow()
    {
        InitializeComponent();
    }

    public PromptWindow(string message) : this()
    {
        MessageLabel.Text = message;
        OkButton.Click += OkButton_Click;
        InputBox.KeyDown += InputBox_KeyDown;
        Opened += (_, _) => InputBox.Focus();
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
        => Close(InputBox.Text);

    private void InputBox_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Return)
            Close(InputBox.Text);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Result = null;
    }
}