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
        Type VARCHAR(50)
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
        ScanItem(barcode);
    }

    async void ScanItem(string barcodeID)
    {
        string sql =
            @$"SELECT Name, Price 
            FROM Products p
            WHERE p.Barcode = {barcodeID}";
        SqliteDataReader queryResult = Query(sql);

        string name = "ERROR";
        decimal price = 0;
        if (queryResult.HasRows)
        {
            queryResult.Read();
            name = queryResult.GetString(queryResult.GetOrdinal("Name"));
            price = queryResult.GetDecimal(queryResult.GetOrdinal("Price"));
        }


        var clone = item.Instantiate();
        Item cloneScript = (Item)clone;

        cloneScript.SetName(name);
        cloneScript.SetPrice(price);

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
