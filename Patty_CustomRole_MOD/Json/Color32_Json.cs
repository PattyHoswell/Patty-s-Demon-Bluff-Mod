using UnityEngine;

namespace Patty_CustomRole_MOD.Json
{
    public class Color32_Json
    {
        public byte R { get; set; } = 255;
        public byte G { get; set; } = 255;
        public byte B { get; set; } = 255;
        public byte A { get; set; } = 255;

        public Color32_Json() { }
        public Color32_Json(Color32 color32)
        {
            R = color32.r;
            G = color32.g;
            B = color32.b;
            A = color32.a;
        }

        public static implicit operator Color(Color32_Json data)
        {
            return new Color32(data.R, data.G, data.B, data.A);
        }

        public static implicit operator Color32_Json(Color color)
        {
            return new Color32_Json(color);
        }

        public static implicit operator Color32(Color32_Json data)
        {
            return new Color32(data.R, data.G, data.B, data.A);
        }

        public static implicit operator Color32_Json(Color32 color32)
        {
            return new Color32_Json(color32);
        }
    }

}
