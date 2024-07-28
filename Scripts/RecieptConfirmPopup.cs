using Godot;
using System;

public partial class RecieptConfirmPopup : Control
{

    public void Start()
    {
        Show();
    }

    void Close()
    {
        Hide();
    }

    void _OnYesPressed()
    {
        Close();
        Global.PrintReceipt();
    }

    void _OnNoPressed()
    {
        Close();
    }

}
