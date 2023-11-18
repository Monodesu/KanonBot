using System.IO;
using System.Numerics;
using KanonBot.API;
using KanonBot.Functions.OSU.RosuPP;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Img = SixLabors.ImageSharp.Image;
using ResizeOptions = SixLabors.ImageSharp.Processing.ResizeOptions;
using System.Collections.Generic;
using SixLabors.ImageSharp.Diagnostics;
using KanonBot.API.OSU;
using static KanonBot.Image.OSU.ResourceRegistrar;
using static KanonBot.API.OSU.DataStructure;

namespace KanonBot.Image.OSU
{
    public static class OsuUserResourceHelper
    {
        public static string GetAvatarUrl(string userId)
        {
            return $"https://a.ppy.sh/{userId}";
        }

        public static async Task<Image<Rgba32>> GetInfoV2BannerAsync(long osu_uid, UserPanelData.CustomMode ColorMode,float SideImgBrightness)
        {
            string sidePicPath;
            if (File.Exists($"./work/panelv2/user_customimg/{osu_uid}.png"))
                sidePicPath = $"./work/panelv2/user_customimg/{osu_uid}.png";
            else
                sidePicPath = ColorMode switch
                {
                    UserPanelData.CustomMode.Custom => "./work/panelv2/infov2-dark-customimg.png",
                    UserPanelData.CustomMode.Light => "./work/panelv2/infov2-light-customimg.png",
                    UserPanelData.CustomMode.Dark => "./work/panelv2/infov2-dark-customimg.png",
                    _ => throw new ArgumentOutOfRangeException("未知的自定义模式")
                };
            var sidePic = await ReadImageRgba(sidePicPath);
            sidePic.Mutate(x => x.Brightness(SideImgBrightness));
            return sidePic;
        }

        public static async Task<Image<Rgba32>> GetInfoV2PanelAsync(long osu_uid, UserPanelData.CustomMode ColorMode)
        {
            string panelPath;
            if (File.Exists($"./work/panelv2/user_infopanel/{osu_uid}.png"))
                panelPath = $"./work/panelv2/user_infopanel/{osu_uid}.png";
            else
                panelPath = ColorMode switch
                {
                    UserPanelData.CustomMode.Custom => "./work/panelv2/infov2-dark.png",
                    UserPanelData.CustomMode.Light => "./work/panelv2/infov2-light.png",
                    UserPanelData.CustomMode.Dark => "./work/panelv2/infov2-dark.png",
                    _ => throw new ArgumentOutOfRangeException("未知的颜色模式"),
                };
            var panel = await ReadImageRgba(panelPath);
            return panel;
        }

        public static async Task<Image<Rgba32>> GetCountryOrRegionFlagAsync(string code,int version,float CountryFlagAlpha = 1.0f,float CountryFlagBrightness = 1.0f)
        {
            var flags = await ReadImageRgba($"./work/flags/{code}.png");
            if (version == 1)
            {
                return flags;
            }
            else
            {
                flags.Mutate(x => x.Resize(100, 67).Brightness(CountryFlagBrightness));
                flags.Mutate(
                    x =>
                        x.ProcessPixelRowsAsVector4(row =>
                        {
                            for (int p = 0; p < row.Length; p++)
                                if (row[p].W > 0.2f)
                                    row[p].W = CountryFlagAlpha;
                        })
                );
            }
            return flags;
        }
    }
}
