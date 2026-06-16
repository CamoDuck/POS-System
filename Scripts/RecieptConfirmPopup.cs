using Godot;
using System;

public partial class ConfirmPopup : Control
{
    [Export] Label contentLabel;

    Action YesCallback;
    Action NoCallback;

    public void Start(string content, Action yesCallback, Action noCallback)
    {
        YesCallback = yesCallback;
        NoCallback = noCallback;
        contentLabel.Text = content;
        Show();
    }

    void Close()
    {
        Hide();
    }

    void _OnYesPressed()
    {
        Close();
        YesCallback?.Invoke();
    }

    void _OnNoPressed()
    {
        Close();
        NoCallback?.Invoke();
    }

}
