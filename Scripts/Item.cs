using Godot;
using System;

public partial class Item : Control
{
    [Export] Label NameLabel;
    [Export] Label PriceLabel;

    // public override void _Ready()
    // {
    // }

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
