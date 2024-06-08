using System.Reflection.Metadata;
using Godot;

public static class Global
{
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
}