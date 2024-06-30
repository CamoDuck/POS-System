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

    public void Print(object s)
    {
        GD.Print(s);
    }

    public override void _EnterTree()
    {
        Global.CS = this; // TESTING

        Item.ITEM_SCENE = ITEM_SCENE;
        Item.quantityPopup = quantityPopup;
        Item.itemList = itemListScroll;

        Discount.DISCOUNT_SCENE = DISCOUNT_SCENE;
        Discount.itemList = itemListScroll;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Global.ConnectDb();
        Global.CreateTables();
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
        Global.Product values = Global.GetProductByBarcode(barcodeID);

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

            values.barcode = barcodeID;
            Global.AddItemToDb(values);
        }
        enterProductPopup.CloseIfOpen();

        Item.NewItem(values);
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
        if (Nodes.Count == 0)
        {
            Item lastItem = itemListScroll.GetItem(-1);
            if (lastItem == null)
            {
                return;
            }
            Nodes.Add(lastItem);
        }

        foreach (Item node in Nodes)
        {
            if (percent == 0)
            {
                node.RemoveDiscount();
                continue;
            }

            int index = node.GetIndex();
            Discount.NewDiscount(node, percent, index + 1);
        }
    }

    ///////////////////////////////////////////////////////////////////////////

    [Export] AddItemByNamePopup addItemByNamePopup;

    public void _OnAddItemByNameButtonPressed()
    {
        addItemByNamePopup.Open();
    }

}
