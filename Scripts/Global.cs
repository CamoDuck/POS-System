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

    ///////////////////////////////////////////////////////////////////////////
    // Money 
    ///////////////////////////////////////////////////////////////////////////

    static readonly decimal GST_PRECENT = 0.05m;
    static readonly decimal PST_PRECENT = 0.07m;
    static readonly decimal ENVIRONMENTAL_FEE = 0.05m;
    static readonly decimal BOTTLE_DEPOSIT_FEE = 0.10m;


    public static decimal CalculateTotal(decimal originalPrice, decimal discountPercent, bool isGST, bool isPST, bool isEnvironmentalFee, bool isBottleDepositFee)
    {
        decimal newPrice = originalPrice * (1 - discountPercent);
        return CalculateTotal(newPrice, isGST, isPST, isEnvironmentalFee, isBottleDepositFee);
    }

    public static decimal CalculateTotal(decimal originalPrice, bool isGST, bool isPST, bool isEnvironmentalFee, bool isBottleDepositFee)
    {
        decimal gst = isGST ? originalPrice * GST_PRECENT : 0;
        decimal pst = isPST ? originalPrice * PST_PRECENT : 0;
        decimal environmentalFee = isEnvironmentalFee ? ENVIRONMENTAL_FEE : 0;
        decimal bottleDepositFee = isBottleDepositFee ? BOTTLE_DEPOSIT_FEE : 0;

        decimal total = originalPrice + gst + pst + environmentalFee + bottleDepositFee;
        return total;
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

    public static void DisconnectDb()
    {
        dbConnection.Close();
    }

    public static SqliteDataReader Query(string sql)
    {
        SqliteCommand command = new SqliteCommand(sql, dbConnection);
        return command.ExecuteReader();
    }

    public static object[] GetProduct(string barcodeID)
    {
        string sql =
            @$"SELECT *
            FROM Products p
            WHERE p.Barcode = '{barcodeID}'";
        SqliteDataReader queryResult = Global.Query(sql);

        if (queryResult.HasRows)
        {
            queryResult.Read();
            object[] row = new object[Global.PRODUCT_COLUMN_COUNT];
            if (queryResult.GetValues(row) > 0)
            {
                return row;
            }
        }
        return null;
    }

    public static void AddItemToDb(object[] values)
    {
        // string barcode, string name, decimal price, int quantity, string type, bool gst, bool pst, bool environmentalFee, bool bottleDepositFee
        if (values.Length != 9)
        {
            GD.PushError("Wrong number of values");
            return;
        }

        string barcode = Convert.ToString(values[0]);
        string name = Convert.ToString(values[1]);
        decimal price = Convert.ToDecimal(values[2]);
        int quantity = Convert.ToInt32(values[3]);
        string type = Convert.ToString(values[4]);
        bool gst = Convert.ToBoolean(values[5]);
        bool pst = Convert.ToBoolean(values[6]);
        bool environmentalFee = Convert.ToBoolean(values[7]);
        bool bottleDepositFee = Convert.ToBoolean(values[8]);

        GD.Print($"Save : [{barcode}]");
        string sql = $@"INSERT INTO Products (Barcode, Name, Price, Quantity, Type, GST, PST, EnviromentalFee, BottleDepositFee)
            VALUES ('{barcode}', '{name}', {price}, {quantity}, '{type}', {gst}, {pst}, {environmentalFee}, {bottleDepositFee}) ;";
        SqliteCommand command = new SqliteCommand(sql, dbConnection);
        command.ExecuteNonQuery();
    }

}