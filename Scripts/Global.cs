using System;
using System.Data;
using System.Linq;
using Godot;
using Microsoft.Data.Sqlite;
using System.IO.Ports;
using System.Collections.Generic;
using System.Drawing.Printing;

using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Godot.Collections;
using System.Collections;
using System.Threading.Tasks;

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
    public static decimal subtotal = 0;
    public static SceneTree sceneTree;

    public static decimal priceMarkup = 1.35m;

    ///////////////////////////////////////////////////////////////////////////
    // Money 
    ///////////////////////////////////////////////////////////////////////////

    public static readonly decimal GST_PRECENT = 0.05m;
    public static readonly decimal PST_PRECENT = 0.07m;
    public static readonly decimal ENVIRONMENTAL_FEE = 0.05m;
    public static readonly decimal BOTTLE_DEPOSIT_FEE = 0.10m;

    public static decimal Round(decimal value)
    {
        return Math.Round(value * 20) / 20;
    }
    public static decimal ApplyMarkupAndRound(decimal value)
    {
        decimal markupPrice = value * priceMarkup;
        decimal roundedPrice = Round(markupPrice);
        return roundedPrice;
    }

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

    static System.IO.Ports.SerialPort port = null;
    static List<Action<string>> callbacks = new List<Action<string>>();

    public static async void ConnectScanner(Action<string> callback)
    {
        callbacks.Add(callback);
        if (port == null)
        {
            port = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
            port.DataReceived += new SerialDataReceivedEventHandler(_OnPortDataReceived);

            bool succeed = false;
            while (!succeed)
            {
                try
                {
                    port.Open();
                    succeed = true;
                    GD.Print("Scanner Connected!");
                }
                catch (Exception e)
                {
                    GD.PrintErr("Failed to connect to printer. Retrying in 5 seconds");
                    await Task.Delay(5000);
                }
            }
        }

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
    static bool dbExists = File.Exists(Global.globalDbPath);

    static long lastOrderID = -1;
    public static void ConnectDb()
    {
        dbConnection.Open();
    }

    public static void CreateTables()
    {
        if (dbExists) { return; }
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

        string[] types =
        {"Deli",
        "Snack",
        "Dessert",
        "Candy",
        "Drink",
        "Noodle"};

        foreach (string type in types)
        {
            sql = $@"INSERT INTO ProductTypes (Type)
                VALUES ('{type}');";
            command = new SqliteCommand(sql, dbConnection);
            command.ExecuteNonQuery();
        }
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
            Product p = new Product(rows[0]);
            return p;
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
        lastOrderID = id;

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
    // Selected Item 
    ///////////////////////////////////////////////////////////////////////////

    public static readonly string SELECTED_ITEMS_GROUP_NAME = "SelectedItems";

    static public bool IsAnySelected()
    {
        return GetSelectedCount() > 0;
    }

    static public int GetSelectedCount()
    {
        return GetSelected().Count;
    }

    static public Godot.Collections.Array<Node> GetSelected()
    {
        return sceneTree.GetNodesInGroup(SELECTED_ITEMS_GROUP_NAME);
    }

    static public void SelectAllItems()
    {
        foreach (Node item in sceneTree.GetNodesInGroup("Items"))
        {
            if (!item.IsInGroup(SELECTED_ITEMS_GROUP_NAME))
            {
                item.AddToGroup(SELECTED_ITEMS_GROUP_NAME);
            }
        }
        UpdateItemsSelectedStatus();
    }

    static public void DeSelectAllItems()
    {
        foreach (Node item in sceneTree.GetNodesInGroup(SELECTED_ITEMS_GROUP_NAME))
        {
            item.RemoveFromGroup(SELECTED_ITEMS_GROUP_NAME);
        }
        UpdateItemsSelectedStatus();
    }

    static void UpdateItemsSelectedStatus()
    {
        foreach (Item item in sceneTree.GetNodesInGroup("Items"))
        {
            item.UpdateSelectedColor();
        }
    }

    static public void DeleteSelectedItems()
    {
        Godot.Collections.Array<Node> items = GetSelected();
        foreach (Node item in items)
        {
            DeleteItem(item);
        }
    }

    static public void DeleteItem(Node item)
    {
        item.QueueFree();
    }

    static public void DeleteAllItems()
    {
        SelectAllItems();
        DeleteSelectedItems();
    }

    ///////////////////////////////////////////////////////////////////////////
    // Item 
    ///////////////////////////////////////////////////////////////////////////

    static public Godot.Collections.Array<Node> GetAllItems()
    {
        return sceneTree.GetNodesInGroup("Items");
    }

    static public Item GetItem(int index)
    {
        Node[] items = sceneTree.GetNodesInGroup("Items").ToArray();
        if (items.Count() == 0)
        {
            return null;
        }
        return (Item)items.Last();
    }


    ///////////////////////////////////////////////////////////////////////////
    // MISC 
    ///////////////////////////////////////////////////////////////////////////

    public static bool IsEventClickDown(InputEvent e)
    {
        return e is InputEventMouseButton mouseEvent && !mouseEvent.Pressed;
    }



    ///////////////////////////////////////////////////////////////////////////
    // Printing 
    ///////////////////////////////////////////////////////////////////////////
    // static readonly string PORT_NAME = "RONGTA 80mm Series Printer";

    static byte lineSpacing = 30;

    enum Justification { LEFT, MIDDLE, RIGHT };

    static class CMD
    {
        public static readonly byte[] ESC_3_n = { 27, 51, 0 };
        public static readonly byte[] GS_V_m = { 29, 86, 0 };
        public static readonly byte[] GS_V_m_n = { 29, 86, 0, 0 };
        public static readonly byte[] DC2_T = { 18, 84 };
        public static readonly byte[] LF = { 10 };
        public static readonly byte[] ESC_d_n = { 27, 100, 0 };
        public static readonly byte[] ESC_dollors_nL_nH = { 27, 36, 0, 0 };
        public static readonly byte[] GS_dollors_nL_nH = { 29, 36, 0, 0 };
        public static readonly byte[] GS_w_n = { 29, 119, 0 };
        public static readonly byte[] GS_h_n = { 29, 104, 0 };
        public static readonly byte[] GS_f_n = { 29, 102, 0 };
        public static readonly byte[] GS_H_n = { 29, 72, 0 };
        public static readonly byte[] GS_k_m_n_ = { 29, 107, 0, 0 };

        public static readonly byte[] GS_exclamationmark_n = { 29, 33, 0 };
        public static readonly byte[] ESC_a_n = { 27, 97, 0 };
        public static readonly byte[] CR = { 13 };

        public static readonly byte[] ESC_p_m_t1_t2 = { 27, 112, 0, 0, 0 };
        // public static readonly byte[]  = { };
        // public static readonly byte[]  = { };
        // public static readonly byte[]  = { };
        // public static readonly byte[]  = { };
        // public static readonly byte[]  = { };
    }

    static string printer;

    public static T[] ConcatArray<T>(T[] x, T[] y)
    {
        T[] z = new T[x.Length + y.Length];
        x.CopyTo(z, 0);
        y.CopyTo(z, x.Length);
        return z;
    }

    static void PrintTestPage()
    {
        byte[] cmd = CMD.DC2_T;
        PrintBytes(cmd);
    }

    static public void OpenCashDrawer()
    {
        byte[] cmd = CMD.ESC_p_m_t1_t2;
        cmd[2] = 0; // pin 5
        cmd[3] = 100;
        cmd[4] = 100;
        PrintBytes(cmd);
    }

    public static void PrintBarcode(string pszInfoBuffer, int nOrgx, int nType, int nWidthX, int nHeight, int nHriFontType, int nHriFontPosition)
    {
        if (!(nOrgx < 0 || nOrgx > 65535 || nType < 65 || nType > 73 || nWidthX < 2 || nWidthX > 6 || nHeight < 1 || nHeight > 255))
        {
            byte[] bytes = Encoding.Default.GetBytes(pszInfoBuffer);
            int num = CMD.ESC_dollors_nL_nH.Length + CMD.GS_w_n.Length + CMD.GS_h_n.Length + CMD.GS_f_n.Length + CMD.GS_H_n.Length + CMD.GS_k_m_n_.Length + bytes.Length;
            byte[] array = new byte[num];
            int num2 = 0;
            CMD.ESC_dollors_nL_nH[2] = (byte)(nOrgx % 256);
            CMD.ESC_dollors_nL_nH[3] = (byte)(nOrgx / 256);
            CMD.ESC_dollors_nL_nH.CopyTo(array, num2);
            num2 += CMD.ESC_dollors_nL_nH.Length;
            CMD.GS_w_n[2] = (byte)nWidthX;
            CMD.GS_w_n.CopyTo(array, num2);
            num2 += CMD.GS_w_n.Length;
            CMD.GS_h_n[2] = (byte)nHeight;
            CMD.GS_h_n.CopyTo(array, num2);
            num2 += CMD.GS_h_n.Length;
            CMD.GS_f_n[2] = (byte)((uint)nHriFontType & 1u);
            CMD.GS_f_n.CopyTo(array, num2);
            num2 += CMD.GS_f_n.Length;
            CMD.GS_H_n[2] = (byte)((uint)nHriFontPosition & 3u);
            CMD.GS_H_n.CopyTo(array, num2);
            num2 += CMD.GS_H_n.Length;
            CMD.GS_k_m_n_[2] = (byte)nType;
            CMD.GS_k_m_n_[3] = (byte)bytes.Length;
            CMD.GS_k_m_n_.CopyTo(array, num2);
            num2 += CMD.GS_k_m_n_.Length;
            bytes.CopyTo(array, num2);
            PrintBytes(array);
        }
    }

    /// <summary>
    /// distance * 0.125mm
    /// </summary>
    /// <param name="distance"></param>
    static void CutPaper(byte distance = 0)
    {
        byte[] cmd = CMD.GS_V_m_n;
        cmd[2] = 66;
        cmd[3] = distance;

        PrintBytes(cmd);
    }

    /// <summary>
    /// distance * 0.125mm
    /// </summary>
    /// <param name="distance"></param>
    static void PrintSpace(byte distance)
    {
        byte prevDistance = lineSpacing;
        SetLineSpacing(distance);

        PrintEmptyLine();

        SetLineSpacing(prevDistance);
    }

    static void SetLineSpacing(byte distance)
    {
        lineSpacing = distance;
        byte[] cmd = CMD.ESC_3_n;
        cmd[2] = distance;
        PrintBytes(cmd);
    }

    static void PrintEmptyLine(byte lines = 1)
    {
        byte[] cmd = CMD.ESC_d_n;
        cmd[2] = lines;

        PrintBytes(cmd);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="size">0-7</param>
    /// <param name="justification">left=0, center=1, right=2</param>
    static void PrintLine(string text, int size = 0, Justification justification = 0)
    {
        byte[] widths = new byte[8] { 0, 16, 32, 48, 64, 80, 96, 112 };
        byte[] heights = new byte[8] { 0, 1, 2, 3, 4, 5, 6, 7 };
        byte[] cmd = CMD.GS_exclamationmark_n;
        cmd[2] = (byte)(widths[size] + heights[size]);
        PrintBytes(cmd);

        cmd = CMD.ESC_a_n;
        cmd[2] = (byte)justification;
        PrintBytes(cmd);

        cmd = CMD.LF;
        byte[] byteText = Encoding.Default.GetBytes(text);

        PrintBytes(ConcatArray(cmd, byteText));


    }

    static void PrintBytes(byte[] data)
    {
        try
        {
            string tempFilePath = "printerFile.txt";
            File.WriteAllBytes(tempFilePath, data);
            File.Copy(tempFilePath, printer);
        }
        catch (Exception e)
        {
            GD.PrintErr(e);
        }
    }

    public static void ConnectToPrinter()
    {
        string PCName = System.Environment.MachineName;
        // GD.Print($"Count: {PrinterSettings.InstalledPrinters.Count}");
        foreach (string printerName in PrinterSettings.InstalledPrinters)
        {
            printer = @$"\\{PCName}\" + printerName;
            // GD.Print(PCName);
            break;
        }
    }

    static public string[] CreateItemString(string name, int quantity, decimal singleOriginalPrice, decimal noDiscountPrice, decimal subTotalPrice, decimal discount)
    {
        bool isDiscounted = discount > 0;
        string itemText = $"{name}";
        string quantityText = $"     {quantity} @ {singleOriginalPrice:C}".PadRight(39, ' ') + $"{noDiscountPrice:C}".PadRight(9, ' ');
        string discountText = isDiscounted ? $"     DISCOUNT {discount * 100}%".PadRight(38, ' ') + $"-{noDiscountPrice - subTotalPrice:C}".PadRight(10, ' ') : null;

        string[] ret = { itemText, quantityText, discountText };
        return ret;
    }

    static public void PrintReceiptFromData(int purchaseId)
    {
        const int columnCount = 9;
        string sql =
        @$"DROP TABLE IF EXISTS CPITemp;
            DROP TABLE IF EXISTS ProductTemp;

            CREATE TABLE CPITemp AS
            SELECT ItemId AS Id, Discount, Price, Quantity
            FROM CustomerPurchaseItem
            WHERE PurchaseId = {purchaseId};

            CREATE TABLE ProductTemp AS
            SELECT Id, Name, GST, PST, EnviromentalFee, BottleDepositFee
            FROM Products;

            SELECT * 
            FROM CPITemp NATURAL JOIN ProductTemp;";

        List<List<object>> rows = QueryAndRead(sql, columnCount);
        Array<string[]> items = new Array<string[]>();
        if (rows != null && rows.Count > 0)
        {
            decimal discount = Convert.ToDecimal(rows[1]);
            decimal price = Convert.ToDecimal(rows[2]);
            int quantity = Convert.ToInt32(rows[3]);
            string name = Convert.ToString(rows[4]);
            bool hasGST = Convert.ToBoolean(rows[5]);
            bool hasPST = Convert.ToBoolean(rows[6]);
            bool hasEnviroFee = Convert.ToBoolean(rows[7]);
            bool hasbottleDeposit = Convert.ToBoolean(rows[8]);
            items.Add(CreateItemString(name: name,
            quantity: quantity,
            singleOriginalPrice: price,
            noDiscountPrice: price * quantity,
            subTotalPrice: CalculateTotal(quantity, price, hasGST, hasPST, hasEnviroFee, hasbottleDeposit),
            discount: discount));
        }

        PrintReceipt(items);
    }

    static public void PrintReceipt(Array<string[]> ItemData = null)
    {
        SetLineSpacing(5);

        PrintLine("SOZAIYA", 1, Justification.MIDDLE);
        PrintLine("2906 EAST 2ND AVE", 0, Justification.MIDDLE);
        PrintLine("VANCOUVER, BC V5M 1E6", 0, Justification.MIDDLE);
        PrintLine("INSTAGRAM.COM/SozaiyaCanada", 0, Justification.MIDDLE);
        PrintLine("Authentic Japanese Goods", 0, Justification.MIDDLE);
        PrintEmptyLine(2);

        if (ItemData == null)
        {
            ItemData = new Array<string[]>();
            foreach (Item item in GetAllItems())
            {
                ItemData.Add(item.ToText());
            }
        }

        // Print items
        foreach (string[] texts in ItemData)
        {
            foreach (string text in texts)
            {
                if (text == null) { continue; }
                PrintLine(text);
            }
            PrintEmptyLine();
        }

        PrintEmptyLine(2);
        PrintLine($"Subtotal".PadRight(39, ' ') + $"{subtotal:C}".PadRight(9, ' '));
        PrintEmptyLine();
        PrintLine($"Total".PadRight(39, ' ') + $"{total:C}".PadRight(9, ' '));

        PrintEmptyLine();
        PrintLine("Thank you for shopping with us");
        PrintEmptyLine();

        string barcodeNum = $"ORD" + $"{lastOrderID}".PadLeft(12 - 3, '0');
        PrintLine($"{barcodeNum}", justification: Justification.MIDDLE);
        PrintBarcode(barcodeNum, 280, 0x41, 2, 80, 0, 0);

        PrintEmptyLine(2);
        CutPaper();
    }



}