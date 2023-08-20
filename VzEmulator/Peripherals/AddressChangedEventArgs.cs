using System;
public class AddressChangedEventArgs : EventArgs
{
    public int Address { get; set; }
    public byte OldData { get; set; }
    public byte NewData { get; set; }

    public AddressChangedEventArgs(int address, byte oldData, byte newData)
    {
        Address = address;
        OldData = oldData;
        NewData = newData;
    }
}
