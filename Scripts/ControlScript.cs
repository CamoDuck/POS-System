using Godot;
using System;
using System.Threading.Tasks;


public partial class ControlScript : Control
{
    [Export] public Control itemList;
    [Export] public ScrollContainer itemListScroll;
    [Export] public Label totalLabel;

    // [Export] Control barcodeInput;
    [Export] EnterProductPopup enterProductPopup;

    [Export] PackedScene item;

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

        var newItem = item.Instantiate();
        Item itemScript = (Item)newItem;
        itemScript.SetValues(values);

        itemList.AddChild(newItem);

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
}
