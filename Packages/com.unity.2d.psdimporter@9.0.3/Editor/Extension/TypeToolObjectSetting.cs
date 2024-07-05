using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PhotoshopFile;
using UnityEngine;

internal class TypeToolObjectSetting
{
    private string signature;
    public string Signature
    {
        get { return signature; }
    }

    private string key;
    public string Key
    {
        get { return key; }
    }

    private byte[] data;
    public byte[] Data
    {
        get { return data; }
    }


    public short Version { get; }
    
    // xx xy yx yy tx ty
    public double[] Transform { get; }
    public short TextVersion { get; }
    public int TextDescriptorVersion { get; }
    public DescriptorStructure TextData { get; }
    public short WarpVersion { get; }
    public int WrapDescriptorVersion { get; }
    public DescriptorStructure WarpData { get; }
    
    // Left Top Right Bottom
    public int[] Padding { get; }


    public TypeToolObjectSetting(byte[] data, string signature, string key)
    {
        this.signature = signature;
        this.key = key;
        this.data = data;

        using var ms = new MemoryStream(Data);
        using var r = new PsdBinaryReader(ms, Encoding.Default);
        Version = r.ReadInt16();
        Transform = new double[6];
        for (var i = 0; i < 6; i++) Transform[i] = r.ReadDouble();
        TextVersion = r.ReadInt16();
        TextDescriptorVersion = r.ReadInt32();
        TextData = new DescriptorStructure(r);
        WarpVersion = r.ReadInt16();
        WrapDescriptorVersion = r.ReadInt32();
        WarpData = new DescriptorStructure(r);
        Padding = new int[4];
        for (var i = 0; i < 4; i++) Padding[i] = r.ReadInt32();
    }


    public static bool TryParse(RawLayerInfo layerInfo, out TypeToolObjectSetting ttos)
    {
        if (layerInfo == null || layerInfo.Key != "TySh")
        {
            ttos = null;
            return false;
        }
        else
        {
            ttos = new TypeToolObjectSetting(layerInfo.Data, layerInfo.Signature, layerInfo.Key);
            return true;
        }
    }

}