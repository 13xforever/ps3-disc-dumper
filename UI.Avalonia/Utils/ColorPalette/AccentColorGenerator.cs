using System;
using Avalonia.Media;

namespace UI.Avalonia.Utils.ColorPalette;

public static class AccentColorGenerator
{
    // TODO: the contrast dword (buffer slot 8; complementary color) is not modelled; parked pending the high-contrast question.

    public static AccentColorInfo Get(Color color, AccentColorOptions? options = null)
    {
        Span<uint> result = stackalloc uint[8];
        GenerateSlots(color, result, options ?? AccentColorOptions.Default);
        return new(
            ToColor(result[(int)Slot.Accent]),
            ToColor(result[(int)Slot.Light1]),
            ToColor(result[(int)Slot.Light2]),
            ToColor(result[(int)Slot.Light3]),
            ToColor(result[(int)Slot.Dark1]),
            ToColor(result[(int)Slot.Dark2]),
            ToColor(result[(int)Slot.Dark3])
        );
    }

    internal static Color ClampToAccentColor(Color color, AccentColorOptions? options = null)
    {
        Span<uint> result = stackalloc uint[8];
        GenerateSlots(color, result, options ?? AccentColorOptions.Default);
        return ToColor(result[(int)Slot.Accent]);
    }

    private static void GenerateSlots(Color color, Span<uint> result, AccentColorOptions options)
    {
        color = AdjustToRgbGridIfNeeded(color);
        var key = color.ToRgb();
        foreach (var row in PaletteTable)
        {
            if (row[(int)Slot.Accent] != key)
                continue;
            
            row.AsSpan().CopyTo(result);
            return;
        }

        var hsl = TopGenHsl(color);
        if (TryLegacyGenerate(hsl, result, options))
            return;
        
        LabGenerate(color, result);
    }

    internal static Color AdjustToRgbGridIfNeeded(Color color)
    {
        byte r = color.R, g = color.G, b = color.B;

        // Hue in [300,360): red is the max channel, green the min, blue strictly between them.
        // b > g also means an excess of at least one unit; b == g (hue exactly 0/360) and b < g
        // (measured: ff8f8e) were never observed to move. A color of this exact shape was applied in
        // every one of the six hue sectors and only this one moved, so the restriction is measured
        // behavior across the whole wheel rather than an untested assumption.
        if (r < b || b <= g)
            return color;

        var grid = Math.Round((r - g) / 60d, MidpointRounding.ToEven);
        if (grid <= b - g)
            return color;

        b = (byte)(g + (int)grid);
        if (b > r)
            b = r;
        return Color.FromArgb(color.A, r, g, b);
    }
    
    private static (float h, float s, float l) TopGenHsl(Color color)
    {
        var r = color.R / 255f;
        var g = color.G / 255f;
        var b = color.B / 255f;
        var maxC = Math.Max(Math.Max(r, g), b);
        var minC = Math.Min(Math.Min(r, g), b);
        var l = (minC + maxC) * 0.5f;
        var delta = maxC - minC;
        var absDelta = Math.Abs(delta);
        if (absDelta < Epsilon)
            return (0f, 0f, l);

        var denominator = minC + maxC;
        if (0.5f < l)
            denominator = 2f - denominator;
        var s = absDelta / denominator;
        var hR = (maxC - r) * (1f/6f) / delta;
        var hG = (maxC - g) * (1f/6f) / delta;
        var hB = (maxC - b) * (1f/6f) / delta;

        float h;
        if (Math.Abs(r - maxC) < Epsilon)
            h = hB - hG;
        else if (Math.Abs(g - maxC) < Epsilon)
            h = hR + (1f/3f) - hB; // order matters for precision
        else
            h = hG + (2f/3f) - hR; // order matters for precision
        if (0f > h)
            h += 1f;
        if (h > 1f)
            h -= 1f;
        return (h, s, l);
    }

