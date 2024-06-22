using Godot;
using System;
using System.Text.RegularExpressions;

public partial class EnterProductPopup : Panel
{
    [Signal] public delegate void ClosedPopupEventHandler();

    [Export] LineEdit NameLineEdit;
    [Export] LineEdit PriceLineEdit;
    [Export] CheckBox GSTBox;
    [Export] CheckBox PSTBox;
    [Export] CheckBox EnviromentalFeeBox;
    [Export] CheckBox BottleDepositBox;
    [Export] Button CreateButton;

    [Export] StyleBoxFlat ErrorStyle;

    bool isNameValid;
    bool isPriceValid;

    bool isCreateButtonPressed;

    public void Start()
    {
        NameLineEdit.Clear();
        PriceLineEdit.Clear();
        GSTBox.ButtonPressed = false;
        PSTBox.ButtonPressed = false;
        EnviromentalFeeBox.ButtonPressed = false;
        BottleDepositBox.ButtonPressed = false;

        isNameValid = false;
        isPriceValid = false;
        isCreateButtonPressed = false;
        Show();
    }

    public void CloseIfOpen()
    {
        Hide();
    }

    public void _OnNameLineEditChanged(string newText)
    {
        const int maxLength = 50;

        bool isLengthValid = PriceLineEdit.Text.Length <= maxLength;
        isNameValid = isLengthValid;

        if (isLengthValid)
        {
            NameLineEdit.RemoveThemeStyleboxOverride("normal");
        }
        else
        {
            NameLineEdit.AddThemeStyleboxOverride("normal", ErrorStyle);
        }

        UpdateCreateButton();
    }


    public void _OnPriceLineEditChanged(string newText)
    {
        const decimal minValue = 0.10m;
        const decimal maxValue = 500.00m;
        const string regex = @"^[+-]?[0-9]{1,3}(?:,?[0-9]{3})*(?:\.[0-9]{2})?$";

        decimal value;
        bool isDecimal = decimal.TryParse(PriceLineEdit.Text, out value);
        bool isRangeValid = value > minValue && value <= maxValue;
        bool isFormatValid = Regex.IsMatch(PriceLineEdit.Text, regex);

        isPriceValid = isDecimal && isRangeValid && isFormatValid;

        if (isPriceValid)
        {
            PriceLineEdit.RemoveThemeStyleboxOverride("normal");
        }
        else
        {
            PriceLineEdit.AddThemeStyleboxOverride("normal", ErrorStyle);
        }

        UpdateCreateButton();
    }

    public void _OnCreateButtonPressed()
    {
        Hide();
        isCreateButtonPressed = true;
        EmitSignal(SignalName.ClosedPopup);
    }

    public void _OnCancelButtonPressed()
    {
        Hide();
        isCreateButtonPressed = false;
        EmitSignal(SignalName.ClosedPopup);
    }

    void UpdateCreateButton()
    {
        CreateButton.Disabled = !(isNameValid && isPriceValid);
    }

    public object[] GetValues()
    {
        if (!isCreateButtonPressed)
        {
            return null;
        }

        string name = NameLineEdit.Text;
        double price = double.Parse(PriceLineEdit.Text);
        bool gst = GSTBox.ButtonPressed;
        bool pst = PSTBox.ButtonPressed;
        bool enviromentalFee = EnviromentalFeeBox.ButtonPressed;
        bool bottleDepositFee = BottleDepositBox.ButtonPressed;

        object[] values = { null, name, price, null, null, gst, pst, enviromentalFee, bottleDepositFee };
        return values;
    }

}
