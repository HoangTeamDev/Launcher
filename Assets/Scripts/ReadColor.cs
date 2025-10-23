using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class ReadColor
{
    // 🌈 Màu cơ bản
    public static readonly string Red = "#FF0000";
    public static readonly string Green = "#00FF00";
    public static readonly string Blue = "#0000FF";
    public static readonly string Yellow = "#FFFF00";
    public static readonly string Cyan = "#00FFFF";
    public static readonly string Magenta = "#FF00FF";
    public static readonly string White = "#FFFFFF";
    public static readonly string Black = "#000000";

    // 🍊 Màu mở rộng
    public static readonly string Orange = "#FFA500";
    public static readonly string Purple = "#800080";
    public static readonly string Pink = "#FFC0CB";
    public static readonly string Brown = "#8B4513";
    public static readonly string Lime = "#32CD32";
    public static readonly string Violet = "#EE82EE";
    public static readonly string BrownDark = "#4E2A0C";
    public static readonly string WhiteFade = "#A6ADB4";

    // 🎨 Màu pastel
    public static readonly string PastelBlue = "#AEC6CF";
    public static readonly string PastelGreen = "#77DD77";
    public static readonly string PastelPurple = "#B39EB5";
    public static readonly string PastelPink = "#FFD1DC";
    public static readonly string PastelYellow = "#FDFD96";
    public static readonly string PastelOrange = "#FFB347";

    // 🌅 Màu trung tính
    public static readonly string Gray = "#808080";
    public static readonly string LightGray = "#D3D3D3";
    public static readonly string DarkGray = "#A9A9A9";
    public static readonly string Silver = "#C0C0C0";
    public static readonly string Gold = "#FFD700";
    public static readonly string Bronze = "#CD7F32";

    // 🔥 Màu đặc biệt
    public static readonly string Crimson = "#DC143C";   // Đỏ thẫm
    public static readonly string MidnightBlue = "#191970"; // Xanh đậm
    public static readonly string Navy = "#000080";      // Xanh hải quân
    public static readonly string Teal = "#008080";      // Xanh lam lục
    public static readonly string Indigo = "#4B0082";    // Chàm
    public static readonly string Olive = "#808000";     // Xanh ô liu
    public static readonly string Maroon = "#800000";    // Đỏ nâu
    public static readonly string Chocolate = "#D2691E"; // Nâu sô cô la
    public static readonly string Turquoise = "#40E0D0"; // Xanh ngọc
    public static readonly string Coral = "#FF7F50";     // Cam hồng
    public static readonly string Tomato = "#FF6347";    // Đỏ cà chua 
    public static Color FromHex(string hex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
            return color;
        return Color.white;
    }
}

