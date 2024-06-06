using Godot;
using System;

public partial class ControlScript : Control
{
    [Export] public Control itemList;
    [Export] public ScrollContainer itemListScroll;
    [Export] public Label totalLabel;

    [Export] Control barcodeInput;

    [Export] PackedScene item;

    RandomNumberGenerator rng = new RandomNumberGenerator();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    public void _OnBarcode(string barcode)
    {
        AddItem(barcode);
    }


    async void AddItem(string barcodeID)
    {
        var clone = item.Instantiate();
        Item cloneScript = (Item)clone;

        cloneScript.SetName(barcodeID);
        float randNum = Mathf.Round(rng.Randf() * Mathf.Pow(10.0f, 5)) / Mathf.Pow(10.0f, 2);
        cloneScript.SetPrice(randNum);
        itemList.AddChild(clone);

        // delay needed for maxvalue to update;
        await ToSignal(GetTree().CreateTimer(0.001), "timeout");
        itemListScroll.ScrollVertical = (int)itemListScroll.GetVScrollBar().MaxValue;
    }

    void DeleteSelectedItems()
    {
        var items = GetTree().GetNodesInGroup("SelectedItems");
        foreach (Node item in items)
        {
            item.QueueFree();
        }
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Q"))
        {
            DeleteSelectedItems();
        }

    }
}
