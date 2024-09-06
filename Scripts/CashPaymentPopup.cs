using Godot;
using System;

public partial class CashPaymentPopup : Control
{
    [Export] RecieptConfirmPopup recieptConfirmPopup;
    [Export] Label totalLabel;
    [Export] SpinBox cashGiven;
    [Export] Label cashReturn;

    decimal total;

    public void _OnStartButtonClick()
    {
        this.total = Global.total;
        totalLabel.Text = $"Total: {total:C}";
        Show();
    }

    public void _OnValueChanged(float newValue)
    {
        decimal change = total - Convert.ToDecimal(newValue);
        cashReturn.Text = $"Change: {change:C}";
    }

    public void _OnConfirmPressed()
    {
        cashGiven.Value = 0;
        Close();

        Global.AddCustomerPurchase(true, 1);
        Global.OpenCashDrawer();
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

