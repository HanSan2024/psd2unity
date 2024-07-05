using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PaintDotNet.Data.PhotoshopFileType;
using PDNWrapper;
using UnityEngine;

namespace PhotoshopFile
{
    internal class EffectLayerInfo 
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

        public short Version;
        public Dictionary<string, EffectInfo> Effects;

        public EffectLayerInfo(byte[] data, string signature, string key)
        {
            this.signature = signature;
            this.key = key;
            this.data = data;

            using var ms = new MemoryStream(Data);
            using var r = new PsdBinaryReader(ms, Encoding.Default);
            Version = r.ReadInt16();
            var count = r.ReadInt16();
            Effects = new Dictionary<string, EffectInfo>();
            for (var i = 0; i < count; i++)
            {
                var sig = r.ReadAsciiChars(4);
                var ostype = r.ReadAsciiChars(4);
                switch (ostype)
                {
                    case "cmnS":
                        Effects[ostype] = new CommonStateInfo(r);
                        break;
                    case "dsdw":
                        Effects[ostype] = new ShadowInfo(r);
                        break;
                    case "isdw":
                        Effects[ostype] = new ShadowInfo(r);
                        break;
                    case "oglw":
                        Effects[ostype] = new OuterGlowInfo(r);
                        break;
                    case "iglw":
                        Effects[ostype] = new InnerGlowInfo(r);
                        break;
                    case "bevl":
                        Effects[ostype] = new BevelInfo(r);
                        break;
                    case "sofi":
                        Effects[ostype] = new SolidFillInfo(r);
                        break;
                    default:
                        Debug.LogError($"Not Implemented:{ostype}");
                        break;
                }
            }
        }

        public static bool TryParse(RawLayerInfo layerInfo, out EffectLayerInfo eli)
        {
            if (layerInfo == null || layerInfo.Key != "lrFX")
            {
                eli = null;
                return false;
            }
            else
            {
                eli = new EffectLayerInfo(layerInfo.Data, layerInfo.Signature, layerInfo.Key);
                return true;
            }
        }


        public ShadowInfo GetShadowInfo()
        {
            Effects.TryGetValue("dsdw", out var info);
            return info as ShadowInfo;
        }
        
        
        public override string ToString()
        {
            var sb = new StringBuilder(300);
            sb.AppendLine("EffectLayerInfo");
            sb.AppendLine($"Key:{Key}");
            sb.AppendLine($"Version:{Version}");
            foreach (var e in Effects)
            {
                sb.AppendLine($"{e.Key}:{{{e.Value}}}");
            }

            return sb.ToString();
        }

        public class EffectInfo
        {
            protected Color ReadColor(PsdBinaryReader reader)
            {
                var space = reader.ReadUInt16();
                var r = reader.ReadUInt16();
                var g = reader.ReadUInt16();
                var b = reader.ReadUInt16();
                var a = reader.ReadUInt16();
                return new Color(r / 65535f, g / 65535f, b / 65535f, a / 65535f);
            }

            protected LayerBlendMode ReadBlendMode(PsdBinaryReader reader)
            {
                var signature = reader.ReadAsciiChars(4);
                if (signature != "8BIM")
                    throw new PsdInvalidException("Invalid section divider signature.");
                return BlendModeMapping.FromPsdBlendMode(reader.ReadAsciiChars(4));
            }


            protected int ReadInt32(PsdBinaryReader reader)
            {
                var i = reader.ReadInt32();
                return i >> 16 | i << 16;
            }

        }
        
        // 公共
        public class CommonStateInfo : EffectInfo
        {
            public int Size { get; }
            public int Version { get; }
            public bool Visible { get; }
            public int Reservation { get; }

            public CommonStateInfo(PsdBinaryReader reader)
            {
                Size = reader.ReadInt32();
                Version = reader.ReadInt32();
                Visible = reader.ReadByte() > 0;
                Reservation = reader.ReadInt16();
            }


            public override string ToString()
            {
                return $"[公共]    Size:{Size}    Version:{Version}   Visible:{Visible}   Reservation:{Reservation}";
            }
            
        }

        // 阴影
        public class ShadowInfo : EffectInfo
        {
            public int Size { get; }
            public int Version { get; }
            public int Blur { get; }
            public int Intensity { get; }
            public int Angle { get; }
            public int Distance { get; }
            public Color Color { get; }
            public LayerBlendMode BlendMode { get; }
            public bool Enabled { get; }
            public bool UseForAll { get; }
            public byte Opacity { get; }
            public Color NativeColor { get; }


            public ShadowInfo(PsdBinaryReader reader)
            {
                Size = reader.ReadInt32();
                Version = reader.ReadInt32();
                Blur = ReadInt32(reader);
                Intensity = ReadInt32(reader);
                Angle = ReadInt32(reader);
                Distance = ReadInt32(reader);
                Color = ReadColor(reader);
                BlendMode = ReadBlendMode(reader);
                Enabled = reader.ReadByte() > 0;
                UseForAll = reader.ReadByte() > 0;
                Opacity = reader.ReadByte();
                NativeColor = ReadColor(reader);
            }

            public override string ToString()
            {
                return $"[阴影]    Size:{Size}    Version:{Version}    Blur:{Blur}    Intensity:{Intensity}    Angle:{Angle}    " +
                       $"Distance:{Distance}    Color:{Color}    BlendMode:{BlendMode}    Enabled:{Enabled}    UseForAll:{UseForAll}    " +
                       $"Opacity:{Opacity}    NativeColor:{NativeColor}";
            }
        }

