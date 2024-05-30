using Godot;
using System;

public partial class ControlScript : Control
{
    [Export] public Control itemList;
    [Export] public ScrollContainer itemListScroll;
    [Export] public Label totalLabel;

    [Export] PackedScene item;

    RandomNumberGenerator rng = new RandomNumberGenerator();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }


    async void AddItem()
    {
        var clone = item.Instantiate();
        Item cloneScript = (Item)clone;

        cloneScript.SetName("Item!!");
        float randNum = Mathf.Round(rng.Randf() * Mathf.Pow(10.0f, 5)) / Mathf.Pow(10.0f, 2);
        cloneScript.SetPrice(randNum);
        itemList.AddChild(clone);

        // delay needed for maxvalue to update;
        await ToSignal(GetTree().CreateTimer(0.001), "timeout");
        itemListScroll.ScrollVertical = (int)itemListScroll.GetVScrollBar().MaxValue;
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("A"))
        {
            AddItem();
        }

    }
}
