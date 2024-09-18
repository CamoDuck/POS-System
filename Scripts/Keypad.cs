using Godot;
using System;

public partial class Keypad : Control
{
    public enum Key
    {
        Zero = 0,
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Delete = -1,
        Dot = -2,
    };

    // [Signal] public delegate void OnKeyPressedEventHandler(int value);

    [Export] SpinBox box;
    string currentText = "";

    public void Clear() {
        currentText = "";
    }

    public void _OnKeyPressed(int value)
    {
        Key keyType = (Key)value;

        if (keyType == Key.Delete)
        {
            if (currentText.Length > 0)
            {
                currentText = currentText[..^1];
            }
        }
        else if (keyType == Key.Dot)
        {
            if (currentText.IndexOf(".") == -1)
            {
                currentText += ".";
            }
        }
        else // If numeric
        {
            if (currentText.IndexOf(".") == -1 || currentText.IndexOf(".") >= currentText.Length - 2)
            {
                currentText += $"{value}";
            }
        }


        if (currentText == "")
        {
            box.Value = 0;
            return;
        }
        box.Value = Convert.ToDouble(currentText);
        box.GetLineEdit().Text = $"{box.Prefix} {currentText} {box.Suffix}";

    }

}
