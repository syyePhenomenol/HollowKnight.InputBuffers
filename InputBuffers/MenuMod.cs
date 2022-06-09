using Modding;
using System.Collections.Generic;
using IB = InputBuffers.InputBuffers;

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
                    IB.GS.BufferJump = !IB.GS.BufferJump;
                },
                Loader = () => IB.GS.BufferJump ? 1 : 0
            },

            new()
            {
                Name = "Buffer Dash",
                Description = "",
                Values = new string[] { "Off", "On" },
                Saver = opt =>
                {
                    IB.GS.BufferDash = !IB.GS.BufferDash;
                },
                Loader = () => IB.GS.BufferDash ? 1 : 0
            },

            new()
            {
                Name = "Buffer Attacks",
                Description = "",
                Values = new string[] { "Off", "On" },
                Saver = opt =>
                {
                    IB.GS.BufferAttack = !IB.GS.BufferAttack;
                },
                Loader = () => IB.GS.BufferAttack ? 1 : 0
            },

            new()
            {
                Name = "Buffer Casts",
                Description = "",
                Values = new string[] { "Off", "On" },
                Saver = opt =>
                {
                    IB.GS.BufferCast = !IB.GS.BufferCast;
                },
                Loader = () => IB.GS.BufferCast ? 1 : 0
            },

            new()
            {
                Name = "Buffer Quick Casts",
                Description = "",
                Values = new string[] { "Off", "On" },
                Saver = opt =>
                {
                    IB.GS.BufferQuickCast = !IB.GS.BufferQuickCast;
                },
                Loader = () => IB.GS.BufferQuickCast ? 1 : 0
            },

            new()
            {
                Name = "Buffer Dream Nail",
                Description = "",
                Values = new string[] { "Off", "On" },
                Saver = opt =>
                {
                    IB.GS.BufferDreamNail = !IB.GS.BufferDreamNail;
                },
                Loader = () => IB.GS.BufferDreamNail ? 1 : 0
            },

            new()
            {
                Name = "Buffer Superdash",
                Description = "",
                Values = new string[] { "Off", "On" },
                Saver = opt =>
                {
                    IB.GS.BufferSuperdash = !IB.GS.BufferSuperdash;
                },
                Loader = () => IB.GS.BufferSuperdash ? 1 : 0
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
                    IB.GS.BufferDuration = opt;
                },
                Loader = () => IB.GS.BufferDuration
            },

            new()
            {
                Name = "Superdash Release",
                Description = "Hold the direction you are facing to automatically release superdash",
                Values = new string[] { "Off", "On" },
                Saver = opt =>
                {
                    IB.GS.SuperdashRelease = !IB.GS.SuperdashRelease;
                },
                Loader = () => IB.GS.SuperdashRelease ? 1 : 0
            }
        };
    }
}
