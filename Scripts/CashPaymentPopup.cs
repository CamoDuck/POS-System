using Godot;
using System;

public partial class CashPaymentPopup : Control
{
	[Export] ConfirmPopup confirmPopup;
	[Export] SpinBox cashGiven;
	[Export] Label cashReturn;
	[Export] Keypad keypad;

	decimal total;

	public void _OnStartButtonClick()
	{
		confirmPopup.Start($"Total items: [color=red]{Global.GetTotalItemCount()}[/color]\nContinue to cash payment?", StartCashPayment, null);
	}
	
	public void StartCashPayment()
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
		confirmPopup.Start("Print Receipt?", () =>
		{
			Global.PrintReceipt();
			Global.DeleteAllItems();
		}, Global.DeleteAllItems);
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
