using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PhotoshopFile
{
    internal abstract class Structure
    {

    }

    internal class DescriptorStructure : Structure
    {
        public string Name;
        public int ClassID;
        public byte[] ClassName;
        public int Count;
        public Dictionary<string, object> Items;

        // engine data解析数据
        public EngineDataNode EngineData;

        internal DescriptorStructure(PsdBinaryReader reader)
        {
            Name = reader.ReadUnicodeString();
            var len = reader.ReadInt32();
            if (len == 0)
            {
                ClassID = reader.ReadInt32();
            }
            else
            {
                ClassName = reader.ReadBytes(len);
            }

            Count = reader.ReadInt32();
            Items = new Dictionary<string, object>(Count);
            for (int i = 0; i < Count; i++)
            {
                var l = reader.ReadInt32();
                var key = reader.ReadAsciiChars(l == 0 ? 4 : l);
                var ostype = reader.ReadAsciiChars(4);
                object value = null;
                switch (ostype)
                {
                    case "obj ":
                        value = new ReferenceStructure(reader);
                        break;
                    case "Objc":
                        value = new DescriptorStructure(reader);
                        break;
                    case "VlLs":
                        value = new ListStructure(reader);
                        break;
                    case "doub":
                        value = reader.ReadDouble();
                        break;
                    case "UntF":
                        value = new UintFloatStructure(reader);
                        break;
                    case "TEXT":
                        value = reader.ReadUnicodeString();
                        break;
                    case "enum":
                        value = new EnumeratedStructure(reader);
                        break;
                    case "long":
                        value = reader.ReadInt32();
                        break;
                    case "comp":
                        value = reader.ReadInt64();
                        break;
                    case "bool":
                        value = reader.ReadByte();
                        break;
                    case "GlbO":
                        value = new GlobalObjectStructure(reader);
                        break;
                    case "type":
                    case "GlbC":
                        value = new ClassStructure(reader);
                        break;
                    case "alis":
                        value = new AliasStructure(reader);
                        break;
                    case "tdta":
                        var tdta = new RawDataStructure(reader);
                        EngineData = EngineDataParser.Parse(tdta.RawData);
                        value = tdta;
                        // var str = Encoding.UTF8.GetString(tdta.RawData);
                        // File.WriteAllText("Assets/test_psd.txt", str);
                        break;
                }

                Items[key] = value;
            }
        }
    }


    internal class ReferenceStructure : Structure
    {
        public int Count;
        public Dictionary<string, object> Items;

        public ReferenceStructure(PsdBinaryReader reader)
        {
            Count = reader.ReadInt32();
            Items = new Dictionary<string, object>(Count);
            for (int i = 0; i < Count; i++)
            {
                var key = reader.ReadAsciiChars(4);
                object value = null;
                switch (key)
                {
                    case "prop":
                        value = new PropertyStructure(reader);
                        break;
                    case "Clss":
                        value = new ClassStructure(reader);
                        break;
                    case "Enmr":
                        value = new EnumeratedReferenceStructure(reader);
                        break;
                    case "rele":
                        value = new OffsetStructure(reader);
                        break;
                    case "Idnt":
                        value = reader.ReadInt16();
                        break;
                    case "indx":
                        value = reader.ReadInt16();
                        break;
                    case "name":
                        value = reader.ReadUnicodeString();
                        break;
                }

                Items[key] = value;
            }
        }
    }


    internal class ListStructure : Structure
    {
        public int Count;
        public Dictionary<string, object> Items;

        public ListStructure(PsdBinaryReader reader)
        {
            Items = new Dictionary<string, object>();
            Count = reader.ReadInt32();
            for (int i = 0; i < Count; i++)
            {
                var key = reader.ReadAsciiChars(4);
                object value = null;
                switch (key)
                {
                    case "obj ":
                        value = new ReferenceStructure(reader);
                        break;
                    case "Objc":
                        value = new DescriptorStructure(reader);
                        break;
                    case "VlLs":
                        value = new ListStructure(reader);
                        break;
                    case "doub":
                        value = reader.ReadDouble();
                        break;
                    case "UntF":
                        value = new UintFloatStructure(reader);
                        break;
                    case "TEXT":
                        value = reader.ReadUnicodeString();
                        break;
                    case "enum":
                        value = new EnumeratedStructure(reader);
                        break;
                    case "long":
                        value = reader.ReadInt32();
                        break;
                    case "comp":
                        value = reader.ReadInt64();
                        break;
                    case "bool":
                        value = reader.ReadByte();
                        break;
                    case "GlbO":
                        value = new GlobalObjectStructure(reader);
                        break;
                    case "type":
                    case "GlbC":
                        value = new ClassStructure(reader);
                        break;
                    case "alis":
                        value = new AliasStructure(reader);
                        break;
                    case "tdta":
                        value = new RawDataStructure(reader);
                        break;
                }

                Items[key] = value;
            }
        }
    }


    internal class UintFloatStructure : Structure
    {
        public string Units;
        public double Value;

        public UintFloatStructure(PsdBinaryReader reader)
        {
            Units = reader.ReadAsciiChars(4);
            Value = reader.ReadDouble();
        }
    }


    internal class EnumeratedStructure : Structure
    {
        public string TypeID;
        public string Enum;

        public EnumeratedStructure(PsdBinaryReader reader)
        {
            var len = reader.ReadInt32();
            TypeID = len == 0 ? reader.ReadInt32().ToString() : reader.ReadAsciiChars(len);
            len = reader.ReadInt32();
            Enum = len == 0 ? reader.ReadInt32().ToString() : reader.ReadAsciiChars(len);
        }
    }


    internal class EnumeratedReferenceStructure : Structure
    {
        public string ClassName;
        public string ClassID;
        public string TypeID;
        public string Enum;

        public EnumeratedReferenceStructure(PsdBinaryReader reader)
        {
            ClassName = reader.ReadUnicodeString();
            var len = reader.ReadInt32();
            ClassID = len == 0 ? reader.ReadInt32().ToString() : reader.ReadAsciiChars(len);
            len = reader.ReadInt32();
            TypeID = len == 0 ? reader.ReadInt32().ToString() : reader.ReadAsciiChars(len);
            len = reader.ReadInt32();
            Enum = len == 0 ? reader.ReadInt32().ToString() : reader.ReadAsciiChars(len);
        }
    }


    internal class GlobalObjectStructure : DescriptorStructure
    {
        public GlobalObjectStructure(PsdBinaryReader reader) : base(reader)
        {

        }
    }


    internal class ClassStructure : Structure
    {
        public string ClassName;
        public string ClassID;

        internal ClassStructure(PsdBinaryReader reader)
        {
            ClassName = reader.ReadUnicodeString();
            var len = reader.ReadInt32();
            ClassID = len == 0 ? reader.ReadInt32().ToString() : reader.ReadAsciiChars(len);
        }
    }

    internal class AliasStructure : Structure
    {
        public byte[] Data;

        public AliasStructure(PsdBinaryReader reader)
        {
            var len = reader.ReadInt32();
            Data = reader.ReadBytes(len);
        }
    }


    internal class RawDataStructure : Structure
    {
        public int Length;
        public byte[] RawData;

        public RawDataStructure(PsdBinaryReader reader)
        {
            Length = reader.ReadInt32();
            RawData = reader.ReadBytes(Length);
        }
    }


    internal class OffsetStructure : Structure
    {
        public string ClassName;
        public string ClassID;
        public int Value;

        public OffsetStructure(PsdBinaryReader reader)
        {
            ClassName = reader.ReadUnicodeString();
            var len = reader.ReadInt32();
            ClassID = len == 0 ? reader.ReadInt32().ToString() : reader.ReadAsciiChars(len);
            Value = reader.ReadInt32();
        }
    }


    internal class PropertyStructure : Structure
    {
        public string ClassName;
        public string ClassID;
        public string KeyID;

        public PropertyStructure(PsdBinaryReader reader)
        {
            ClassName = reader.ReadUnicodeString();
            var len = reader.ReadInt32();
            ClassID = len == 0 ? reader.ReadInt32().ToString() : reader.ReadAsciiChars(len);
            len = reader.ReadInt32();
            KeyID = len == 0 ? reader.ReadInt32().ToString() : reader.ReadAsciiChars(len);
        }
    }

}