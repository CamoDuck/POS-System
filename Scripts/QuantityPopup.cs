using Godot;
using System;

public partial class QuantityPopup : Panel
{
    [Export] SpinBox quantityEdit;

    Action<int> LabelCallback;

    public void Start(string name, int quantity, Action<int> labelCallback)
    {
        this.LabelCallback = labelCallback;
        UpdateLabels(quantity);
        Show();
    }

    public void onDonePressed()
    {
        LabelCallback(GetQuantity());
        Hide();
    }

    public void _OnQuantityTextChanged(float newValue)
    {
        UpdateLabels(Mathf.RoundToInt(newValue));
    }

    public void _OnQuantityButtonPressed(int change)
    {
        int newValue = GetQuantity() + change;
        UpdateLabels(newValue);
    }

    void UpdateLabels(int newValue)
    {
        quantityEdit.Value = newValue;
    }

    int GetQuantity()
    {
        return Mathf.RoundToInt(quantityEdit.Value);
    }

}
