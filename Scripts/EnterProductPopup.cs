using Godot;
using System;
using System.Text.RegularExpressions;

public partial class EnterProductPopup : Panel
{
    [Signal] public delegate void CreatedEventHandler(string name, double price, int quantity, string type, bool gst, bool pst, bool environmentalFee, bool bottleDepositFee);
    [Signal] public delegate void CanceledEventHandler();

    [Export] LineEdit NameLineEdit;
    [Export] LineEdit PriceLineEdit;
    [Export] CheckBox GSTBox;
    [Export] CheckBox PSTBox;
    [Export] CheckBox EnviromentalFeeBox;
    [Export] CheckBox BottleDepositBox;
    [Export] Button CreateButton;

    [Export] StyleBoxFlat ErrorStyle;

    bool isNameValid = false;
    bool isPriceValid = false;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        UpdateCreateButton();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void _OnNameLineEditChanged(string newText)
    {
        bool isLengthValid = PriceLineEdit.Text.Length <= 50;
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
        decimal minValue = 0.10m;
        decimal maxValue = 500.00m;
        string regex = @"^[+-]?[0-9]{1,3}(?:,?[0-9]{3})*(?:\.[0-9]{2})?$";

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
        string name = NameLineEdit.Text;
        double price = double.Parse(PriceLineEdit.Text);
        bool gst = GSTBox.ButtonPressed;
        bool pst = PSTBox.ButtonPressed;
        bool enviromentalFee = EnviromentalFeeBox.ButtonPressed;
        bool bottleDepositFee = BottleDepositBox.ButtonPressed;
        EmitSignal(SignalName.Created, name, price, -1, "", gst, pst, enviromentalFee, bottleDepositFee);
        Hide();
    }

    public void _OnCancelButtonPressed()
    {
        Hide();
        EmitSignal(SignalName.Canceled);
    }


    void UpdateCreateButton()
    {
        CreateButton.Disabled = !(isNameValid && isPriceValid);
    }

}
