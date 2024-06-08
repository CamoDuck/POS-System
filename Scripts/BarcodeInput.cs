using Godot;
using System;
using System.IO.Ports;

public partial class BarcodeInput : Control
{
    [Signal] public delegate void BarcodeReadEventHandler(string barcode);

    [Export] public LineEdit barcodeInput;


    private SerialPort port = new SerialPort("COM4", 9600, Parity.None, 8, StopBits.One);

    public override void _Ready()
    {
        // Attach a method to be called when there
        // is data waiting in the port's buffer 
        port.DataReceived += new SerialDataReceivedEventHandler(_OnPortDataReceived);
        // Begin communications 
        port.Open();
    }
    public override void _ExitTree()
    {
        port.Close();
    }

    void _OnPortDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        // Show all the incoming data in the port's buffer
        string barcode = port.ReadExisting();
        CallDeferred("emit_signal", SignalName.BarcodeRead, barcode);
    }

}