    private static void LabGenerate(Color color, Span<uint> result)
    {
        var (x, y, z) = RgbToXyz(color);
        var (l, a, cb) = XyzToLab(x, y, z);

        var mid = XyzToRgbBytes(LabToXyz(Math.Clamp(l, 49, 50), a, cb));
        var lo = XyzToRgbBytes(LabToXyz(0, a, cb));
        var hi = XyzToRgbBytes(LabToXyz(100, a, cb));

        var l3 = Blend(hi, mid, 0.82, 0.18000000000000005);
        var l2 = Blend(hi, mid, 0.58, 0.42000000000000004);
        var l1 = Blend(mid, hi, 0.84, 0.16);
        var d1 = Blend(lo, mid, 0.21999999999999997, 0.78);
        var d2 = Blend(mid, lo, 0.5, 0.5);
        var d3 = Blend(lo, mid, 0.82000000000000006, 0.18);

        // each pre-blend is post-processed against the accent: saturation floor
        result[0] = ToUInt32(ReAdjustBin(mid, l3));
        result[1] = ToUInt32(ReAdjustBin(mid, l2));
        result[2] = ToUInt32(ReAdjustBin(mid, l1));
        result[3] = ToUInt32(mid);
        result[4] = ToUInt32(ReAdjustBin(mid, d1));
        result[5] = ToUInt32(ReAdjustBin(mid, d2));
        result[6] = ToUInt32(ReAdjustBin(mid, d3));
    }

    private static (double x, double y, double z) RgbToXyz(Color color)
    {
        var r = SrgbToLinearByte(color.R);
        var g = SrgbToLinearByte(color.G);
        var b = SrgbToLinearByte(color.B);
        var x = (r*0.4124 + g*0.3576 + b*0.1805)*100;
        var y = (r*0.2126 + g*0.7152 + b*0.0722)*100;
        var z = (r*0.0193 + g*0.1192 + b*0.9505)*100;
        return (x, y, z);
    }

    private static (double l, double a, double b) XyzToLab(double x, double y, double z)
    {
        var fx = LabF(x / 95.047);
        var fy = LabF(y / 100);
        var fz = LabF(z / 108.883);
        var l = fy*116 - 16;
        var a = (fx - fy)*500;
        var b = (fy - fz)*200;
        return (l, a, b);
    }

    private static double SrgbToLinearByte(int v)
    {
        var x = v / 255d;
        if (x <= 0.04045)
            return x / 12.92;
        return Math.Pow((x + 0.055) / 1.055, 2.4);
    }

    private static double LabF(double t)
        => t <= 0.008856d
            ? t * 7.787 + (4/29d)
            : Math.Pow(t, 1/3d);

    private static (double x, double y, double z) LabToXyz(double l, double a, double b)
    {
        var fy = (l + 16) / 116;
        var fx = a/500 + fy;
        var fz = fy - b/200;
        var x = LabInv(fx) * 95.047;
        var y = LabInv(fy) * 100;
        var z = LabInv(fz) * 108.883;
        return (x, y, z);
    }

    private static double LabInv(double f)
        => f <= 6/29d
            ? (f - 4/29d) * (108/841d)
            : Math.Pow(f, 3);

    private static (byte r, byte g, byte b) XyzToRgbBytes((double x, double y, double z) xyz)
    {
        var z100 = xyz.z/100;
        var x100 = xyz.x/100;
        var y100 = xyz.y/100;
        var r = LinearToSrgb(x100*3.2406 - y100*1.5372 - z100*0.4986);
        var g = LinearToSrgb(z100*0.0415 + y100*1.8758 - x100*0.9689);
        var b = LinearToSrgb(x100*0.0557 - y100*0.204 + z100*1.057);
        return (LabByte(r), LabByte(g), LabByte(b));
    }

    private static double LinearToSrgb(double v)
    {
        var g = v <= 0.0031308
            ? v * 12.92
            : Math.Pow(v, (5/12d)) * 1.055 - 0.055;
        var c = g > 1 ? 1 : g;
        var r = c < 0 ? 0 : c;
        return r;
    }

    private static byte LabByte(double c)
        => (byte)Math.Round(c * 255, MidpointRounding.ToEven);

    private static (byte, byte, byte) Blend(
        (byte r, byte g, byte b) cA,
        (byte r, byte g, byte b) cB,
        double wA,
        double wB)
        => (
            MixByte(cA.r, cB.r, wA, wB),
            MixByte(cA.g, cB.g, wA, wB),
            MixByte(cA.b, cB.b, wA, wB)
        );

