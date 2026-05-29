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