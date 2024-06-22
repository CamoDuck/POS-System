using Godot;
using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

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

    public void AddToList(Node newNode, bool isItem, int index)
    {
        if (isItem)
        {
            string newName = ((Item)newNode).GetName();
            var itemLabels = GetTree().GetNodesInGroup("Items");
            foreach (Item item in itemLabels)
            {
                string name = item.GetName();
                if (name == newName && (!item.isDiscounted()))
                {
                    item.AddOneQuantity();
                    return;
                }
            }
        }

        itemList.AddChild(newNode);
        itemList.MoveChild(newNode, index);
    }

    public Item GetItem(int index)
    {
        return itemList.GetChild<Item>(index);
    }

    void OnListChange()
    {
        UpdateTotalLabel();

        Task.Delay(10).ContinueWith(t => CallDeferred("UpdateScrollBar"));
    }

    void UpdateScrollBar()
    {
        ScrollVertical = (int)GetVScrollBar().MaxValue; // update scroll bar position
        QueueRedraw();
    }


    public void UpdateTotalLabel(bool _ = false)
    {
        decimal total = 0;
        var itemLabels = GetTree().GetNodesInGroup("Items"); ;
        foreach (Item item in itemLabels)
        {
            total += item.GetTotalPrice();
        }

        totalLabel.Text = $"Total : {String.Format("{0:C}", total)}";
    }

}
