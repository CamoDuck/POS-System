using System;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using Godot;
using Microsoft.Data.Sqlite;
using System.IO.Ports;
using System.Collections.Generic;

public static class Global
{
    /// TESTING
    public static ControlScript CS;

    public class Product
    {
        public string barcode;
        public string name;
        public decimal price;
        public int quantity;
        public string type;
        public bool gst;
        public bool pst;
        public bool environmental;
        public bool bottleDeposit;

        public Product(List<object> values)
        {
            barcode = Convert.ToString(values[0]);
            name = Convert.ToString(values[1]);
            price = Convert.ToDecimal(values[2]);
            quantity = Convert.ToInt32(values[3]);
            type = Convert.ToString(values[4]);
            gst = Convert.ToBoolean(values[5]);
            pst = Convert.ToBoolean(values[6]);
            environmental = Convert.ToBoolean(values[7]);
            bottleDeposit = Convert.ToBoolean(values[8]);
        }
    }


    ///////////////////////////////////////////////////////////////////////////
    // Money 
    ///////////////////////////////////////////////////////////////////////////

    static readonly decimal GST_PRECENT = 0.05m;
    static readonly decimal PST_PRECENT = 0.07m;
    static readonly decimal ENVIRONMENTAL_FEE = 0.05m;
    static readonly decimal BOTTLE_DEPOSIT_FEE = 0.10m;

    public static decimal CalculateGST(int quantity, decimal originalPrice, decimal discountPercent, bool isGST)
    {
        decimal newPrice = originalPrice * (1 - discountPercent);
        decimal gst = isGST ? newPrice * GST_PRECENT : 0;
        decimal multipliedTotal = gst * quantity;
        return multipliedTotal;
    }

    public static decimal CalculatePST(int quantity, decimal originalPrice, decimal discountPercent, bool isPST)
    {
        decimal newPrice = originalPrice * (1 - discountPercent);
        decimal pst = isPST ? newPrice * PST_PRECENT : 0;
        decimal multipliedTotal = pst * quantity;
        return multipliedTotal;
    }

    public static decimal CalculateTotal(int quantity, decimal originalPrice, decimal discountPercent, bool isGST, bool isPST, bool isEnvironmentalFee, bool isBottleDepositFee)
    {
        decimal newPrice = originalPrice * (1 - discountPercent);
        return CalculateTotal(quantity, newPrice, isGST, isPST, isEnvironmentalFee, isBottleDepositFee);
    }

    public static decimal CalculateTotal(int quantity, decimal originalPrice, bool isGST, bool isPST, bool isEnvironmentalFee, bool isBottleDepositFee)
    {
        decimal gst = isGST ? originalPrice * GST_PRECENT : 0;
        decimal pst = isPST ? originalPrice * PST_PRECENT : 0;
        decimal environmentalFee = isEnvironmentalFee ? ENVIRONMENTAL_FEE : 0;
        decimal bottleDepositFee = isBottleDepositFee ? BOTTLE_DEPOSIT_FEE : 0;

        decimal total = originalPrice + gst + pst + environmentalFee + bottleDepositFee;
        decimal multipliedTotal = total * quantity;
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

    public static readonly int PRODUCT_COLUMN_COUNT = 9;

    public static void ConnectDb()
    {
        dbConnection.Open();
    }

    public static void CreateTables()
    {
        string sql = @"
        CREATE TABLE IF NOT EXISTS Products 
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
            )
            ";
        SqliteCommand command = new SqliteCommand(sql, dbConnection);
        command.ExecuteNonQuery();

        sql = @"
        CREATE TABLE IF NOT EXISTS ProductTypes
            (
            Type VARCHAR(50) PRIMARY KEY
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
        string sql = $@"INSERT INTO Products (Barcode, Name, Price, Quantity, Type, GST, PST, EnviromentalFee, BottleDepositFee)
            VALUES ('{values.barcode}', '{values.name}', {values.price}, {values.quantity}, 
            '{values.type}', {values.gst}, {values.pst}, {values.environmental}, {values.bottleDeposit}) ;";
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