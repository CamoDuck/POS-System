using Godot;
using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

public partial class ItemScrollContainer : ScrollContainer
{
    ControlScript control;

    [Export] Label totalLabel;
    [Export] Label subTotalLabel;
    [Export] Label totalGSTLabel;
    [Export] Label totalPSTLabel;

    Control itemList;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        control = GetOwner<ControlScript>();
        itemList = control.itemList;
        UpdateLabels();
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



    void OnListChange()
    {
        UpdateLabels();

        Task.Delay(10).ContinueWith(t => CallDeferred("UpdateScrollBar"));
    }

    void UpdateScrollBar()
    {
        ScrollVertical = (int)GetVScrollBar().MaxValue; // update scroll bar position
        QueueRedraw();
    }


    public void UpdateLabels(bool _ = false)
    {
        decimal total = 0;
        decimal subtotal = 0;
        decimal totalGST = 0;
        decimal totalPST = 0;

        var itemLabels = GetTree().GetNodesInGroup("Items"); ;
        foreach (Item item in itemLabels)
        {
            total += item.GetTotalPrice();
            subtotal += item.GetSubTotalPrice();
            totalGST += item.GetGST();
            totalPST += item.GetPST();
        }

        totalLabel.Text = $"Total : {String.Format("{0:C}", total)}";
        subTotalLabel.Text = $"SubTotal : {String.Format("{0:C}", subtotal)}";
        totalGSTLabel.Text = $"Total GST : {String.Format("{0:C}", totalGST)}";
        totalPSTLabel.Text = $"Total PST : {String.Format("{0:C}", totalPST)}";

        Global.subtotal = subtotal;
        Global.total = total;
    }

}
