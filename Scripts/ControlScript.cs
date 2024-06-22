using Godot;
using System;
using System.Threading.Tasks;


public partial class ControlScript : Control
{
    [Export] public Control itemList;
    [Export] public ItemScrollContainer itemListScroll;
    [Export] public Label totalLabel;

    // [Export] Control barcodeInput;
    [Export] EnterProductPopup enterProductPopup;
    [Export] QuantityPopup quantityPopup;

    [Export] PackedScene ITEM_SCENE;
    [Export] PackedScene DISCOUNT_SCENE;

    public override void _EnterTree()
    {
        Item.ITEM_SCENE = ITEM_SCENE;
        Discount.DISCOUNT_SCENE = DISCOUNT_SCENE;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Global.ConnectDb();
        Global.ConnectScanner(_OnBarcode);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Global.DisconnectDb();
        Global.DisconnectScannerAll();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Q"))
        {
            DeleteSelectedItems();
        }
    }

    // Not called on the main thread
    public void _OnBarcode(string barcode)
    {
        GD.Print($"Read : [{barcode}]");
        CallDeferred("ScanItem", barcode);
    }

    async void ScanItem(string barcodeID)
    {
        object[] values = Global.GetProduct(barcodeID);

        if (values == null)
        {
            // Try to add item to database using popup window
            enterProductPopup.Start();
            await ToSignal(enterProductPopup, EnterProductPopup.SignalName.ClosedPopup);
            values = enterProductPopup.GetValues();

            if (values == null)
            {
                GD.Print("Canceled Item Creation");
                return;
            }

            values[0] = barcodeID;
            Global.AddItemToDb(values);
        }
        enterProductPopup.CloseIfOpen();

        Item.NewItem(itemListScroll, quantityPopup, values);
    }

    bool IsAnySelected()
    {
        return GetSelectedCount() > 0;
    }

    int GetSelectedCount()
    {
        return GetSelected().Count;
    }

    Godot.Collections.Array<Node> GetSelected()
    {
        return GetTree().GetNodesInGroup("SelectedItems");
    }

    void DeleteSelectedItems()
    {
        Godot.Collections.Array<Node> items = GetSelected();
        foreach (Node item in items)
        {
            item.QueueFree();
        }
    }


    ///////////////////////////////////////////////////////////////////////////
    // Buttons
    ///////////////////////////////////////////////////////////////////////////

    void UpdateButtons()
    {
        // UpdateDiscountButtons();
    }

    ///////////////////////////////////////////////////////////////////////////

    public void _OnDiscountButtonPressed(int percent)
    {
        var Nodes = GetSelected();
        foreach (Item node in Nodes)
        {
            int index = node.GetIndex();
            Discount.NewDiscount(itemListScroll, node, percent, index + 1);
        }

        if (Nodes.Count == 0)
        {
            Item lastItem = itemListScroll.GetItem(-1);
            Discount.NewDiscount(itemListScroll, lastItem, percent);
        }
    }
    ///////////////////////////////////////////////////////////////////////////

}
