using Godot;
using System;
using System.Text.RegularExpressions;

public partial class Item : Control
{
    public ControlScript control;
    [Export] Label NameLabel;
    [Export] Label PriceLabel;
    [Export] Label GSTLabel;
    [Export] Label PSTLabel;

    StyleBoxFlat stylebox;

    bool selected = false;
    Color defaultColor = new Color(0.9f, 0.9f, 0f, 0f);

    decimal originalPrice;
    bool gstEnabled = false;
    bool pstEnabled = false;
    bool environmentalFeeEnabled = false;
    bool bottleDepositFeeEnabled = false;

    public override void _Ready()
    {
        stylebox = new StyleBoxFlat();
        stylebox.BgColor = defaultColor;
        AddThemeStyleboxOverride("panel", stylebox);
    }

    public void OnGuiInput(InputEvent e)
    {
        if (e is InputEventMouseButton mouseEvent && !mouseEvent.Pressed)
        {
            onClicked();
        }
    }

    void onClicked()
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

    public void SetName(string name)
    {
        NameLabel.Text = name;
    }

    public void SetPrice(decimal p)
    {
        PriceLabel.Text = string.Format("{0:C}", p);
        originalPrice = p;
    }

    public void SetGST(bool value)
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

    public void SetPST(bool value)
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

    public void SetEnviromentalFee(bool value)
    {
        environmentalFeeEnabled = value;
    }

    public void SetBottleDepositFee(bool value)
    {
        bottleDepositFeeEnabled = value;
    }

    public decimal GetOriginalPrice()
    {
        return originalPrice;
    }

    public decimal GetTotalPrice()
    {
        return Global.CalculateTotal(originalPrice, gstEnabled, pstEnabled, environmentalFeeEnabled, bottleDepositFeeEnabled);
    }

}
