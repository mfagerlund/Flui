using System.Collections.Generic;

namespace FluiDemo.GameSettings
{
    public class GameSettings
    {
        public ControlSettingsC ControlSettings { get; set; } = new();
        public ScreenSettingsC ScreenSettings { get; set; } = new();

        public string CompactString
        {
            get
            {
                var text = "";
                if (ControlSettings.SimpleControls) text += "sc";
                if (ControlSettings.Vibration) text += "v";
                if (ControlSettings.ButtonConfiguration) text += "bc";
                text += $"{ControlSettings.CameraDistance:0.0}";
                if (ControlSettings.ScreenVibration) text += "sv";
                if (ControlSettings.ShowSpecialAttack) text += "ssa";
                text += ControlSettings.ControllerType.ControllerTypeName;
                text += ControlSettings.UserName;
                text += "|" + ScreenSettings.Width + "x" + ScreenSettings.Height + "@" + ScreenSettings.PixelDensity;
                text += "|" + ScreenSettings.ColorModeId;
                return text;
            }
        }

        public class ControlSettingsC
        {
            public bool SimpleControls { get; set; } = true;
            public bool Vibration { get; set; } = true;
            public bool ButtonConfiguration { get; set; }
            public float CameraDistance { get; set; } = 10;
            public bool ScreenVibration { get; set; }
            public bool ShowSpecialAttack { get; set; }
            public string UserName { get; set; } = "Arnold";
            public ControllerType ControllerType { get; set; } = ControllerType.DefaultControllerType;
        }

        public class ControllerType
        {
            public int ControllerTypeId { get; set; }
            public string ControllerTypeName { get; set; }
            public float Cost { get; set; }

            public static List<ControllerType> ControllerTypes { get; }
            public static ControllerType DefaultControllerType { get; }

            static ControllerType()
            {
                ControllerTypes = new List<ControllerType>
                {
                    new() { ControllerTypeId = 1, ControllerTypeName = "Mouse", Cost = 15 },
                    new() { ControllerTypeId = 2, ControllerTypeName = "Mando", Cost = 22.5f },
                    new() { ControllerTypeId = 3, ControllerTypeName = "Remote Control", Cost = 50f },
                    new GreenControllerType { ControllerTypeId = 4, ControllerTypeName = "Stick", Material = "Wood", Cost = 0.5f }
                };
                DefaultControllerType = ControllerTypes[0];
            }
        }

        public class GreenControllerType : ControllerType
        {
            public string Material { get; set; }
        }

        public class ScreenSettingsC
        {
            public int Width { get; set; } = 320;
            public int Height { get; set; } = 240;
            public float PixelDensity { get; set; } = 0.3f;
            public int ColorModeId { get; set; }
            public CycleModeEnum CycleMode { get; set; }
        }

        public enum CycleModeEnum
        {
            Forward,
            Backward,
            ForwardWithNoHands,
            BackwardsInHeels
        }
    }
}