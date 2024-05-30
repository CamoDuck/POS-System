using Godot;
using System;

public partial class Item : Control
{
    public ControlScript control;
    [Export] Label NameLabel;
    [Export] Label PriceLabel;

    StyleBoxFlat stylebox;

    bool selected = false;
    Color defaultColor = new Color(0.9f, 0.9f, 0f, 0f);

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
        }
        stylebox.BgColor = color;
    }

    public void SetName(String name)
    {
        NameLabel.Text = name;
    }

    public void SetPrice(float price)
    {
        PriceLabel.Text = String.Format("{0:C}", price);
    }

    public float GetPrice()
    {
        return Convert.ToSingle(PriceLabel.Text.Replace("$", ""));
    }

    // // Called every frame. 'delta' is the elapsed time since the previous frame.
    // public override void _Process(double delta)
    // {
    // }
}
