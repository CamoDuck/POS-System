using Godot;
using System;

public partial class CashPaymentPopup : Control
{
    [Export] RecieptConfirmPopup recieptConfirmPopup;
    [Export] SpinBox cashGiven;
    [Export] Label cashReturn;
    [Export] Keypad keypad;

    decimal total;
    bool cashDrawerOpened;

    public void _OnStartButtonClick()
    {
        cashDrawerOpened = false;
        cashGiven.Value = 0;
        keypad.Clear();
        this.total = Global.total;
        cashReturn.Text = $"Change: {0:C}";
        Show();
    }

    public void _OnValueChanged(float newValue)
    {
        decimal change = Math.Max(Convert.ToDecimal(newValue) - total, 0);
        cashReturn.Text = $"Change: {change:C}";
        if (cashDrawerOpened) {
            Global.OpenCashDrawer();
            cashDrawerOpened = true;
        }

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

