using System;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using Godot;
using Microsoft.Data.Sqlite;
using System.IO.Ports;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Data.Common;

public static class Global
{
    /// TESTING
    public static ControlScript CS;

    public class Product
    {
        public int id;
        public string barcode;
        public string name;
        public decimal price;
        public int stock;
        public string type;
        public bool gst;
        public bool pst;
        public bool environmental;
        public bool bottleDeposit;

        public Product(List<object> values)
        {
            id = Convert.ToInt32(values[0]);
            barcode = Convert.ToString(values[1]);
            name = Convert.ToString(values[2]);
            price = Convert.ToDecimal(values[3]);
            stock = Convert.ToInt32(values[4]);
            type = Convert.ToString(values[5]);
            gst = Convert.ToBoolean(values[6]);
            pst = Convert.ToBoolean(values[7]);
            environmental = Convert.ToBoolean(values[8]);
            bottleDeposit = Convert.ToBoolean(values[9]);
        }
    }
    public static readonly int PRODUCT_COLUMN_COUNT = 10;


    public static decimal total = 0;
    public static SceneTree sceneTree;

    ///////////////////////////////////////////////////////////////////////////
    // Money 
    ///////////////////////////////////////////////////////////////////////////

    static readonly decimal GST_PRECENT = 0.05m;
    static readonly decimal PST_PRECENT = 0.07m;
    static readonly decimal ENVIRONMENTAL_FEE = 0.05m;
    static readonly decimal BOTTLE_DEPOSIT_FEE = 0.10m;

    public static decimal CalculateGST(int stock, decimal originalPrice, decimal discountPercent, bool isGST)
    {
        decimal newPrice = originalPrice * (1 - discountPercent);
        decimal gst = isGST ? newPrice * GST_PRECENT : 0;
        decimal multipliedTotal = gst * stock;
        return multipliedTotal;
    }

    public static decimal CalculatePST(int stock, decimal originalPrice, decimal discountPercent, bool isPST)
    {
        decimal newPrice = originalPrice * (1 - discountPercent);
        decimal pst = isPST ? newPrice * PST_PRECENT : 0;
        decimal multipliedTotal = pst * stock;
        return multipliedTotal;
    }

    public static decimal CalculateTotal(int stock, decimal originalPrice, decimal discountPercent, bool isGST, bool isPST, bool isEnvironmentalFee, bool isBottleDepositFee)
    {
        decimal newPrice = originalPrice * (1 - discountPercent);
        return CalculateTotal(stock, newPrice, isGST, isPST, isEnvironmentalFee, isBottleDepositFee);
    }

    public static decimal CalculateTotal(int stock, decimal originalPrice, bool isGST, bool isPST, bool isEnvironmentalFee, bool isBottleDepositFee)
    {
        decimal gst = isGST ? originalPrice * GST_PRECENT : 0;
        decimal pst = isPST ? originalPrice * PST_PRECENT : 0;
        decimal environmentalFee = isEnvironmentalFee ? ENVIRONMENTAL_FEE : 0;
        decimal bottleDepositFee = isBottleDepositFee ? BOTTLE_DEPOSIT_FEE : 0;

        decimal total = originalPrice + gst + pst + environmentalFee + bottleDepositFee;
        decimal multipliedTotal = total * stock;
        return multipliedTotal;
    }

    ///////////////////////////////////////////////////////////////////////////
    // Barcode Scannner 
    ///////////////////////////////////////////////////////////////////////////

    [Signal] public delegate void BarcodeReadEventHandler(string barcode);

    static SerialPort port = null;
    static List<Action<string>> callbacks = new List<Action<string>>();

    public static void ConnectScanner(Action<string> callback)
    {
        if (port == null)
        {
            port = new SerialPort("COM4", 9600, Parity.None, 8, StopBits.One);
            port.DataReceived += new SerialDataReceivedEventHandler(_OnPortDataReceived);
            // Begin communications 
            port.Open();
        }
        callbacks.Add(callback);
    }

