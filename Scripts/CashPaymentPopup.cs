using Godot;
using System;

public partial class CashPaymentPopup : Control
{
    [Export] RecieptConfirmPopup recieptConfirmPopup;
    [Export] SpinBox cashGiven;
    [Export] Label cashReturn;
    [Export] Keypad keypad;

    decimal total;

    public void _OnStartButtonClick()
    {
        cashGiven.Value = 0;
        keypad.Clear();
        this.total = Global.Round(Global.total);
        cashReturn.Text = $"Change: {0:C}";
        Global.OpenCashDrawer();
        Show();
    }

    public void _OnValueChanged(float newValue)
    {
        decimal change = Math.Max(Convert.ToDecimal(newValue) - total, 0);
        cashReturn.Text = $"Change: {change:C}";
    }

    public void _OnConfirmPressed()
    {
        Close();

        Global.AddCustomerPurchase(true, 1);
        recieptConfirmPopup.Start();
    }

    public void _OnClosePressed()
    {
        Close();
    }

    void Close()
    {
        Hide();
    }
}

