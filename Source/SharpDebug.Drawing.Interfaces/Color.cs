namespace SharpDebug.Drawing.Interfaces
{
    /// <summary>
    /// Simple color structure that defines a RGBA color.
    /// </summary>
    public struct Color
    {
        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #F0F8FF.
        /// </summary>
        public static readonly Color AliceBlue = new Color(240 / 255.0, 248 / 255.0, 1);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FAEBD7.
        /// </summary>
        public static readonly Color AntiqueWhite = new Color(250 / 255.0, 235 / 255.0, 215 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #00FFFF.
        /// </summary>
        public static readonly Color Aqua = new Color(0, 1, 1);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #7FFFD4.
        /// </summary>
        public static readonly Color Aquamarine = new Color(127 / 255.0, 1, 212 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #F0FFFF.
        /// </summary>
        public static readonly Color Azure = new Color(240 / 255.0, 1, 1);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #F5F5DC.
        /// </summary>
        public static readonly Color Beige = new Color(245 / 255.0, 245 / 255.0, 220 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFE4C4.
        /// </summary>
        public static readonly Color Bisque = new Color(1, 228 / 255.0, 196 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #000000.
        /// </summary>
        public static readonly Color Black = new Color(0, 0, 0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFEBCD.
        /// </summary>
        public static readonly Color BlanchedAlmond = new Color(1, 235 / 255.0, 205 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #0000FF.
        /// </summary>
        public static readonly Color Blue = new Color(0, 0, 1);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #8A2BE2.
        /// </summary>
        public static readonly Color BlueViolet = new Color(138 / 255.0, 43 / 255.0, 226 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #A52A2A.
        /// </summary>
        public static readonly Color Brown = new Color(165 / 255.0, 42 / 255.0, 42 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #DEB887.
        /// </summary>
        public static readonly Color BurlyWood = new Color(222 / 255.0, 184 / 255.0, 135 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #5F9EA0.
        /// </summary>
        public static readonly Color CadetBlue = new Color(95 / 255.0, 158 / 255.0, 160 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #7FFF00.
        /// </summary>
        public static readonly Color Chartreuse = new Color(127 / 255.0, 1, 0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #D2691E.
        /// </summary>
        public static readonly Color Chocolate = new Color(210 / 255.0, 105 / 255.0, 30 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FF7F50.
        /// </summary>
        public static readonly Color Coral = new Color(1, 127 / 255.0, 80 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #6495ED.
        /// </summary>
        public static readonly Color CornflowerBlue = new Color(100 / 255.0, 149 / 255.0, 237 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFF8DC.
        /// </summary>
        public static readonly Color Cornsilk = new Color(1, 248 / 255.0, 220 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #DC143C.
        /// </summary>
        public static readonly Color Crimson = new Color(220 / 255.0, 20 / 255.0, 60 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #00FFFF.
        /// </summary>
        public static readonly Color Cyan = new Color(0, 1, 1);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #00008B.
        /// </summary>
        public static readonly Color DarkBlue = new Color(0, 0, 139 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #008B8B.
        /// </summary>
        public static readonly Color DarkCyan = new Color(0, 139 / 255.0, 139 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #B8860B.
        /// </summary>
        public static readonly Color DarkGoldenrod = new Color(184 / 255.0, 134 / 255.0, 11 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #A9A9A9.
        /// </summary>
        public static readonly Color DarkGray = new Color(169 / 255.0, 169 / 255.0, 169 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #006400.
        /// </summary>
        public static readonly Color DarkGreen = new Color(0, 100 / 255.0, 0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #BDB76B.
        /// </summary>
        public static readonly Color DarkKhaki = new Color(189 / 255.0, 183 / 255.0, 107 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #8B008B.
        /// </summary>
        public static readonly Color DarkMagenta = new Color(139 / 255.0, 0, 139 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #556B2F.
        /// </summary>
        public static readonly Color DarkOliveGreen = new Color(85 / 255.0, 107 / 255.0, 47 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FF8C00.
        /// </summary>
        public static readonly Color DarkOrange = new Color(1, 140 / 255.0, 0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #9932CC.
        /// </summary>
        public static readonly Color DarkOrchid = new Color(153 / 255.0, 50 / 255.0, 204 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #8B0000.
        /// </summary>
        public static readonly Color DarkRed = new Color(139 / 255.0, 0, 0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #E9967A.
        /// </summary>
        public static readonly Color DarkSalmon = new Color(233 / 255.0, 150 / 255.0, 122 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #8FBC8F.
        /// </summary>
        public static readonly Color DarkSeaGreen = new Color(143 / 255.0, 188 / 255.0, 143 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #483D8B.
        /// </summary>
        public static readonly Color DarkSlateBlue = new Color(72 / 255.0, 61 / 255.0, 139 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #2F4F4F.
        /// </summary>
        public static readonly Color DarkSlateGray = new Color(47 / 255.0, 79 / 255.0, 79 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #00CED1.
        /// </summary>
        public static readonly Color DarkTurquoise = new Color(0, 206 / 255.0, 209 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #9400D3.
        /// </summary>
        public static readonly Color DarkViolet = new Color(148 / 255.0, 0, 211 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FF1493.
        /// </summary>
        public static readonly Color DeepPink = new Color(1, 20 / 255.0, 147 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #00BFFF.
        /// </summary>
        public static readonly Color DeepSkyBlue = new Color(0, 191 / 255.0, 1);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #696969.
        /// </summary>
        public static readonly Color DimGray = new Color(105 / 255.0, 105 / 255.0, 105 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #1E90FF.
        /// </summary>
        public static readonly Color DodgerBlue = new Color(30 / 255.0, 144 / 255.0, 1);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #B22222.
        /// </summary>
        public static readonly Color Firebrick = new Color(178 / 255.0, 34 / 255.0, 34 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFFAF0.
        /// </summary>
        public static readonly Color FloralWhite = new Color(1, 250 / 255.0, 240 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #228B22.
        /// </summary>
        public static readonly Color ForestGreen = new Color(34 / 255.0, 139 / 255.0, 34 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FF00FF.
        /// </summary>
        public static readonly Color Fuchsia = new Color(1, 0, 1);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #DCDCDC.
        /// </summary>
        public static readonly Color Gainsboro = new Color(220 / 255.0, 220 / 255.0, 220 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #F8F8FF.
        /// </summary>
        public static readonly Color GhostWhite = new Color(248 / 255.0, 248 / 255.0, 1);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFD700.
        /// </summary>
        public static readonly Color Gold = new Color(1, 215 / 255.0, 0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #DAA520.
        /// </summary>
        public static readonly Color Goldenrod = new Color(218 / 255.0, 165 / 255.0, 32 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #808080.
        /// </summary>
        public static readonly Color Gray = new Color(128 / 255.0, 128 / 255.0, 128 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #008000.
        /// </summary>
        public static readonly Color Green = new Color(0, 128 / 255.0, 0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #ADFF2F.
        /// </summary>
        public static readonly Color GreenYellow = new Color(173 / 255.0, 1, 47 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #F0FFF0.
        /// </summary>
        public static readonly Color Honeydew = new Color(240 / 255.0, 1, 240 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FF69B4.
        /// </summary>
        public static readonly Color HotPink = new Color(1, 105 / 255.0, 180 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #CD5C5C.
        /// </summary>
        public static readonly Color IndianRed = new Color(205 / 255.0, 92 / 255.0, 92 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #4B0082.
        /// </summary>
        public static readonly Color Indigo = new Color(75 / 255.0, 0, 130 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFFFF0.
        /// </summary>
        public static readonly Color Ivory = new Color(1, 1, 240 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #F0E68C.
        /// </summary>
        public static readonly Color Khaki = new Color(240 / 255.0, 230 / 255.0, 140 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #E6E6FA.
        /// </summary>
        public static readonly Color Lavender = new Color(230 / 255.0, 230 / 255.0, 250 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFF0F5.
        /// </summary>
        public static readonly Color LavenderBlush = new Color(1, 240 / 255.0, 245 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #7CFC00.
        /// </summary>
        public static readonly Color LawnGreen = new Color(124 / 255.0, 252 / 255.0, 0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFFACD.
        /// </summary>
        public static readonly Color LemonChiffon = new Color(1, 250 / 255.0, 205 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #ADD8E6.
        /// </summary>
        public static readonly Color LightBlue = new Color(173 / 255.0, 216 / 255.0, 230 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #F08080.
        /// </summary>
        public static readonly Color LightCoral = new Color(240 / 255.0, 128 / 255.0, 128 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #E0FFFF.
        /// </summary>
        public static readonly Color LightCyan = new Color(224 / 255.0, 1, 1);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FAFAD2.
        /// </summary>
        public static readonly Color LightGoldenrodYellow = new Color(250 / 255.0, 250 / 255.0, 210 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #D3D3D3.
        /// </summary>
        public static readonly Color LightGray = new Color(211 / 255.0, 211 / 255.0, 211 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #90EE90.
        /// </summary>
        public static readonly Color LightGreen = new Color(144 / 255.0, 238 / 255.0, 144 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFB6C1.
        /// </summary>
        public static readonly Color LightPink = new Color(1, 182 / 255.0, 193 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFA07A.
        /// </summary>
        public static readonly Color LightSalmon = new Color(1, 160 / 255.0, 122 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #20B2AA.
        /// </summary>
        public static readonly Color LightSeaGreen = new Color(32 / 255.0, 178 / 255.0, 170 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #87CEFA.
        /// </summary>
        public static readonly Color LightSkyBlue = new Color(135 / 255.0, 206 / 255.0, 250 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #778899.
        /// </summary>
        public static readonly Color LightSlateGray = new Color(119 / 255.0, 136 / 255.0, 153 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #B0C4DE.
        /// </summary>
        public static readonly Color LightSteelBlue = new Color(176 / 255.0, 196 / 255.0, 222 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFFFE0.
        /// </summary>
        public static readonly Color LightYellow = new Color(1, 1, 224 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #00FF00.
        /// </summary>
        public static readonly Color Lime = new Color(0, 1, 0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #32CD32.
        /// </summary>
        public static readonly Color LimeGreen = new Color(50 / 255.0, 205 / 255.0, 50 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FAF0E6.
        /// </summary>
        public static readonly Color Linen = new Color(250 / 255.0, 240 / 255.0, 230 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FF00FF.
        /// </summary>
        public static readonly Color Magenta = new Color(1, 0, 1);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #800000.
        /// </summary>
        public static readonly Color Maroon = new Color(128 / 255.0, 0, 0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #66CDAA.
        /// </summary>
        public static readonly Color MediumAquamarine = new Color(102 / 255.0, 205 / 255.0, 170 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #0000CD.
        /// </summary>
        public static readonly Color MediumBlue = new Color(0, 0, 205 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #BA55D3.
        /// </summary>
        public static readonly Color MediumOrchid = new Color(186 / 255.0, 85 / 255.0, 211 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #9370DB.
        /// </summary>
        public static readonly Color MediumPurple = new Color(147 / 255.0, 112 / 255.0, 219 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #3CB371.
        /// </summary>
        public static readonly Color MediumSeaGreen = new Color(60 / 255.0, 179 / 255.0, 113 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #7B68EE.
        /// </summary>
        public static readonly Color MediumSlateBlue = new Color(123 / 255.0, 104 / 255.0, 238 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #00FA9A.
        /// </summary>
        public static readonly Color MediumSpringGreen = new Color(0, 250 / 255.0, 154 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #48D1CC.
        /// </summary>
        public static readonly Color MediumTurquoise = new Color(72 / 255.0, 209 / 255.0, 204 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #C71585.
        /// </summary>
        public static readonly Color MediumVioletRed = new Color(199 / 255.0, 21 / 255.0, 133 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #191970.
        /// </summary>
        public static readonly Color MidnightBlue = new Color(25 / 255.0, 25 / 255.0, 112 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #F5FFFA.
        /// </summary>
        public static readonly Color MintCream = new Color(245 / 255.0, 1, 250 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFE4E1.
        /// </summary>
        public static readonly Color MistyRose = new Color(1, 228 / 255.0, 225 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFE4B5.
        /// </summary>
        public static readonly Color Moccasin = new Color(1, 228 / 255.0, 181 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFDEAD.
        /// </summary>
        public static readonly Color NavajoWhite = new Color(1, 222 / 255.0, 173 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #000080.
        /// </summary>
        public static readonly Color Navy = new Color(0, 0, 128 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FDF5E6.
        /// </summary>
        public static readonly Color OldLace = new Color(253 / 255.0, 245 / 255.0, 230 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #808000.
        /// </summary>
        public static readonly Color Olive = new Color(128 / 255.0, 128 / 255.0, 0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #6B8E23.
        /// </summary>
        public static readonly Color OliveDrab = new Color(107 / 255.0, 142 / 255.0, 35 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFA500.
        /// </summary>
        public static readonly Color Orange = new Color(1, 165 / 255.0, 0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FF4500.
        /// </summary>
        public static readonly Color OrangeRed = new Color(1, 69 / 255.0, 0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #DA70D6.
        /// </summary>
        public static readonly Color Orchid = new Color(218 / 255.0, 112 / 255.0, 214 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #EEE8AA.
        /// </summary>
        public static readonly Color PaleGoldenrod = new Color(238 / 255.0, 232 / 255.0, 170 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #98FB98.
        /// </summary>
        public static readonly Color PaleGreen = new Color(152 / 255.0, 251 / 255.0, 152 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #AFEEEE.
        /// </summary>
        public static readonly Color PaleTurquoise = new Color(175 / 255.0, 238 / 255.0, 238 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #DB7093.
        /// </summary>
        public static readonly Color PaleVioletRed = new Color(219 / 255.0, 112 / 255.0, 147 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFEFD5.
        /// </summary>
        public static readonly Color PapayaWhip = new Color(1, 239 / 255.0, 213 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFDAB9.
        /// </summary>
        public static readonly Color PeachPuff = new Color(1, 218 / 255.0, 185 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #CD853F.
        /// </summary>
        public static readonly Color Peru = new Color(205 / 255.0, 133 / 255.0, 63 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFC0CB.
        /// </summary>
        public static readonly Color Pink = new Color(1, 192 / 255.0, 203 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #DDA0DD.
        /// </summary>
        public static readonly Color Plum = new Color(221 / 255.0, 160 / 255.0, 221 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #B0E0E6.
        /// </summary>
        public static readonly Color PowderBlue = new Color(176 / 255.0, 224 / 255.0, 230 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #800080.
        /// </summary>
        public static readonly Color Purple = new Color(128 / 255.0, 0, 128 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FF0000.
        /// </summary>
        public static readonly Color Red = new Color(1, 0, 0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #BC8F8F.
        /// </summary>
        public static readonly Color RosyBrown = new Color(188 / 255.0, 143 / 255.0, 143 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #4169E1.
        /// </summary>
        public static readonly Color RoyalBlue = new Color(65 / 255.0, 105 / 255.0, 225 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #8B4513.
        /// </summary>
        public static readonly Color SaddleBrown = new Color(139 / 255.0, 69 / 255.0, 19 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FA8072.
        /// </summary>
        public static readonly Color Salmon = new Color(250 / 255.0, 128 / 255.0, 114 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #F4A460.
        /// </summary>
        public static readonly Color SandyBrown = new Color(244 / 255.0, 164 / 255.0, 96 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #2E8B57.
        /// </summary>
        public static readonly Color SeaGreen = new Color(46 / 255.0, 139 / 255.0, 87 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFF5EE.
        /// </summary>
        public static readonly Color SeaShell = new Color(1, 245 / 255.0, 238 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #A0522D.
        /// </summary>
        public static readonly Color Sienna = new Color(160 / 255.0, 82 / 255.0, 45 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #C0C0C0.
        /// </summary>
        public static readonly Color Silver = new Color(192 / 255.0, 192 / 255.0, 192 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #87CEEB.
        /// </summary>
        public static readonly Color SkyBlue = new Color(135 / 255.0, 206 / 255.0, 235 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #6A5ACD.
        /// </summary>
        public static readonly Color SlateBlue = new Color(106 / 255.0, 90 / 255.0, 205 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #708090.
        /// </summary>
        public static readonly Color SlateGray = new Color(112 / 255.0, 128 / 255.0, 144 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFFAFA.
        /// </summary>
        public static readonly Color Snow = new Color(1, 250 / 255.0, 250 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #00FF7F.
        /// </summary>
        public static readonly Color SpringGreen = new Color(0, 1, 127 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #4682B4.
        /// </summary>
        public static readonly Color SteelBlue = new Color(70 / 255.0, 130 / 255.0, 180 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #D2B48C.
        /// </summary>
        public static readonly Color Tan = new Color(210 / 255.0, 180 / 255.0, 140 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #008080.
        /// </summary>
        public static readonly Color Teal = new Color(0, 128 / 255.0, 128 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #D8BFD8.
        /// </summary>
        public static readonly Color Thistle = new Color(216 / 255.0, 191 / 255.0, 216 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FF6347.
        /// </summary>
        public static readonly Color Tomato = new Color(1, 99 / 255.0, 71 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an ARGB value of #00FFFFFF.
        /// </summary>
        public static readonly Color Transparent = new Color(1, 1, 1, 0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #40E0D0.
        /// </summary>
        public static readonly Color Turquoise = new Color(64 / 255.0, 224 / 255.0, 208 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #EE82EE.
        /// </summary>
        public static readonly Color Violet = new Color(238 / 255.0, 130 / 255.0, 238 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #F5DEB3.
        /// </summary>
        public static readonly Color Wheat = new Color(245 / 255.0, 222 / 255.0, 179 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFFFFF.
        /// </summary>
        public static readonly Color White = new Color(1, 1, 1);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #F5F5F5.
        /// </summary>
        public static readonly Color WhiteSmoke = new Color(245 / 255.0, 245 / 255.0, 245 / 255.0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #FFFF00.
        /// </summary>
        public static readonly Color Yellow = new Color(1, 1, 0);

        /// <summary>
        /// Gets the pre-defined color that has an RGB value of #9ACD32.
        /// </summary>
        public static readonly Color YellowGreen = new Color(154 / 255.0, 205 / 255.0, 50 / 255.0);

        /// <summary>
        /// Red channel.
        /// </summary>
        public double R;

        /// <summary>
        /// Green channel.
        /// </summary>
        public double G;

        /// <summary>
        /// Blue channel.
        /// </summary>
        public double B;

        /// <summary>
        /// Opaque channel.
        /// </summary>
        public double A;

        /// <summary>
        /// Initializes a new instance of the <see cref="Color" /> structure.
        /// </summary>
        /// <param name="red">Red channel.</param>
        /// <param name="green">Green channel.</param>
        /// <param name="blue">Blue channel.</param>
        /// <param name="alpha">Opaque channel.</param>
        public Color(double red, double green, double blue, double alpha = 1)
            : this()
        {
            R = red;
            G = green;
            B = blue;
            A = alpha;
        }
    }
}
