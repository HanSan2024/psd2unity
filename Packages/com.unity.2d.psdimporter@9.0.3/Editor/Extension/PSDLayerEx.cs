using System;
using System.Collections;
using System.Collections.Generic;
using PhotoshopFile;
using TMPro;
using UnityEditor.U2D.PSD;
using UnityEngine;

public static class PSDLayerEx
{
    internal static bool IsTextLayer(this PSDLayer layer)
    {
        return layer.GetLayerTextData(out _) != null;
    }

    public struct LayerTextData
    {
        public string text;
        public float fontSize;
        public Color color;
        public FontStyles fontStyles;
        public TextAlignmentOptions aligment;
        public Vector4 margins;
        public float characterSpacing;
        public float lineSpacing;
        public float paragraphSpacing;
        public float wordSpacing;
        public float lineSpacingAdjustment;
        public bool enableVertexGradient;
        public VertexGradient colorGradient;
        public bool enableKerning;
    }


    internal static LayerTextData? GetLayerTextData(this PSDLayer layer, out TypeToolObjectSetting ttos)
    {
        try
        {
            var infos = layer.bitmapLayer.PsdLayer.AdditionalInfo;
            ttos = null;
            foreach (var li in infos)
            {
                ttos = li.ToTypeToolObjectSetting();
                if (ttos != null) break;
            }

            if (ttos != null)
            {
                var td = new LayerTextData();
                var engineData = ttos.TextData.EngineData;
                var engineDict = engineData["EngineDict"];
                td.text = engineDict["Editor"]["Text"].textValue.Trim();
                var styleSheetData = engineDict["StyleRun"]["RunArray"][0]["StyleSheet"]["StyleSheetData"];
                td.fontSize = styleSheetData["FontSize"].floatValue;
                var fc = styleSheetData["FillColor"]["Values"];
                td.color = new Color(fc[1].floatValue, fc[2].floatValue, fc[3].floatValue, fc[0].floatValue);

                var styles = FontStyles.Normal;
                var blod = styleSheetData["FauxBold"]?.boolValue ?? false;
                var italic = styleSheetData["FauxItalic"]?.boolValue ?? false;
                var underline = styleSheetData["Underline"]?.boolValue ?? false;
                var strikethrough = styleSheetData["Strikethrough"]?.boolValue ?? false;
                var fontcaps = styleSheetData["FontCaps"]?.intValue ?? 0; // 2全大写  1选小写  0正常
                var fcase = fontcaps switch
                {
                    1 => FontStyles.LowerCase,
                    2 => FontStyles.UpperCase,
                    _ => FontStyles.Normal
                };
                styles |= blod ? FontStyles.Bold : styles;
                styles |= italic ? FontStyles.Italic : styles;
                styles |= underline ? FontStyles.Underline : styles;
                styles |= strikethrough ? FontStyles.Strikethrough : styles;
                styles |= fcase;
                td.fontStyles = styles;

                var paragraphSheet = engineDict["ParagraphRun"]["RunArray"][0]["ParagraphSheet"]["Properties"];
                var aligment = paragraphSheet["Justification"]?.intValue ?? 0;
                td.aligment = aligment switch
                {
                    0 => TextAlignmentOptions.MidlineLeft,
                    1 => TextAlignmentOptions.MidlineRight,
                    2 => TextAlignmentOptions.Midline,
                    3 => TextAlignmentOptions.TopLeft,
                    4 => TextAlignmentOptions.TopRight,
                    5 => TextAlignmentOptions.Top,
                    6 => TextAlignmentOptions.TopFlush,
                    _ => TextAlignmentOptions.MidlineLeft
                };

                return td;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        ttos = null;
        return default;
    }


    public struct LayerEffectData
    {
        public bool shadowEnabled;
        public int shadowDistance;
        public int shadowAngle;
        public Color shadowColor;

        public bool outlineEnabled;
        public int outlineDistance;
        public int outlineAngle;
        public Color outlineColor;
    }


    internal static LayerEffectData? GetLayerEffectData(this PSDLayer layer)
    {
        try
        {
            var infos = layer.bitmapLayer.PsdLayer.AdditionalInfo;
            EffectLayerInfo eli = null;
            foreach (var li in infos)
            {
                eli = li.ToEffectLayerInfo();
                if (eli != null) break;
            }

            if (eli != null)
            {
                var data = new LayerEffectData();
                var shadow = eli.GetShadowInfo();
                if (shadow != null)
                {
                    data.shadowEnabled = shadow.Enabled;
                    data.shadowDistance = shadow.Distance;
                    data.shadowAngle = shadow.Angle;
                    data.shadowColor = shadow.Color;
                    data.shadowColor.a = shadow.Opacity / (float)255;
                }

                return data;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        return default;
    }
}