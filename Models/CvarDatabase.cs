using System;
using System.Collections.Generic;

namespace SourceConfigMaker.Models;

public class CvarDatabase
{
    // Оставляем структуру для UI
    public Dictionary<string, string[]> Categories { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        { "ОСНОВНОЕ", new[] { "name", "default_fov", "fps_max", "fps_override", "cl_bob", "cl_hidecorpses", "m_rawinput", "m_filter", "zoom_sensitivity_ratio", "cl_autojump", "cl_autorecord" } },
        { "СЕТЬ", new[] { "rate", "cl_updaterate", "cl_cmdrate", "ex_interp", "cl_dlmax", "cl_lc", "cl_lw", "cl_cmdbackup", "cl_timeout", "cl_resend", "cl_latency" } },
        { "ЗВУК", new[] { "volume", "hisound", "bgmvolume", "MP3Volume", "suitvolume", "voice_enable", "voice_scale", "ambient_level", "room_off", "s_a3d", "s_eax" } },
        { "ВИДЕО", new[] { "gamma", "brightness", "r_drawviewmodel", "gl_vsync", "cl_forceenemymodels", "cl_forceteammatemodels", "hud_fastswitch", "net_graph" } },
        { "ПРИЦЕЛ", new[] { "cl_cross", "cl_cross_size", "cl_cross_color", "cl_cross_thickness", "cl_cross_gap", "cl_cross_dot_size", "cl_cross_alpha" } }
    };

    // НОВОЕ: Хардкодим дефолтные значения движка (можешь поправить под свои нужды)
    public Dictionary<string, string> DefaultValues { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        // ОСНОВНОЕ
        { "name", "Player" }, { "default_fov", "90" }, { "fps_max", "100" }, { "fps_override", "0" },
        { "cl_bob", "0.01" }, { "cl_hidecorpses", "0" }, { "m_rawinput", "1" }, { "m_filter", "0" },
        { "zoom_sensitivity_ratio", "1.2" }, { "cl_autojump", "0" }, { "cl_autorecord", "0" },
        
        // СЕТЬ
        { "rate", "100000" }, { "cl_updaterate", "102" }, { "cl_cmdrate", "105" }, { "ex_interp", "0.01" },
        { "cl_dlmax", "128" }, { "cl_lc", "1" }, { "cl_lw", "1" }, { "cl_cmdbackup", "2" },
        { "cl_timeout", "60" }, { "cl_resend", "6" }, { "cl_latency", "0" },
        
        // ЗВУК
        { "volume", "0.5" }, { "hisound", "1" }, { "bgmvolume", "0" }, { "MP3Volume", "0.2" },
        { "suitvolume", "0.25" }, { "voice_enable", "1" }, { "voice_scale", "1" },
        { "ambient_level", "0.3" }, { "room_off", "0" }, { "s_a3d", "0" }, { "s_eax", "0" },
        
        // ВИДЕО
        { "gamma", "2.5" }, { "brightness", "1.0" }, { "r_drawviewmodel", "1" }, { "gl_vsync", "0" },
        { "cl_forceenemymodels", "0" }, { "cl_forceteammatemodels", "0" }, { "hud_fastswitch", "1" }, { "net_graph", "0" },
        
        // ПРИЦЕЛ
        { "cl_cross", "1" }, { "cl_cross_size", "5" }, { "cl_cross_color", "255 255 255" },
        { "cl_cross_thickness", "1" }, { "cl_cross_gap", "1" }, { "cl_cross_dot_size", "0" }, { "cl_cross_alpha", "255" }
    };
}