    private static byte MixByte(byte cA, byte cB, double wA, double wB)
        => (byte)((int)(cA * wA + cB * wB + 0.5) & 0xFF);
    
    private static (byte r, byte g, byte b) ReAdjustBin((byte r, byte g, byte b) accent, (byte r, byte g, byte b) mixed)
    {
        var (_, accentS, _) = RgbToHslBin(accent);
        var (mixH, mixS, mixL) = RgbToHslBin(mixed);
        if (mixS < accentS)
            mixS = accentS;
        return HslToRgbBytesBin(mixH, mixS, mixL);
    }

    private static (double h, double s, double l) RgbToHslBin((byte r, byte g, byte b) color)
    {
        var r = color.r / 255d;
        var g = color.g / 255d;
        var b = color.b / 255d;
        var maxC = Math.Max(Math.Max(r, g), b);
        var minC = Math.Min(Math.Min(r, g), b);
        var l = (minC + maxC) * 0.5;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (maxC == minC)
            return (0, 0, l);

        var d = maxC - minC;
        var denominator = minC + maxC;
        if (l > 0.5)
            denominator = 2 - maxC - minC;
        var s = d / denominator;

        double h;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (maxC == r)
        {
            if (g < b)
                h = ((g - b) / d + 6) / 6;
            else
                h = ((g - b) / d + 0) / 6;
        }
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        else if (maxC == g)
            h = ((b - r) / d + 2) / 6;
        else
            h = ((r - g) / d + 4) / 6;
        return (h, s, l);
    }

    private static (byte r, byte g, byte b) HslToRgbBytesBin(double h, double s, double l)
    {
        h -= Math.Truncate(h);
        if (h < 0)
            h += 1;
        h = Math.Clamp(h, 0, 1);
        s = Math.Clamp(s, 0, 1);
        l = Math.Clamp(l, 0, 1);
        var q = 0.5 < l
            ? (s + l) - s * l
            : (s + 1) * l;
        if (q <= 0)
            return (0, 0, 0);
        
        var p = l*2 - q;
        var h6 = h * 6;
        var i = (int)h6;
        var t = (q - p) * (h6 - i);
        var m = q - t;
        var n = p + t;
        var (r, g, b) = i switch
        {
            0 => (q, n, p),
            1 => (m, q, p),
            2 => (p, q, n),
            3 => (p, m, q),
            4 => (n, p, q),
            5 => (q, p, m),
            _ => (q, n, p)
        };
        return (LabByte(r), LabByte(g), LabByte(b));
    }

    private static uint ToUInt32(byte r, byte g, byte b) => (uint)((b << 16) | (g << 8) | r);
    private static uint ToUInt32((byte r, byte g, byte b) rgb) => ToUInt32(rgb.r, rgb.g, rgb.b);

    private static bool TryLegacyGenerate((float h, float s, float l) color, Span<uint> result, AccentColorOptions options)
    {
        if (options is { AutoColorAccent: true, UseNewAutoColorAccentAlgorithm: true })
            if (color.s < options.MinSaturation / 1000f)
            {
                AutoColorDefaultPalette.AsSpan().CopyTo(result);
                return true;
            }
            else
                return false;

        float lAdj;
        if (color.l < LowLightnessClamp)
            lAdj = LowLightnessClamp;
        else
        {
            var d = 1d - color.l;
            if (d < 0.25)
                lAdj = 0.75f;
            else if (color.s < 0.149999991f)
                lAdj = color.l;
            else
                return false;
        }

        var x = Math.Abs(0.5f - lAdj) * 4;
        Span<float> offsets = stackalloc float[7];
        for (var i = 0; i < 7; i++)
            offsets[i] = x * SweepCoefficients[i];
        var first = lAdj + offsets[0];
        var clamp = first > 1f
            ? first - 1f
            : Math.Min(0f, lAdj + offsets[6]);
        for (var i = 0; i < 7; i++)
        {
            var val = Math.Clamp(lAdj + offsets[i] - clamp, 0, 1);
            var (r, g, b) = LegacyWrapper(color.h, color.s, val);
            result[i] = ToUInt32(r, g, b);
        }
        return true;
    }

