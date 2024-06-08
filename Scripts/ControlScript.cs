using Godot;
using System;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Data;


public partial class ControlScript : Control
{
    [Export] public Control itemList;
    [Export] public ScrollContainer itemListScroll;
    [Export] public Label totalLabel;

    [Export] Control barcodeInput;
    [Export] Control enterProductPopup;

    [Export] PackedScene item;

    RandomNumberGenerator rng = new RandomNumberGenerator();

    // Database
    SqliteConnection dbConnection;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        string dbPath = "database.db";
        string globalDbPath = ProjectSettings.GlobalizePath(dbPath);
        GD.Print(globalDbPath);

        dbConnection = new SqliteConnection($"Data Source={globalDbPath}");
        dbConnection.Open();


        string sql = @"CREATE TABLE IF NOT EXISTS Products 
        (
        Barcode VARCHAR(13) PRIMARY KEY, 
        Name VARCHAR(50) NOT NULL,
        Price SMALLMONEY NOT NULL,
        Quantity INT NOT NULL,
        Type VARCHAR(50),
        GST BOOLEAN NOT NULL,
        PST BOOLEAN NOT NULL,
        EnviromentalFee BOOLEAN NOT NULL,
        BottleDepositFee BOOLEAN NOT NULL
        )";
        SqliteCommand command = new SqliteCommand(sql, dbConnection);
        command.ExecuteNonQuery();

    }

    public override void _ExitTree()
    {
        base._ExitTree();
        dbConnection.Close();
    }

    SqliteDataReader Query(string sql)
    {
        SqliteCommand command = new SqliteCommand(sql, dbConnection);
        return command.ExecuteReader();
    }


    public void _OnBarcode(string barcode)
    {
        GD.Print($"Read : [{barcode}]");
        ScanItem(barcode);
    }

    public void AddItemToDb(string barcode, string name, decimal price, int quantity, string type, bool gst, bool pst, bool environmentalFee, bool bottleDepositFee)
    {
        GD.Print($"Save : [{barcode}]");
        string sql = $@"INSERT INTO Products (Barcode, Name, Price, Quantity, Type, GST, PST, EnviromentalFee, BottleDepositFee)
            VALUES ('{barcode}', '{name}', {price}, {quantity}, '{type}', {gst}, {pst}, {environmentalFee}, {bottleDepositFee}) ;";
        SqliteCommand command = new SqliteCommand(sql, dbConnection);
        command.ExecuteNonQuery();
    }


    async void ScanItem(string barcodeID)
    {
        string sql =
            @$"SELECT Name, Price, Quantity, Type, GST, PST, EnviromentalFee, BottleDepositFee
            FROM Products p
            WHERE p.Barcode = '{barcodeID}'";
        SqliteDataReader queryResult = Query(sql);

        string name = "ERROR";
        decimal price = 0;
        int quantity = -1;
        string type = "";
        bool gst = false;
        bool pst = false;
        bool environmentalFee = false;
        bool bottleDepositFee = false;
        if (queryResult.HasRows)
        {
            queryResult.Read();
            name = queryResult.GetString(queryResult.GetOrdinal("Name"));
            price = queryResult.GetDecimal(queryResult.GetOrdinal("Price"));
            gst = queryResult.GetBoolean(queryResult.GetOrdinal("GST"));
            pst = queryResult.GetBoolean(queryResult.GetOrdinal("PST"));
            environmentalFee = queryResult.GetBoolean(queryResult.GetOrdinal("EnviromentalFee"));
            bottleDepositFee = queryResult.GetBoolean(queryResult.GetOrdinal("BottleDepositFee"));
        }
        else
        {
            enterProductPopup.Show();
            Variant[] result = await ToSignal(enterProductPopup, EnterProductPopup.SignalName.Created);

            if (result.Length >= 8)
            {
                name = result[0].ToString();
                price = (decimal)result[1].AsDouble();
                gst = result[4].AsBool();
                pst = result[5].AsBool();
                environmentalFee = result[6].AsBool();
                bottleDepositFee = result[7].AsBool();
                AddItemToDb(barcodeID, name, price, quantity, type, gst, pst, environmentalFee, bottleDepositFee);
            }
        }

        var clone = item.Instantiate();
        Item cloneScript = (Item)clone;

        cloneScript.SetName(name);
        cloneScript.SetPrice(price);
        cloneScript.SetGST(gst);
        cloneScript.SetPST(pst);
        cloneScript.SetEnviromentalFee(environmentalFee);
        cloneScript.SetBottleDepositFee(bottleDepositFee);

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