    public static void DisconnectScannerAll()
    {
        port.Close();
    }

    static void _OnPortDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        // Show all the incoming data in the port's buffer
        string barcode = port.ReadExisting();
        foreach (var callback in callbacks)
        {
            callback(barcode);
        }
    }


    ///////////////////////////////////////////////////////////////////////////
    // SQL 
    ///////////////////////////////////////////////////////////////////////////

    static readonly string dbPath = "database.db";
    static readonly string globalDbPath = ProjectSettings.GlobalizePath(dbPath);
    public static SqliteConnection dbConnection = new SqliteConnection($"Data Source={globalDbPath}");

    public static void ConnectDb()
    {
        dbConnection.Open();
    }

    public static void CreateTables()
    {
        string sql = @"
        CREATE TABLE IF NOT EXISTS Products 
            (
            Id INTEGER PRIMARY KEY,
            Barcode VARCHAR(13) NOT NULL UNIQUE, 
            Name VARCHAR(50) NOT NULL UNIQUE,
            Price Decimal(19,4) NOT NULL,
            Stock INT NOT NULL,
            Type VARCHAR(50) NOT NULL,
            GST BOOLEAN NOT NULL,
            PST BOOLEAN NOT NULL,
            EnviromentalFee BOOLEAN NOT NULL,
            BottleDepositFee BOOLEAN NOT NULL

            
            )
            ";

        //FOREIGN KEY (Type) REFERENCES ProductTypes (Type)
        SqliteCommand command = new SqliteCommand(sql, dbConnection);
        command.ExecuteNonQuery();

        sql = @"
        CREATE TABLE IF NOT EXISTS ProductTypes
            (
            Type VARCHAR(50) PRIMARY KEY
            )";
        command = new SqliteCommand(sql, dbConnection);
        command.ExecuteNonQuery();

        sql = @"
        CREATE TABLE IF NOT EXISTS CustomerPurchase
            (
            Id INTEGER PRIMARY KEY,
            CreateTime DATETIME NOT NULL DEFAULT(DATETIME('now')),
            IsCash Boolean NOT NULL,
            Discount DECIMAL(5,4)
            )";
        command = new SqliteCommand(sql, dbConnection);
        command.ExecuteNonQuery();

        sql = @"
        CREATE TABLE IF NOT EXISTS CustomerPurchaseItem
            (
            PurchaseId INT,
            ItemId INT,
            Discount DECIMAL(5,4),
            Price Decimal(19,4) NOT NULL, 
            Quantity SMALLINT NOT NULL, 

            PRIMARY KEY (PurchaseId, ItemId, Discount),

            FOREIGN KEY (PurchaseId) REFERENCES CustomerPurchase (Id),
            FOREIGN KEY (ItemId) REFERENCES Products (Id)
            )";
        command = new SqliteCommand(sql, dbConnection);
        command.ExecuteNonQuery();
    }

    public static void DisconnectDb()
    {
        dbConnection.Close();
    }

    static SqliteDataReader Query(string sql)
    {
        SqliteCommand command = new SqliteCommand(sql, dbConnection);
        return command.ExecuteReader();
    }

    static List<List<object>> QueryAndRead(string sql, int columnCount, int maxRowsCount = int.MaxValue)
    {
        SqliteDataReader reader = Global.Query(sql);
        List<List<object>> data = new List<List<object>>();

        int i = 0;
        while (reader.Read() && i < maxRowsCount)
        {
            object[] vTemp = new object[columnCount];
            reader.GetValues(vTemp);
            data.Add(vTemp.ToList());
            i++;
        }

        if (i == 0)
        {
            return null;
        }
        return data;
    }

    public static Product GetProductByBarcode(string barcodeID)
    {
        string sql =
            @$"SELECT *
            FROM Products p
            WHERE p.Barcode = '{barcodeID}'";

        List<List<object>> rows = QueryAndRead(sql, Global.PRODUCT_COLUMN_COUNT, 1);
        if (rows != null && rows.Count > 0)
        {
            return new Product(rows[0]);
        }
        return null;
    }

    public static List<Product> GetProductsByName(string Name, string type = null, int maxRowsCount = int.MaxValue)
    {
        string condition1 = type == null ? "" : $"AND p.Type == '{type}'";

        string sql =
        @$"SELECT *
            FROM Products p
            WHERE p.Name LIKE '%{Name}%' {condition1}
            ORDER BY LOWER(p.Name) ASC";

        List<List<object>> rows = QueryAndRead(sql, PRODUCT_COLUMN_COUNT, maxRowsCount);
        if (rows != null && rows.Count > 0)
        {
            List<Product> products = rows.Select(row => new Product(row)).ToList();
            return products;
        }
        return null;
    }

    public static List<string> GetProductTypes()
    {
        string sql =
        @$"SELECT *
            FROM ProductTypes pt
            ORDER BY LOWER(pt.Type) ASC";

        List<List<object>> rows = QueryAndRead(sql, 1);

        if (rows != null && rows.Count > 0)
        {
            List<string> types = new List<string>();

            foreach (List<object> row in rows)
            {
                types.Add((string)row[0]);
            }
            return types;
        }
        return null;
    }

    public static void AddItemToDb(Global.Product values)
    {
        GD.Print($"Save : [{values.barcode}]");
        string sql = $@"INSERT INTO Products (Id, Barcode, Name, Price, Stock, Type, GST, PST, EnviromentalFee, BottleDepositFee)
            VALUES (NULL, '{values.barcode}', '{values.name}', {values.price}, {values.stock}, 
            '{values.type}', {values.gst}, {values.pst}, {values.environmental}, {values.bottleDeposit}) ;";
        SqliteCommand command = new SqliteCommand(sql, dbConnection);
        command.ExecuteNonQuery();

        ////// for preventing sql injections
        // string sql = @"INSERT INTO Products (Barcode, Name, Price, Stock, Type, GST, PST, EnviromentalFee, BottleDepositFee)
        //       VALUES (@barcode, @name, @price, @stock, @type, @gst, @pst, @environmental, @bottleDeposit)";

        // SqliteCommand command = new SqliteCommand(sql, dbConnection);
        // command.Parameters.AddWithValue("@barcode", values.barcode);
        // command.Parameters.AddWithValue("@name", values.name);
        // command.Parameters.AddWithValue("@price", values.price);
        // // Add similar lines for all other parameters
        // command.ExecuteNonQuery();
    }

    public static void AddCustomerPurchase(bool isCash, decimal discount)
    {
        string sql = $@"INSERT INTO CustomerPurchase (Id, IsCash, Discount)
            VALUES (NULL, {isCash}, {discount})
            RETURNING Id;
            ";
        SqliteCommand command = new SqliteCommand(sql, dbConnection);
        long id = (long)command.ExecuteScalar();

        CS.Print(id);

        var itemLabels = sceneTree.GetNodesInGroup("Items"); ;
        foreach (Item item in itemLabels)
        {
            AddPurchasedItem(id, item);
        }

    }

    static void AddPurchasedItem(long PurchaseId, Item item)
    {
        string sql = $@"INSERT INTO CustomerPurchaseItem (PurchaseId, ItemId, Discount, Price, Quantity)
            VALUES ({PurchaseId}, {item.GetId()}, {item.GetDiscount()}, {item.GetSingleOriginalPrice()}, {item.GetQuantity()});";
        SqliteCommand command = new SqliteCommand(sql, dbConnection);
        command.ExecuteNonQuery();
    }

    ///////////////////////////////////////////////////////////////////////////
    // MISC 
    ///////////////////////////////////////////////////////////////////////////

    public static bool IsEventClickDown(InputEvent e)
    {
        return e is InputEventMouseButton mouseEvent && !mouseEvent.Pressed;
    }

}