using System;
using System.Collections;
using System.Collections.Generic;
using PhotoshopFile;
using UnityEngine;

public static class LayerInfoEx
{
    internal static TypeToolObjectSetting ToTypeToolObjectSetting(this LayerInfo info)
    {
        TypeToolObjectSetting.TryParse(info as RawLayerInfo, out var ttos);
        return ttos;
    }

    internal static EffectLayerInfo ToEffectLayerInfo(this LayerInfo info)
    {
        EffectLayerInfo.TryParse(info as RawLayerInfo, out var eli);
        return eli;
    }
}