        // 自发光
        public class GlowInfo : EffectInfo
        {
            public int Size { get; }
            public int Version { get; }
            public int Blur { get; }
            public int Intensity { get; }
            public Color Color { get; }
            public LayerBlendMode BlendMode { get; }
            public bool Enabled { get; }
            public byte Opacity { get; }
            public Color NativeColor { get; }

            public GlowInfo(PsdBinaryReader reader)
            {
                Size = reader.ReadInt32();
                Version = reader.ReadInt32();
                Blur = ReadInt32(reader);
                Intensity = ReadInt32(reader);
                Color = ReadColor(reader);
                BlendMode = ReadBlendMode(reader);
                Enabled = reader.ReadByte() > 0;
                Opacity = reader.ReadByte();
            }
            
        }
        
        public class OuterGlowInfo : GlowInfo
        {
            public Color NativeColor { get; }

            public OuterGlowInfo(PsdBinaryReader reader) : base(reader)
            {
                if (Version == 2)
                {
                    NativeColor = ReadColor(reader);
                }
            }
            
            
            public override string ToString()
            {
                return $"[外发光]    Size:{Size}    Version:{Version}    Blur:{Blur}    Intensity:{Intensity}    Color:{Color}    " +
                       $"BlendMode:{BlendMode}    Enabled:{Enabled}    Opacity:{Opacity}    NativeColor:{NativeColor}";
            }
        }
        
        public class InnerGlowInfo : GlowInfo
        {
            public byte Invert { get; }
            public Color NativeColor { get; }

            public InnerGlowInfo(PsdBinaryReader reader) : base(reader)
            {
                if (Version == 2)
                {
                    Invert = reader.ReadByte();
                    NativeColor = ReadColor(reader);
                }
            }
            
            
            public override string ToString()
            {
                return $"[内发光]    Size:{Size}    Version:{Version}    Blur:{Blur}    Intensity:{Intensity}    Color:{Color}    " +
                       $"BlendMode:{BlendMode}    Enabled:{Enabled}    Opacity:{Opacity}    Invert:{Invert}    " +
                       $"NativeColor:{NativeColor}";
            }
        }

        // 斜角
        public class BevelInfo : EffectInfo
        {
            public int Size { get; }
            public int Version { get; }
            public int Angle { get; }
            public int Strength { get; }
            public int Blur { get; }
            public LayerBlendMode HighlightBlendMode { get; }
            public LayerBlendMode ShadowBlendMode { get; }
            public Color HighlightColor { get; }
            public Color ShadowColor { get; }
            public byte BevelStyle { get; }
            public byte HighlightOpacity { get; }
            public byte ShadowOpacity { get; }
            public bool Enabled { get; }
            public bool UseForAll { get; }
            public byte UpOrDown { get; }

            public Color RealHighlightColor { get; }
            public Color RealShadowColor { get; }

            public BevelInfo(PsdBinaryReader reader)
            {
                Size = reader.ReadInt32();
                Version = reader.ReadInt32();
                Angle = ReadInt32(reader);
                Strength = reader.ReadInt32();
                Blur = ReadInt32(reader);
                HighlightBlendMode = ReadBlendMode(reader);
                ShadowBlendMode = ReadBlendMode(reader);
                HighlightColor = ReadColor(reader);
                ShadowColor = ReadColor(reader);
                BevelStyle = reader.ReadByte();
                HighlightOpacity = reader.ReadByte();
                ShadowOpacity = reader.ReadByte();
                Enabled = reader.ReadByte() > 0;
                UseForAll = reader.ReadByte() > 0;
                UpOrDown = reader.ReadByte();
                
                if (Version == 2)
                {
                    RealHighlightColor = ReadColor(reader);
                    RealShadowColor = ReadColor(reader);
                }
            }
            
            
            public override string ToString()
            {
                return $"[斜角]    Size:{Size}    Version:{Version}    Angle:{Angle}    Strength:{Strength}    Blur:{Blur}    " +
                       $"HighlightBlendMode:{HighlightBlendMode}    ShadowBlendMode:{ShadowBlendMode}    HighlightColor:{HighlightColor}    " +
                       $"ShadowColor:{ShadowColor}    BevelStyle:{BevelStyle}    HighlightOpacity:{HighlightOpacity}    " +
                       $"ShadowOpacity:{ShadowOpacity}    Enabled:{Enabled}    UseForAll:{UseForAll}    UpOrDown:{UpOrDown}";
            }
            
        }

        // 实体填充
        public class SolidFillInfo : EffectInfo
        {
            public int Size { get; }
            public int Version { get; }
            public LayerBlendMode BlendMode { get; }
            public Color Color { get; }
            public byte Opacity { get; }
            public bool Enabled { get; }
            public Color NativeColor { get; }

            public SolidFillInfo(PsdBinaryReader reader)
            {
                Size = reader.ReadInt32();
                Version = reader.ReadInt32();
                BlendMode = ReadBlendMode(reader);
                Color = ReadColor(reader);
                Opacity = reader.ReadByte();
                Enabled = reader.ReadByte() > 0;
                NativeColor = ReadColor(reader);
            }
            
            
            public override string ToString()
            {
                return $"[固体填充]    Size:{Size}    Version:{Version}    BlendMode:{BlendMode}    Color:{Color}    Opacity:{Opacity}    " +
                       $"Enabled:{Enabled}    NativeColor:{NativeColor}    ";
            }
        }
    }

}