    private static (byte r, byte g, byte b) LegacyWrapper(float h, float s, float l)
    {
        var (r, g, b) = HslHelper(h, s, l);
        return (ToByte(r), ToByte(g), ToByte(b));
    }

    private static (float r, float g, float b) HslHelper(float h, float s, float l)
    {
        if (Math.Abs(s) < Epsilon)
            return (l, l, l);
        
        var q = l > 0.5f
            ? (s + l) - s * l
            : (s + 1f) * l;
        var p = l*2 - q;
        var r = Channel(p, q, h + 1/3f);
        var g = Channel(p, q, h);
        var b = Channel(p, q, h - 1/3f);
        return (r, g, b);
    }

    private static float Channel(float p, float q, float t)
    {
        if (t < 0f)
            t += 1f;
        else if (t > 1f)
            t += -1f;
        if (t < 1/6f)
            return p + (q - p) * t * 6f;
        
        if (t < 0.5f)
            return q;

        if (t < 2/3f)
            return p + (q - p) * (2/3f - t) * 6f;
        
        return p;
    }
 
    private static byte ToByte(float x)
        => (byte)((int)(x * 255f) & 0xFF);

    private static Color ToColor(uint dword)
        => Color.FromArgb(
            0xFF,
            (byte)(dword & 0xFF),
            (byte)((dword >> 8) & 0xFF),
            (byte)((dword >> 16) & 0xFF)
        );

    private static uint ToRgb(this Color color)
        => (uint)(color.R | (color.G << 8) | (color.B << 16));

    private enum Slot
    {
        Light3 = 0,
        Light2 = 1,
        Light1 = 2,
        Accent = 3,
        Dark1 = 4,
        Dark2 = 5,
        Dark3 = 6,
    }

    private const float Epsilon = 2.38418579e-7f;
    private const float LowLightnessClamp = 0.25f;

    private static readonly float[] SweepCoefficients =
    [
        0.17f, 0.1f, 0.05f, 0f, -0.05f, -0.099999994f, -0.17f
    ];

    private static uint[] AutoColorDefaultPalette => PaletteTable[2];

