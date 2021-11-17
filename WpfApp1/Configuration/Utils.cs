/**
 *   Copyright (C) 2021 okaygo
 *
 *   https://github.com/misterokaygo/MapAssist/
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 **/

using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using MapAssist.Types;
using SkiaSharp;

namespace MapAssist.Settings
{
    public static class Utils
    {
        private static string _defaultColor = "#DD0000";
        public static string[] ParseCommaSeparatedNpcsByName(string npc)
        {
            return npc
                .Split(',')
                .Select(o => LookupNpcByName(o.Trim()))
                .Where(o => o != "")
                .ToArray();
        }
        public static Area[] ParseCommaSeparatedAreasByName(string areas)
        {
            return areas
                .Split(',')
                .Select(o => LookupAreaByName(o.Trim()))
                .Where(o => o != Area.None)
                .ToArray();
        }

        private static string LookupNpcByName(string name)
        {
            var name2 = "";
            try
            {
                name2 = Enum.GetName(typeof(Npc), name);
            }
            catch
            {
                return name;
            }
            return name2;
        }
        private static Area LookupAreaByName(string name)
        {
            return Enum.GetValues(typeof(Area)).Cast<Area>().FirstOrDefault(area => area.Name() == name);
        }

        private static T GetConfigValue<T>(string key, Func<string, T> converter, T fallback = default)
        {
            string valueString = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(valueString) ? fallback : converter.Invoke(valueString);
        }

        public static SKColor ParseColor(string value)
        {
            value = value.ToUpper();
            if (value.StartsWith("#"))
            {
                if (SKColor.TryParse(value, out var color))
                    return color;
                Console.WriteLine($"ERROR: Could not parse color from string {value}");
                return SKColor.Parse(_defaultColor);
            }

            if (!value.Contains(","))
            {
                if (SKColor.TryParse(value, out var color))
                    return color;
                Console.WriteLine($"ERROR: Could not parse color from string {value}");
                return SKColor.Parse(_defaultColor);
            }

            byte[] bytes = value.Split(',').Select(o => byte.Parse(o.Trim())).ToArray();
            switch (bytes.Length)
            {
                case 4:
                    return new SKColor(bytes[0], bytes[1], bytes[2], bytes[3]);
                case 3:
                    return new SKColor(bytes[0], bytes[1], bytes[2]);
            }

            Console.WriteLine($"ERROR: Could not parse color from string {value}");
            return SKColor.Parse(_defaultColor);
        }

        public static PointOfInterestRendering GetRenderingSettingsForPrefix(string name)
        {
            return new PointOfInterestRendering
            {
                IconColor = GetConfigValue($"{name}.IconColor", ParseColor, SKColor.Empty),
                IconShape = GetConfigValue($"{name}.IconShape", t => (Shape)Enum.Parse(typeof(Shape), t, true)),
                IconSize = GetConfigValue($"{name}.IconSize", Convert.ToInt32),
                LineColor = GetConfigValue($"{name}.LineColor", ParseColor, SKColor.Empty),
                LineThickness = GetConfigValue($"{name}.LineThickness", Convert.ToSingle, 1),
                ArrowHeadSize = GetConfigValue($"{name}.ArrowHeadSize", Convert.ToInt32),
                LabelColor = GetConfigValue($"{name}.LabelColor", ParseColor, SKColor.Empty),
                LabelFont = GetConfigValue($"{name}.LabelFont", t => t, "Arial"),
                LabelFontSize = GetConfigValue($"{name}.LabelFontSize", Convert.ToInt32, 14),
            };
        }
        public static MobRendering GetMobRendering()
        {
            return new MobRendering
            {
                NormalColor = GetConfigValue($"MobColor.Normal", ParseColor, SKColor.Empty),
                UniqueColor = GetConfigValue($"MobColor.Unique", ParseColor, SKColor.Empty)
            };
        }
    }
}
