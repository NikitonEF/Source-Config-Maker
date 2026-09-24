using System;
using System.Collections.Generic;

namespace SourceConfigMaker.Models;

public enum CvarControlType { Text, Boolean, Slider }

public class CvarDefinition
{
    public string Name { get; set; } = string.Empty;
    public CvarControlType Type { get; set; } = CvarControlType.Text;
    public string DefaultValue { get; set; } = string.Empty;
    public float Min { get; set; } = 0;
    public float Max { get; set; } = 100;
    public string ToolTipKey { get; set; } = string.Empty;
}

public class CvarDatabase
{
    public Dictionary<string, List<CvarDefinition>> Categories { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        { "ОСНОВНОЕ", new List<CvarDefinition>
            {
                new() { Name = "name", Type = CvarControlType.Text, DefaultValue = "Player", ToolTipKey = "Lang_Tip_name" },
                new() { Name = "default_fov", Type = CvarControlType.Slider, DefaultValue = "90", Min = 70, Max = 120, ToolTipKey = "Lang_Tip_default_fov" },
                new() { Name = "fps_max", Type = CvarControlType.Slider, DefaultValue = "100", Min = 20, Max = 300, ToolTipKey = "Lang_Tip_fps_max" },
                new() { Name = "fps_override", Type = CvarControlType.Boolean, DefaultValue = "0", ToolTipKey = "Lang_Tip_fps_override" },
                new() { Name = "cl_bob", Type = CvarControlType.Slider, DefaultValue = "0.01", Min = 0, Max = 0.1f, ToolTipKey = "Lang_Tip_cl_bob" },
                new() { Name = "cl_hidecorpses", Type = CvarControlType.Boolean, DefaultValue = "0", ToolTipKey = "Lang_Tip_cl_hidecorpses" },
                new() { Name = "m_rawinput", Type = CvarControlType.Boolean, DefaultValue = "1", ToolTipKey = "Lang_Tip_m_rawinput" },
                new() { Name = "m_filter", Type = CvarControlType.Boolean, DefaultValue = "0", ToolTipKey = "Lang_Tip_m_filter" },
                new() { Name = "zoom_sensitivity_ratio", Type = CvarControlType.Slider, DefaultValue = "1.2", Min = 0.5f, Max = 2.0f, ToolTipKey = "Lang_Tip_zoom_sensitivity_ratio" },
                new() { Name = "cl_autojump", Type = CvarControlType.Boolean, DefaultValue = "0", ToolTipKey = "Lang_Tip_cl_autojump" },
                new() { Name = "cl_autorecord", Type = CvarControlType.Boolean, DefaultValue = "0", ToolTipKey = "Lang_Tip_cl_autorecord" }
            }
        },
        { "СЕТЬ", new List<CvarDefinition>
            {
                new() { Name = "rate", Type = CvarControlType.Slider, DefaultValue = "100000", Min = 20000, Max = 100000, ToolTipKey = "Lang_Tip_rate" },
                new() { Name = "cl_updaterate", Type = CvarControlType.Slider, DefaultValue = "102", Min = 10, Max = 102, ToolTipKey = "Lang_Tip_cl_updaterate" },
                new() { Name = "cl_cmdrate", Type = CvarControlType.Slider, DefaultValue = "105", Min = 10, Max = 105, ToolTipKey = "Lang_Tip_cl_cmdrate" },
                new() { Name = "ex_interp", Type = CvarControlType.Slider, DefaultValue = "0.01", Min = 0, Max = 0.1f, ToolTipKey = "Lang_Tip_ex_interp" },
                new() { Name = "cl_dlmax", Type = CvarControlType.Slider, DefaultValue = "128", Min = 16, Max = 1024, ToolTipKey = "Lang_Tip_cl_dlmax" },
                new() { Name = "cl_lc", Type = CvarControlType.Boolean, DefaultValue = "1", ToolTipKey = "Lang_Tip_cl_lc" },
                new() { Name = "cl_lw", Type = CvarControlType.Boolean, DefaultValue = "1", ToolTipKey = "Lang_Tip_cl_lw" },
                new() { Name = "cl_cmdbackup", Type = CvarControlType.Slider, DefaultValue = "2", Min = 0, Max = 10, ToolTipKey = "Lang_Tip_cl_cmdbackup" },
                new() { Name = "cl_timeout", Type = CvarControlType.Slider, DefaultValue = "60", Min = 30, Max = 999, ToolTipKey = "Lang_Tip_cl_timeout" },
                new() { Name = "cl_resend", Type = CvarControlType.Slider, DefaultValue = "6", Min = 1, Max = 10, ToolTipKey = "Lang_Tip_cl_resend" },
                new() { Name = "cl_latency", Type = CvarControlType.Slider, DefaultValue = "0", Min = -100, Max = 100, ToolTipKey = "Lang_Tip_cl_latency" }
            }
        },
        { "ЗВУК", new List<CvarDefinition>
            {
                new() { Name = "volume", Type = CvarControlType.Slider, DefaultValue = "0.5", Min = 0, Max = 1.0f, ToolTipKey = "Lang_Tip_volume" },
                new() { Name = "hisound", Type = CvarControlType.Boolean, DefaultValue = "1", ToolTipKey = "Lang_Tip_hisound" },
                new() { Name = "bgmvolume", Type = CvarControlType.Slider, DefaultValue = "0", Min = 0, Max = 1.0f, ToolTipKey = "Lang_Tip_bgmvolume" },
                new() { Name = "MP3Volume", Type = CvarControlType.Slider, DefaultValue = "0.2", Min = 0, Max = 1.0f, ToolTipKey = "Lang_Tip_MP3Volume" },
                new() { Name = "suitvolume", Type = CvarControlType.Slider, DefaultValue = "0.25", Min = 0, Max = 1.0f, ToolTipKey = "Lang_Tip_suitvolume" },
                new() { Name = "voice_enable", Type = CvarControlType.Boolean, DefaultValue = "1", ToolTipKey = "Lang_Tip_voice_enable" },
                new() { Name = "voice_scale", Type = CvarControlType.Slider, DefaultValue = "1", Min = 0, Max = 2.0f, ToolTipKey = "Lang_Tip_voice_scale" },
                new() { Name = "ambient_level", Type = CvarControlType.Slider, DefaultValue = "0.3", Min = 0, Max = 1.0f, ToolTipKey = "Lang_Tip_ambient_level" },
                new() { Name = "room_off", Type = CvarControlType.Boolean, DefaultValue = "0", ToolTipKey = "Lang_Tip_room_off" },
                new() { Name = "s_a3d", Type = CvarControlType.Boolean, DefaultValue = "0", ToolTipKey = "Lang_Tip_s_a3d" },
                new() { Name = "s_eax", Type = CvarControlType.Boolean, DefaultValue = "0", ToolTipKey = "Lang_Tip_s_eax" }
            }
        },
        { "ВИДЕО", new List<CvarDefinition>
            {
                new() { Name = "gamma", Type = CvarControlType.Slider, DefaultValue = "2.5", Min = 1.8f, Max = 3.0f, ToolTipKey = "Lang_Tip_gamma" },
                new() { Name = "brightness", Type = CvarControlType.Slider, DefaultValue = "1.0", Min = 0.0f, Max = 2.0f, ToolTipKey = "Lang_Tip_brightness" },
                new() { Name = "r_drawviewmodel", Type = CvarControlType.Boolean, DefaultValue = "1", ToolTipKey = "Lang_Tip_r_drawviewmodel" },
                new() { Name = "gl_vsync", Type = CvarControlType.Boolean, DefaultValue = "0", ToolTipKey = "Lang_Tip_gl_vsync" },
                new() { Name = "cl_forceenemymodels", Type = CvarControlType.Boolean, DefaultValue = "0", ToolTipKey = "Lang_Tip_cl_forceenemymodels" },
                new() { Name = "cl_forceteammatemodels", Type = CvarControlType.Boolean, DefaultValue = "0", ToolTipKey = "Lang_Tip_cl_forceteammatemodels" },
                new() { Name = "hud_fastswitch", Type = CvarControlType.Boolean, DefaultValue = "1", ToolTipKey = "Lang_Tip_hud_fastswitch" },
                new() { Name = "net_graph", Type = CvarControlType.Slider, DefaultValue = "0", Min = 0, Max = 3, ToolTipKey = "Lang_Tip_net_graph" }
            }
        },
        { "ПРИЦЕЛ", new List<CvarDefinition>
            {
                new() { Name = "cl_cross", Type = CvarControlType.Boolean, DefaultValue = "1", ToolTipKey = "Lang_Tip_cl_cross" },
                new() { Name = "cl_cross_size", Type = CvarControlType.Slider, DefaultValue = "5", Min = 0, Max = 20, ToolTipKey = "Lang_Tip_cl_cross_size" },
                new() { Name = "cl_cross_color", Type = CvarControlType.Text, DefaultValue = "255 255 255", ToolTipKey = "Lang_Tip_cl_cross_color" },
                new() { Name = "cl_cross_thickness", Type = CvarControlType.Slider, DefaultValue = "1", Min = 0, Max = 10, ToolTipKey = "Lang_Tip_cl_cross_thickness" },
                new() { Name = "cl_cross_gap", Type = CvarControlType.Slider, DefaultValue = "1", Min = -10, Max = 10, ToolTipKey = "Lang_Tip_cl_cross_gap" },
                new() { Name = "cl_cross_dot_size", Type = CvarControlType.Slider, DefaultValue = "0", Min = 0, Max = 5, ToolTipKey = "Lang_Tip_cl_cross_dot_size" },
                new() { Name = "cl_cross_alpha", Type = CvarControlType.Slider, DefaultValue = "255", Min = 0, Max = 255, ToolTipKey = "Lang_Tip_cl_cross_alpha" }
            }
        }
    };
}