    private static readonly uint[][] PaletteTable =
    [
        [ 0x0045E8FFu, 0x002AD5FFu, 0x000DC2FFu, 0x0000B9FFu, 0x00009DE1u, 0x00005D9Bu, 0x0000215Cu, 0x0094B200u ],
        [ 0x00B7B4FBu, 0x00918AF3u, 0x00685CEBu, 0x005648E7u, 0x003829E3u, 0x001E16A6u, 0x000F0D65u, 0x00008CFFu ],
        [ 0x00FFEB99u, 0x00FFC24Cu, 0x00F89100u, 0x00D47800u, 0x00C06700u, 0x00923E00u, 0x00681A00u, 0x000C63F7u ],
        [ 0x00FFF77Au, 0x00FFE530u, 0x00DFB800u, 0x00BC9900u, 0x00A68500u, 0x00735500u, 0x00452A00u, 0x008C00E3u ],
        [ 0x00E4E5E7u, 0x00B8B9BDu, 0x0088898Eu, 0x0074757Au, 0x00636369u, 0x003A3B40u, 0x0015151Bu, 0x005E00EAu ],
        [ 0x00E6E6E6u, 0x00BABBBBu, 0x008B8B8Bu, 0x00767676u, 0x00646464u, 0x003B3B3Bu, 0x00151515u, 0x003834D1u ],
        [ 0x0055D1FFu, 0x0034B6FFu, 0x001099FFu, 0x00008CFFu, 0x000077E3u, 0x000046A1u, 0x00001965u, 0x00B16300u ],
        [ 0x008B9DFBu, 0x006267F4u, 0x003327EFu, 0x002311E8u, 0x001E0ED2u, 0x0012099Eu, 0x0006036Fu, 0x007E7969u ],
        [ 0x00FFEB9Cu, 0x00FFBD40u, 0x00DC7F00u, 0x00B16300u, 0x00A15500u, 0x007C3300u, 0x005A1400u, 0x006ACC00u ],
        [ 0x00EDE8A7u, 0x00D5C46Fu, 0x00B79835u, 0x009A7D2Du, 0x00886C26u, 0x005F4517u, 0x003A2108u, 0x006ACC00u ],
        [ 0x00DEE1E2u, 0x00AAACAEu, 0x00707376u, 0x00585A5Du, 0x004B4D50u, 0x002C2D30u, 0x00101014u, 0x004343FFu ],
        [ 0x00DCDEDFu, 0x00A1A5A6u, 0x00626568u, 0x00484A4Cu, 0x003D3F41u, 0x00242527u, 0x000D0D10u, 0x00107C10u ],
        [ 0x0068BDFEu, 0x00449AFBu, 0x001D74F8u, 0x000C63F7u, 0x000753E0u, 0x000531A1u, 0x00021269u, 0x00BC9900u ],
        [ 0x00CA95FFu, 0x009F52FFu, 0x006F0AFFu, 0x005E00EAu, 0x005000D3u, 0x002F009Eu, 0x0011006Du, 0x0074757Au ],
        [ 0x00F8E3E8u, 0x00ECC1C5u, 0x00DE9C9Fu, 0x00D88C8Eu, 0x00CD6B6Du, 0x009B3436u, 0x0056191Au, 0x00B86487u ],
        [ 0x00FFFC69u, 0x00FFF729u, 0x00E1D500u, 0x00C3B700u, 0x00AA9F00u, 0x00706700u, 0x003B3400u, 0x0059544Au ],
        [ 0x00EAE6D9u, 0x00C5BBADu, 0x009C8A7Cu, 0x008A7668u, 0x00796558u, 0x00513C34u, 0x002D1713u, 0x0094B200u ],
        [ 0x00E8E7D8u, 0x00BFBCADu, 0x00928E7Cu, 0x007E7969u, 0x006D6759u, 0x00453F35u, 0x00201913u, 0x00606848u ],
        [ 0x007CC0F5u, 0x004693F0u, 0x001260E4u, 0x001050CAu, 0x000E44B6u, 0x00082887u, 0x00030E5Cu, 0x00545E52u ],
        [ 0x00D197FFu, 0x009D45FFu, 0x006500EAu, 0x005200C3u, 0x004600B1u, 0x00290086u, 0x000F005Fu, 0x00737C56u ],
        [ 0x00F8D8E4u, 0x00EBADB5u, 0x00DC7D81u, 0x00D6696Bu, 0x00CE4D4Fu, 0x009E2B2Cu, 0x006A1313u, 0x006ACC00u ],
        [ 0x00FCFB7Cu, 0x00FAF621u, 0x00B2AD04u, 0x00878303u, 0x00767203u, 0x004E4B02u, 0x002A2801u, 0x005069EFu ],
        [ 0x00E4E2D7u, 0x00B7AEA0u, 0x00867465u, 0x006B5C51u, 0x005E4E45u, 0x00402F29u, 0x0024120Fu, 0x005069EFu ],
        [ 0x00E1E0D6u, 0x00AEAB9Eu, 0x00756F61u, 0x0059544Au, 0x004D483Fu, 0x00322B25u, 0x0018110Du, 0x00C3B700u ],
        [ 0x006CB1FEu, 0x00347EFEu, 0x000148F4u, 0x00013BDAu, 0x000132C5u, 0x00011E93u, 0x00000B66u, 0x00008CFFu ],
        [ 0x00EE94FFu, 0x00CB4FFFu, 0x00A404FFu, 0x008C00E3u, 0x007B00CDu, 0x00530099u, 0x002E0069u, 0x00BC9900u ],
        [ 0x00F2D4E9u, 0x00DCA8C3u, 0x00C37999u, 0x00B86487u, 0x00AC4F74u, 0x0079324Bu, 0x00511224u, 0x00D88C8Eu ],
        [ 0x00EEFF67u, 0x00E0FF21u, 0x00B1D300u, 0x0094B200u, 0x007E9B00u, 0x004A6600u, 0x001B3600u, 0x008A7668u ],
        [ 0x00E4E7C5u, 0x00B8BE9Au, 0x008A9466u, 0x00737C56u, 0x00626B49u, 0x003A442Bu, 0x0015200Fu, 0x005200C3u ],
        [ 0x00D2E7D3u, 0x00A7BEA8u, 0x00779177u, 0x00647C64u, 0x00556B55u, 0x00324332u, 0x00121E12u, 0x003834D1u ],
        [ 0x00A3C2FCu, 0x0083A0F7u, 0x005F79F1u, 0x005069EFu, 0x002D49ECu, 0x001022B1u, 0x00090E67u, 0x00A94D74u ],
        [ 0x00EE97FFu, 0x00C843FFu, 0x009400E6u, 0x007700BFu, 0x006900ADu, 0x00480083u, 0x0029005Cu, 0x00981788u ],
        [ 0x00F0CEE6u, 0x00D49CBAu, 0x00B86389u, 0x00A94D74u, 0x00994165u, 0x00752740u, 0x00540E1Fu, 0x005069EFu ],
        [ 0x00F7FE77u, 0x00E9FD1Au, 0x009CB001u, 0x00748501u, 0x00637401u, 0x003A4E01u, 0x00152A00u, 0x003834D1u ],
        [ 0x00E1E4C7u, 0x00AFB594u, 0x007A835Bu, 0x00606848u, 0x00525A3Du, 0x00303924u, 0x00111B0Du, 0x00058249u ],
        [ 0x00D9E2D7u, 0x00A5AFA3u, 0x006B7869u, 0x00545E52u, 0x00475146u, 0x002A3129u, 0x000F150Fu, 0x001050CAu ],
        [ 0x00A5B1F7u, 0x007A80E8u, 0x004C4BD8u, 0x003834D1u, 0x002E2BBEu, 0x001C1A8Du, 0x000A0961u, 0x00647C64u ],
        [ 0x00F1B2F4u, 0x00D983E1u, 0x00BF4FCBu, 0x00B339C2u, 0x00A030AEu, 0x00751D7Fu, 0x004D0A54u, 0x009A7D2Du ],
        [ 0x00F4BBF1u, 0x00E18DD8u, 0x00CB5BBDu, 0x00C246B1u, 0x00B03A9Eu, 0x0082236Fu, 0x005A0D46u, 0x00008CFFu ],
        [ 0x00A5FF5Fu, 0x008EFF26u, 0x0075E700u, 0x006ACC00u, 0x005AB200u, 0x00357600u, 0x00133F00u, 0x008C00E3u ],
        [ 0x006CF9C1u, 0x0018F699u, 0x0007A961u, 0x00058249u, 0x0004723Eu, 0x00034B25u, 0x0001280Du, 0x00606848u ],
        [ 0x00B0E5E9u, 0x0086B9C2u, 0x00528D9Du, 0x00457584u, 0x003B6374u, 0x00233B4Eu, 0x000C152Bu, 0x005069EFu ],
        [ 0x009BABFFu, 0x007982FFu, 0x005356FFu, 0x004343FFu, 0x001F1FFFu, 0x000000C9u, 0x0000007Cu, 0x00C3B700u ],
        [ 0x00FA97FFu, 0x00EE35FFu, 0x00B300C8u, 0x0089009Au, 0x007C008Cu, 0x005E006Bu, 0x0042004Cu, 0x006ACC00u ],
        [ 0x00F2ACEFu, 0x00E65BD9u, 0x00BE1DABu, 0x00981788u, 0x008B147Bu, 0x006D0C5Bu, 0x0051043Fu, 0x007700BFu ],
        [ 0x00A5F189u, 0x0072E83Au, 0x004CAE14u, 0x003E8910u, 0x0035780Eu, 0x001F5208u, 0x000B2E03u, 0x00C3B700u ],
        [ 0x0081EF95u, 0x0032E545u, 0x0015A119u, 0x00107C10u, 0x000E6D0Eu, 0x00084B08u, 0x00032B03u, 0x00484A4Cu ],
        [ 0x00CEE5E8u, 0x00A3B8BFu, 0x00718994u, 0x005F737Eu, 0x0051626Du, 0x00303A47u, 0x00111524u, 0x00C3B700u ]
    ];
}