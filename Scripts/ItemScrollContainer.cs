using Godot;
using System;

public partial class ItemScrollContainer : ScrollContainer
{
    ControlScript control;

    Label totalLabel;
    Control itemList;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        control = GetOwner<ControlScript>();
        totalLabel = control.totalLabel;
        itemList = control.itemList;
    }

    void OnListChange()
    {
        UpdateTotalLabel();
    }


    void UpdateTotalLabel()
    {
        decimal total = 0;
        var itemLabels = itemList.GetChildren();
        foreach (Item item in itemLabels)
        {
            total += item.GetPrice();
        }

        totalLabel.Text = String.Format("{0:C}", total);
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
