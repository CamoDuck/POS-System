using Godot;
using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Security.Cryptography;

public partial class BarcodeInput : Control
{
    [Signal] public delegate void BarcodeReadEventHandler(string barcode);

    [Export] public LineEdit barcodeInput;

    StringBuilder inputBuffer = new StringBuilder();

    public override void _Ready()
    {
    }


    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && @event.IsPressed())
        {
            string key = OS.GetKeycodeString(keyEvent.Keycode);
            inputBuffer.Append(key);
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("StartBarcode"))
        {
            barcodeInput.GrabFocus();
            barcodeInput.Text = "";
            Show();
        }
        else if (Input.IsActionJustPressed("EndBarcode"))
        {
            EmitSignal(SignalName.BarcodeRead, inputBuffer.ToString().Replace("F1", "").Replace("Enter", ""));
            Hide();
            inputBuffer.Clear();
        }

    }

    public string GetBarcode()
    {
        return barcodeInput.Text;
    }

}
