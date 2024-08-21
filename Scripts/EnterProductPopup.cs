using Godot;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public partial class EnterProductPopup : Panel
{
    [Signal] public delegate void ClosedPopupEventHandler();

    [Export] LineEdit NameLineEdit;
    [Export] LineEdit PriceLineEdit;
    [Export] OptionButton ItemTypes;
    [Export] CheckBox GSTBox;
    [Export] CheckBox PSTBox;
    [Export] CheckBox EnviromentalFeeBox;
    [Export] CheckBox BottleDepositBox;
    [Export] Button CreateButton;
    [Export] SpinBox RealPriceLabel;
    [Export] CheckButton PriceMultipler;
    [Export] SpinBox Quantity;

    [Export] StyleBoxFlat ErrorStyle;

    bool isNameValid;
    bool isPriceValid;

    bool isCreateButtonPressed;

    // double wait = 2;
    // public override void _Process(double delta)
    // {
    //     wait -= delta;
    //     if (wait > 0 && wait < 1)
    //     {
    //         wait = -1;
    //         Start();
    //     }
    // }

    public void Start()
    {
        NameLineEdit.Clear();
        PriceLineEdit.Clear();
        UpdateItemTypes();
        PriceMultipler.Text = $"{Global.priceMarkup}x";
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

    void UpdateItemTypes()
    {
        ItemTypes.Clear();
        ItemTypes.AddItem("Choose a catagory", 0);
        var list = Global.GetProductTypes();
        if (list == null) { return; }

        foreach (string type in list)
        {
            ItemTypes.AddItem(type);
        }

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

        UpdateRealPrice();
        UpdateCreateButton();
    }

    public void UpdateRealPrice()
    {
        decimal value;
        bool isDecimal = decimal.TryParse(PriceLineEdit.Text, out value);
        if (!isDecimal) { return; }

        if (PriceMultipler.ButtonPressed)
        {
            value = Global.ApplyMarkupAndRound(value);
        }

        RealPriceLabel.Value = (double)value;
    }

    public void _OnQuantityValueChanged(float value)
    {
        UpdateCreateButton();
    }

    public void _OnItemTypesChanged(int id)
    {
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
        bool valid = isNameValid && isPriceValid && Quantity.Value > 0 && ItemTypes.Selected != 0;
        CreateButton.Disabled = !valid;
    }

    public Global.Product GetValues()
    {
        if (!isCreateButtonPressed)
        {
            return null;
        }

        string name = NameLineEdit.Text;
        decimal price = (decimal)RealPriceLabel.Value;
        string type = ItemTypes.GetItemText(ItemTypes.Selected);
        int stock = (int)Quantity.Value;
        bool gst = GSTBox.ButtonPressed;
        bool pst = PSTBox.ButtonPressed;
        bool enviromentalFee = EnviromentalFeeBox.ButtonPressed;
        bool bottleDepositFee = BottleDepositBox.ButtonPressed;

        object[] values = { null, null, name, price, stock, type, gst, pst, enviromentalFee, bottleDepositFee };
        return new Global.Product(values.ToList());
    }

}
