using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PhotoshopFile
{
    /// <summary>
    /// Color mode.
    /// </summary>
    public enum ColorMode
    {
        BITMAP = 0,
        GRAYSCALE = 1,
        INDEXED = 2,
        RGB = 3,
        CMYK = 4,
        MULTICHANNEL = 7,
        DUOTONE = 8,
        LAB = 9,
    }


    public enum ColorSpaceID
    {
        RGB = 0,
        HSB = 1,
        CMYK = 2,
        LAB = 7,
        GRAYSCALE = 8,
    }


    public static class BlendMode
    {
        public const string PASS_THROUGH = "pass";
        public const string NORMAL = "norm";
        public const string DISSOLVE = "diss";
        public const string DARKEN = "dark";
        public const string MULTIPLY = "mul ";
        public const string COLOR_BURN = "idiv";
        public const string LINEAR_BURN = "lbrn";
        public const string DARKER_COLOR = "dkCl";
        public const string LIGHTEN = "lite";
        public const string SCREEN = "scrn";
        public const string COLOR_DODGE = "div ";
        public const string LINEAR_DODGE = "lddg";
        public const string LIGHTER_COLOR = "lgCl";
        public const string OVERLAY = "over";
        public const string SOFT_LIGHT = "sLit";
        public const string HARD_LIGHT = "hLit";
        public const string VIVID_LIGHT = "vLit";
        public const string LINEAR_LIGHT = "lLit";
        public const string PIN_LIGHT = "pLit";
        public const string HARD_MIX = "hMix";
        public const string DIFFERENCE = "diff";
        public const string EXCLUSION = "smud";
        public const string SUBTRACT = "fsub";
        public const string DIVIDE = "fdiv";
        public const string HUE = "hue ";
        public const string SATURATION = "sat ";
        public const string COLOR = "colr";
        public const string LUMINOSITY = "lum ";
    }


    public enum GlobalLayerMaskKind
    {
        COLOR_SELECTED = 0,
        COLOR_PROTECTED = 1,
        PER_LAYER = 128,
    }


    // public enum Compression
    // {
    //     RAW = 0,
    //     RLE = 1,
    //     ZIP = 2,
    //     ZIP_WITH_PREDICTION = 3,
    // }


    // Tagged blocks keys
    public static class Tag
    {
        public const string ALPHA = "Alph"; // Undocumented.
        public const string ANIMATION_EFFECTS = "anFX";
        public const string ANNOTATIONS = "Anno";
        public const string ARTBOARD_DATA1 = "art";
        public const string ARTBOARD_DATA2 = "artd";
        public const string ARTBOARD_DATA3 = "abdd";
        public const string BLACK_AND_WHITE = "blwh";
        public const string BLEND_CLIPPING_ELEMENTS = "clbl";
        public const string BLEND_FILL_OPACITY = "iOpa"; // Undocumented.
        public const string BLEND_INTERIOR_ELEMENTS = "infx";
        public const string BRIGHTNESS_AND_CONTRAST = "brit";
        public const string CHANNEL_BLENDING_RESTRICTIONS_SETTING = "brst";
        public const string CHANNEL_MIXER = "mixr";
        public const string COLOR_BALANCE = "blnc";
        public const string COLOR_LOOKUP = "clrL";
        public const string COMPOSITOR_INFO = "cinf"; // Undocumented.
        public const string CONTENT_GENERATOR_EXTRA_DATA = "CgEd";
        public const string CURVES = "curv";
        public const string EFFECTS_LAYER = "lrFX";
        public const string EXPORT_SETTING1 = "extd"; // Undocumented.
        public const string EXPORT_SETTING2 = "extn"; // Undocumented.
        public const string EXPOSURE = "expA";
        public const string FILTER_EFFECTS1 = "FXid";
        public const string FILTER_EFFECTS2 = "FEid";
        public const string FILTER_EFFECTS3 = "FELS"; // Undocumented.
        public const string FILTER_MASK = "FMsk";
        public const string FOREIGN_EFFECT_ID = "ffxi";
        public const string FRAMED_GROUP = "frg"; // Undocumented, Frame tool in CC 2019?
        public const string GRADIENT_FILL_SETTING = "GdFl";
        public const string GRADIENT_MAP = "grdm";
        public const string HUE_SATURATION = "hue2";
        public const string HUE_SATURATION_V4 = "hue ";
        public const string INVERT = "nvrt";
        public const string KNOCKOUT_SETTING = "knko";
        public const string LAYER = "Layr";
        public const string LAYER_16 = "Lr16";
        public const string LAYER_32 = "Lr32";
        public const string LAYER_ID = "lyid";
        public const string LAYER_MASK_AS_GLOBAL_MASK = "lmgm";
        public const string LAYER_NAME_SOURCE_SETTING = "lnsr";
        public const string LAYER_VERSION = "lyvr";
        public const string LEVELS = "levl";
        public const string LINKED_LAYER1 = "lnkD";
        public const string LINKED_LAYER2 = "lnk2";
        public const string LINKED_LAYER3 = "lnk3";
        public const string LINKED_LAYER_EXTERNAL = "lnkE";
        public const string METADATA_SETTING = "shmd";
        public const string NESTED_SECTION_DIVIDER_SETTING = "lsdk";
        public const string OBJECT_BASED_EFFECTS_LAYER_INFO = "lfx2";
        public const string OBJECT_BASED_EFFECTS_LAYER_INFO_V0 = "lmfx"; // Undocumented.
        public const string OBJECT_BASED_EFFECTS_LAYER_INFO_V1 = "lfxs"; // Undocumented.
        public const string PATTERNS1 = "Patt";
        public const string PATTERNS2 = "Pat2";
        public const string PATTERNS3 = "Pat3";
        public const string PATTERN_DATA = "shpa";
        public const string PATTERN_FILL_SETTING = "PtFl";
        public const string PHOTO_FILTER = "phfl";
        public const string PIXEL_SOURCE_DATA1 = "PxSc";
        public const string PIXEL_SOURCE_DATA2 = "PxSD";
        public const string PLACED_LAYER1 = "plLd";
        public const string PLACED_LAYER2 = "PlLd";
        public const string POSTERIZE = "post";
        public const string PROTECTED_SETTING = "lspf";
        public const string REFERENCE_POINT = "fxrp";
        public const string SAVING_MERGED_TRANSPARENCY = "Mtrn";
        public const string SAVING_MERGED_TRANSPARENCY16 = "Mt16";
        public const string SAVING_MERGED_TRANSPARENCY32 = "Mt32";
        public const string SECTION_DIVIDER_SETTING = "lsct";
        public const string SELECTIVE_COLOR = "selc";
        public const string SHEET_COLOR_SETTING = "lclr";
        public const string SMART_OBJECT_LAYER_DATA1 = "SoLd";
        public const string SMART_OBJECT_LAYER_DATA2 = "SoLE";
        public const string SOLID_COLOR_SHEET_SETTING = "SoCo";
        public const string TEXT_ENGINE_DATA = "Txt2";
        public const string THRESHOLD = "thrs";
        public const string TRANSPARENCY_SHAPES_LAYER = "tsly";
        public const string TYPE_TOOL_INFO = "tySh";
        public const string TYPE_TOOL_OBJECT_SETTING = "TySh";
        public const string UNICODE_LAYER_NAME = "luni";
        public const string UNICODE_PATH_NAME = "pths";
        public const string USER_MASK = "LMsk";
        public const string USING_ALIGNED_RENDERING = "sn2P";
        public const string VECTOR_MASK_AS_GLOBAL_MASK = "vmgm";
        public const string VECTOR_MASK_SETTING1 = "vmsk";
        public const string VECTOR_MASK_SETTING2 = "vsms";
        public const string VECTOR_ORIGINATION_DATA = "vogk";
        public const string VECTOR_ORIGINATION_UNKNOWN = "vowv";
        public const string VECTOR_STROKE_DATA = "vstk";
        public const string VECTOR_STROKE_CONTENT_DATA = "vscg";
        public const string VIBRANCE = "vibA";

        // Unknown
        public const string PATT = "patt";
    }


    // Descriptor OSTypes and reference OSTypes.
    public static class OSType
    {
        public const string REFERENCE = "obj ";
        public const string DESCRIPTOR = "Objc";
        public const string LIST = "VlLs";
        public const string DOUBLE = "dou";
        public const string UNIT_FLOAT = "UntF";
        public const string UNIT_FLOATS = "UnFl"; // Undocumented
        public const string STRING = "TEXT";
        public const string ENUMERATED = "enum";
        public const string INTEGER = "long";
        public const string LARGE_INTEGER = "comp";
        public const string BOOLEAN = "bool";
        public const string GLOBAL_OBJECT = "GlbO";
        public const string CLASS1 = "type";
        public const string CLASS2 = "GlbC";
        public const string ALIAS = "alis";
        public const string RAW_DATA = "tdta";
        public const string OBJECT_ARRAY = "ObAr"; // Undocumented
        public const string PATH = "Pth "; // Undocumented

        // Reference OS types
        public const string PROPERTY = "prop";
        public const string CLASS3 = "Clss";
        public const string ENUMERATED_REFERENCE = "Enmr";
        public const string OFFSET = "rele";
        public const string IDENTIFIER = "Idnt";
        public const string INDEX = "indx";
        public const string NAME = "name";
    }


    // OS Type keys for Layer Effects.
    public static class EffectOSType
    {
        public const string COMMON_STATE = "cmnS";
        public const string DROP_SHADOW = "dsdw";
        public const string INNER_SHADOW = "isdw";
        public const string OUTER_GLOW = "oglw";
        public const string INNER_GLOW = "iglw";
        public const string BEVEL = "bevl";
        public const string SOLID_FILL = "sofi";
    }
}