using Modding;
using System.Collections.Generic;

namespace InputBuffers
{
    internal static class MenuMod
    {
        internal static List<IMenuMod.MenuEntry> Menu = new()
        {
            new()
            {
                Name = "Buffer Jumps",
                Description = "",
                Values = new string[] { "Off", "On" },
                Saver = opt =>
                {
                    InputBuffers.GS.BufferJump = !InputBuffers.GS.BufferJump;
                },
                Loader = () => InputBuffers.GS.BufferJump ? 1 : 0
            },

            new()
            {
                Name = "Buffer Dash",
                Description = "",
                Values = new string[] { "Off", "On" },
                Saver = opt =>
                {
                    InputBuffers.GS.BufferDash = !InputBuffers.GS.BufferDash;
                },
                Loader = () => InputBuffers.GS.BufferDash ? 1 : 0
            },

            new()
            {
                Name = "Buffer Attacks",
                Description = "",
                Values = new string[] { "Off", "On" },
                Saver = opt =>
                {
                    InputBuffers.GS.BufferAttack = !InputBuffers.GS.BufferAttack;
                },
                Loader = () => InputBuffers.GS.BufferAttack ? 1 : 0
            },

            new()
            {
                Name = "Buffer Casts",
                Description = "",
                Values = new string[] { "Off", "On" },
                Saver = opt =>
                {
                    InputBuffers.GS.BufferCast = !InputBuffers.GS.BufferCast;
                },
                Loader = () => InputBuffers.GS.BufferCast ? 1 : 0
            },

            new()
            {
                Name = "Buffer Quick Casts",
                Description = "",
                Values = new string[] { "Off", "On" },
                Saver = opt =>
                {
                    InputBuffers.GS.BufferQuickCast = !InputBuffers.GS.BufferQuickCast;
                },
                Loader = () => InputBuffers.GS.BufferQuickCast ? 1 : 0
            },

            new()
            {
                Name = "Buffer Dream Nail",
                Description = "",
                Values = new string[] { "Off", "On" },
                Saver = opt =>
                {
                    InputBuffers.GS.BufferDreamNail = !InputBuffers.GS.BufferDreamNail;
                },
                Loader = () => InputBuffers.GS.BufferDreamNail ? 1 : 0
            },

            new()
            {
                Name = "Buffer Superdash",
                Description = "",
                Values = new string[] { "Off", "On" },
                Saver = opt =>
                {
                    InputBuffers.GS.BufferSuperdash = !InputBuffers.GS.BufferSuperdash;
                },
                Loader = () => InputBuffers.GS.BufferSuperdash ? 1 : 0
            },

            new()
            {
                Name = "Buffer Duration",
                Description = "",
                Values = new string[]
                    {
                        "50 ms (2.5 frames)",
                        "100 ms (5 frames)",
                        "200 ms (10 frames)",
                        "300 ms (15 frames)",
                        "400 ms (20 frames)",
                        "500 ms (25 frames)",
                        "750 ms (37.5 frames)",
                        "1000 ms (50 frames)",
                    },
                Saver = opt =>
                {
                    InputBuffers.GS.BufferDuration = opt;
                },
                Loader = () => InputBuffers.GS.BufferDuration
            },

            new()
            {
                Name = "Superdash Release",
                Description = "Hold the direction you are facing to automatically release superdash",
                Values = new string[] { "Off", "On" },
                Saver = opt =>
                {
                    InputBuffers.GS.SuperdashRelease = !InputBuffers.GS.SuperdashRelease;
                },
                Loader = () => InputBuffers.GS.SuperdashRelease ? 1 : 0
            }
        };
    }
}
