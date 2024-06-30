using Godot;
using System;
using System.Collections.Generic;

public partial class AddItemByNamePopup : Control
{
    [Export] LineEdit searchbar;
    [Export] ItemList itemList;
    [Export] OptionButton typeButton;

    List<Global.Product> products;

    public void Open()
    {
        searchbar.Clear();
        UpdateTypeButton();
        UpdateItemList();
        Show();
    }

    void Close()
    {
        Hide();
    }

    public void _OnCloseButtonPressed()
    {
        Close();
    }

    public void _OnSearchTextChanged(string newText)
    {
        UpdateItemList();
    }

    public void _OnTypeChanged(int index)
    {
        UpdateItemList();
    }

    public void _OnItemSelected(int index)
    {
        Global.Product values = products[index];
        Item.NewItem(values);
        Close();
    }

    void UpdateItemList()
    {
        string searchString = searchbar.Text;
        int typeIndex = typeButton.GetSelectedId();
        string type = typeIndex > 0 ? typeButton.GetItemText(typeIndex) : null;
        products = Global.GetProductsByName(searchString, type);

        itemList.Clear();
        if (products == null) { return; }

        foreach (Global.Product p in products)
        {
            if (p == null) { break; }

            string name = p.name;
            string barcode = p.barcode;
            decimal price = p.price;

            itemList.AddItem($"({barcode}) {name} - ${price:C}");
        }
    }

    void UpdateTypeButton()
    {
        typeButton.Clear();
        List<string> types = Global.GetProductTypes();

        typeButton.AddItem("All", 0);
        foreach (string type in types)
        {
            typeButton.AddItem(type);
        }
    }

}
