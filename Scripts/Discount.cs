using Godot;
using System;

public partial class Discount : PanelContainer
{
    public static PackedScene DISCOUNT_SCENE;
    public static ItemScrollContainer itemList;

    [Export] Label NameLabel;
    [Export] Label DiscountLabel;


    Item discountedItem;
    decimal percent;

    static public Node NewDiscount(Item discountedItem, int percent, int index = -1)
    {
        if (discountedItem.isDiscounted())
        {
            discountedItem.RemoveDiscount();
        }

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
            discountedItem.PriceChanged -= OnDiscountedItemChanged;
            QueueFree();
        }
        else
        {
            decimal newPrice = discountedItem.GetNoDiscountPrice();
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