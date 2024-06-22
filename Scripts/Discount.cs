using Godot;
using System;

public partial class Discount : PanelContainer
{
    public static PackedScene DISCOUNT_SCENE;

    [Export] Label NameLabel;
    [Export] Label DiscountLabel;

    Item discountedItem;
    decimal percent;

    static public Node NewDiscount(ItemScrollContainer itemList, Item discountedItem, int percent, int index = -1)
    {
        Discount newDiscount = DISCOUNT_SCENE.Instantiate<Discount>();

        discountedItem.SetDiscount(percent);
        discountedItem.PriceChanged += newDiscount.OnDiscountedItemChanged;
        newDiscount.discountedItem = discountedItem;
        newDiscount.SetPercent(percent);
        newDiscount.SetName(percent);
        newDiscount.OnDiscountedItemChanged();

        itemList.AddToList(newDiscount, false, index);

        return newDiscount;
    }

    void OnDiscountedItemChanged(bool isDeleted = false)
    {
        if (isDeleted)
        {
            QueueFree();
        }
        else
        {
            decimal newPrice = discountedItem.GetSubTotalPrice();
            decimal discountedAmount = newPrice * percent;
            SetDiscount(discountedAmount);
        }
    }

    void SetPercent(int percent)
    {
        this.percent = percent / 100.0m;
    }

    void SetName(decimal percent)
    {
        string name = $"DISCOUNT {percent}%";
        NameLabel.Text = name;
    }

    void SetDiscount(decimal amount)
    {
        DiscountLabel.Text = $"-{amount:C}";
    }

}