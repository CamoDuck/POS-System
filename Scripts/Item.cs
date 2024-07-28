using Godot;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public partial class Item : Control
{
    [Signal] public delegate void PriceChangedEventHandler(bool isDeleted);

    public static PackedScene ITEM_SCENE;
    public static ItemScrollContainer itemList;
    public static QuantityPopup quantityPopup;


    [Export] Label NameLabel;
    [Export] Label PriceLabel;
    [Export] Label GSTLabel;
    [Export] Label PSTLabel;
    [Export] Label QuantityLabel;

    StyleBoxFlat stylebox;

    Global.Product values;

    Color defaultColor = new Color(0.9f, 0.9f, 0f, 0f);

    decimal originalPrice;
    int quantity = 1;
    decimal discountPercent = 0;

    bool gstEnabled = false;
    bool pstEnabled = false;
    bool environmentalFeeEnabled = false;
    bool bottleDepositFeeEnabled = false;

    public override void _Ready()
    {
        stylebox = new StyleBoxFlat();
        stylebox.BgColor = defaultColor;
        AddThemeStyleboxOverride("panel", stylebox);

        SetQuantity(quantity);
    }

    public override void _ExitTree()
    {
        RemoveDiscount();
    }

    static public Node NewItem(Global.Product values, int index = -1)
    {
        Item newItem = ITEM_SCENE.Instantiate<Item>();
        // newItem.quantityPopup = quantityPopup;
        newItem.SetItemValues(values);
        newItem.AddToGroup("Items");

        newItem.PriceChanged += itemList.UpdateLabels;
        itemList.AddToList(newItem, true, index);

        return newItem;
    }

    public void _OnQuantityClicked()
    {
        GD.Print("Pressed");
        quantityPopup.Start(GetName(), quantity, SetQuantity);
    }

    public void _OnItemClicked()
    {
        if (IsInGroup(Global.SELECTED_ITEMS_GROUP_NAME))
        {
            RemoveFromGroup(Global.SELECTED_ITEMS_GROUP_NAME);
        }
        else
        {
            AddToGroup(Global.SELECTED_ITEMS_GROUP_NAME);
        }
        UpdateSelectedColor();
    }

    public void UpdateSelectedColor()
    {
        Color color = defaultColor;
        if (IsInGroup(Global.SELECTED_ITEMS_GROUP_NAME))
        {
            color.A = 1f;
        }
        stylebox.BgColor = color;
    }

    public void RemoveDiscount()
    {
        discountPercent = 0;
        EmitSignal(SignalName.PriceChanged, true);
    }

    public void SetItemValues(Global.Product values)
    {
        this.values = values;
        originalPrice = values.price;
        SetName(values.name);
        SetPrice(values.price);
        SetGST(values.gst);
        SetPST(values.pst);
        SetEnviromentalFee(values.environmental);
        SetBottleDepositFee(values.bottleDeposit);
    }

    public void SetDiscount(int percent)
    {
        this.discountPercent = percent / 100m;
    }

    public void SetQuantity(int value)
    {
        quantity = value;
        QuantityLabel.Text = $"{quantity} @";

        SetPrice(GetNoDiscountPrice());

        EmitSignal(SignalName.PriceChanged, false);
    }

    void SetName(string name)
    {
        NameLabel.Text = name;
    }

    void SetPrice(decimal p)
    {
        PriceLabel.Text = string.Format("{0:C}", p);
    }

    void SetGST(bool value)
    {
        gstEnabled = value;
        if (gstEnabled)
        {
            GSTLabel.Show();
        }
        else
        {
            GSTLabel.Hide();
        }
    }

    void SetPST(bool value)
    {
        pstEnabled = value;
        if (pstEnabled)
        {
            PSTLabel.Show();
        }
        else
        {
            PSTLabel.Hide();
        }
    }

    void SetEnviromentalFee(bool value)
    {
        environmentalFeeEnabled = value;
    }

    void SetBottleDepositFee(bool value)
    {
        bottleDepositFeeEnabled = value;
    }

    public int GetId()
    {
        return values.id;
    }

    public int GetQuantity()
    {
        return quantity;
    }

    public decimal GetDiscount()
    {
        return discountPercent;
    }

    public decimal GetSingleOriginalPrice()
    {
        return originalPrice;
    }

    public decimal GetNoDiscountPrice()
    {
        return originalPrice * quantity;
    }

    public decimal GetSubTotalPrice()
    {
        return Global.CalculateTotal(quantity, originalPrice, discountPercent, false, false, false, false);
    }

    public decimal GetTotalPrice()
    {
        return Global.CalculateTotal(quantity, originalPrice, discountPercent, gstEnabled, pstEnabled, environmentalFeeEnabled, bottleDepositFeeEnabled);
    }

    public decimal GetGST()
    {
        return Global.CalculateGST(quantity, originalPrice, discountPercent, gstEnabled);
    }

    public decimal GetPST()
    {
        return Global.CalculatePST(quantity, originalPrice, discountPercent, pstEnabled);
    }

    public string GetName()
    {
        return NameLabel.Text;
    }

    public void AddOneQuantity()
    {
        SetQuantity(quantity + 1);
    }

    public bool isDiscounted()
    {
        return discountPercent > 0;
    }

    public string[] ToText()
    {
        string itemText = $"{GetName()}";
        string quantityText = $"     {GetQuantity()} @ {GetSingleOriginalPrice():C}".PadRight(39, ' ') + $"{GetNoDiscountPrice():C}".PadRight(9, ' ');
        string discountText = isDiscounted() ? $"     DISCOUNT {GetDiscount() * 100}%".PadRight(38, ' ') + $"-{GetNoDiscountPrice() - GetSubTotalPrice():C}".PadRight(10, ' ') : null;

        string[] ret = { itemText, quantityText, discountText };
        return ret;
    }

}
