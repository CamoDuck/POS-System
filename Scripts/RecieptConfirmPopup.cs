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
        Global.DeleteAllItems();
    }

    void _OnNoPressed()
    {
        Close();
        Global.DeleteAllItems();
    }

}
