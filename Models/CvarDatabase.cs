using System.Collections.Generic;

namespace SourceConfigMaker.Models;

public class CvarDatabase
{
    // Словарь категорий и их консольных команд (Cvars)
    public Dictionary<string, string[]> Categories { get; } = new()
    {
        { "ОСНОВНОЕ", new[] { "name", "default_fov", "fps_max", "fps_override", "cl_bob", "cl_hidecorpses", "m_rawinput", "m_filter", "zoom_sensitivity_ratio", "cl_autojump", "cl_autorecord" } },
        { "СЕТЬ", new[] { "rate", "cl_updaterate", "cl_cmdrate", "ex_interp", "cl_dlmax", "cl_lc", "cl_lw", "cl_cmdbackup", "cl_timeout", "cl_resend", "cl_latency" } },
        { "ЗВУК", new[] { "volume", "hisound", "bgmvolume", "MP3Volume", "suitvolume", "voice_enable", "voice_scale", "ambient_level", "room_off", "s_a3d", "s_eax" } },
        { "ВИДЕО", new[] { "gamma", "brightness", "r_drawviewmodel", "gl_vsync", "cl_forceenemymodels", "cl_forceteammatemodels", "hud_fastswitch", "net_graph" } },
        { "ПРИЦЕЛ", new[] { "cl_cross", "cl_cross_size", "cl_cross_color", "cl_cross_thickness", "cl_cross_gap", "cl_cross_dot_size", "cl_cross_alpha" } }
    };
}