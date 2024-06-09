using Godot;
using System;
using System.Text.RegularExpressions;

public partial class Item : Control
{
    [Export] Label NameLabel;
    [Export] Label PriceLabel;
    [Export] Label GSTLabel;
    [Export] Label PSTLabel;
    [Export] Label QuantityLabel;

    public QuantityPopup quantityPopup;

    StyleBoxFlat stylebox;

    bool selected = false;
    Color defaultColor = new Color(0.9f, 0.9f, 0f, 0f);

    decimal originalPrice;
    int quantity = 1;

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

    public void OnGuiInput(InputEvent e)
    {
        if (Global.IsEventClickDown(e))
        {
            OnItemClicked();
        }
    }

    public void _OnQuantityClicked(InputEvent e)
    {
        if (Global.IsEventClickDown(e))
        {
            quantityPopup.Start(GetName(), quantity, SetQuantity);
        }
    }

    void OnItemClicked()
    {
        Color color = defaultColor;
        selected = !selected;
        if (selected)
        {
            color.A = 1f;
            AddToGroup("SelectedItems");
        }
        else
        {
            RemoveFromGroup("SelectedItems");
        }
        stylebox.BgColor = color;

    }

    public void SetValues(object[] values, QuantityPopup quantityPopup)
    {
        this.quantityPopup = quantityPopup;

        string barcode = Convert.ToString(values[0]);
        string name = Convert.ToString(values[1]);
        decimal price = Convert.ToDecimal(values[2]);
        // int quantity = Convert.ToInt32(values[3]);
        // string type = Convert.ToString(values[4]);
        bool gst = Convert.ToBoolean(values[5]);
        bool pst = Convert.ToBoolean(values[6]);
        bool environmentalFee = Convert.ToBoolean(values[7]);
        bool bottleDepositFee = Convert.ToBoolean(values[8]);

        SetName(name);
        SetPrice(price);
        SetGST(gst);
        SetPST(pst);
        SetEnviromentalFee(environmentalFee);
        SetBottleDepositFee(bottleDepositFee);
    }

    public void SetQuantity(int value)
    {
        quantity = value;
        QuantityLabel.Text = $"x{quantity}";
    }

    void SetName(string name)
    {
        NameLabel.Text = name;
    }

    void SetPrice(decimal p)
    {
        PriceLabel.Text = string.Format("{0:C}", p);
        originalPrice = p;
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

    public decimal GetOriginalPrice()
    {
        return originalPrice;
    }

    public decimal GetTotalPrice()
    {
        return Global.CalculateTotal(quantity, originalPrice, gstEnabled, pstEnabled, environmentalFeeEnabled, bottleDepositFeeEnabled);
    }

    string GetName()
    {
        return NameLabel.Text;
    }

}
