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
    [Export] RecieptConfirmPopup recieptConfirmPopup;

    [Export] PackedScene ITEM_SCENE;
    [Export] PackedScene DISCOUNT_SCENE;

    public void Print(object s)
    {
        GD.Print(s);
    }

    public override void _EnterTree()
    {
        Global.sceneTree = GetTree();
        Global.CS = this; // TESTING

        Item.ITEM_SCENE = ITEM_SCENE;
        Item.quantityPopup = quantityPopup;
        Item.itemList = itemListScroll;

        Discount.DISCOUNT_SCENE = DISCOUNT_SCENE;
        Discount.itemList = itemListScroll;

        GD.Print("TRIED VIEWPORTS");
        secondViewport.World2D = mainViewport.World2D;
        ((SubViewportContainer)mainViewport.GetParent()).Stretch = false;
        ((SubViewportContainer)mainViewport.GetParent()).Stretch = true;
        GD.Print("RUNNED VIEWPOETSS");
    }

    [Export] SubViewport mainViewport;
    [Export] SubViewport secondViewport;

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
        var Nodes = Global.GetSelected();
        if (Nodes.Count == 0)
        {
            Item lastItem = Global.GetItem(-1);
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

    ///////////////////////////////////////////////////////////////////////////

    public void _DeleteButtonPressed()
    {
        Global.DeleteSelectedItems();
    }

    public void _DeSelectAllButtonPressed()
    {
        Global.DeSelectAllItems();
    }

    public void _SelectAllButtonPressed()
    {
        GD.Print("ran");
        Global.SelectAllItems();
    }

    ///////////////////////////////////////////////////////////////////////////


    public void _OnPayCardPressed()
    {
        Global.AddCustomerPurchase(false, 0);
        recieptConfirmPopup.Start();
    }

}
