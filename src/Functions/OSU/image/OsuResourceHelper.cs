using System.Collections.Generic;
using System.IO;
using System.Numerics;
using KanonBot.API;
using KanonBot.API.OSU;
using LanguageExt.ClassInstances;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.Diagnostics;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static KanonBot.API.OSU.DataStructure;
using static KanonBot.Image.OSU.ResourceRegistrar;
using Img = SixLabors.ImageSharp.Image;
using ResizeOptions = SixLabors.ImageSharp.Processing.ResizeOptions;

namespace KanonBot.Image.OSU
{
    public static class OsuResourceHelper
    {
        public static async Task<Image<Rgba32>> GetUserAvatarAsync(long osu_uid, Uri avatar_url)
        {
            var avatarPath = $"./work/avatar/{osu_uid}.png";
            var avatar = await TryAsync(ReadImageRgba(avatarPath))
                .IfFail(async () =>
                {
                    try
                    {
                        avatarPath = await avatar_url.DownloadFileAsync(
                            "./work/avatar/",
                            $"{osu_uid}.png"
                        );
                    }
                    catch (Exception ex)
                    {
                        var msg = $"从API下载用户头像时发生了一处异常\n异常类型: {ex.GetType()}\n异常信息: '{ex.Message}'";
                        Log.Error(msg);
                        throw;
                    }
                    return await ReadImageRgba(avatarPath); // 下载后再读取
                });
            return avatar!;
        }

        public static async Task<Image<Rgba32>> GetInfoV1PanelAsync(long osu_uid)
        {
            var bannerPath = "./work/legacy/default-info-v1.png";
            if (File.Exists($"./work/legacy/v1_infopanel/{osu_uid}.png"))
                bannerPath = $"./work/legacy/v1_infopanel/{osu_uid}.png";
            return await ReadImageRgba(bannerPath);
        }

        public static async Task<Image<Rgba32>> GetInfoCoverAsync(long osu_uid, Uri CoverUrl)
        {
            var coverPath = $"./work/legacy/v1_cover/custom/{osu_uid}.png";
            if (!File.Exists(coverPath))
            {
                coverPath = $"./work/legacy/v1_cover/osu!web/{osu_uid}.png";
                if (!File.Exists(coverPath))
                {
                    try
                    {
                        coverPath = await CoverUrl.DownloadFileAsync(
                            "./work/legacy/v1_cover/osu!web/",
                            $"{osu_uid}.png"
                        );
                    }
                    catch
                    {
                        int n = new Random().Next(1, 6);
                        coverPath = $"./work/legacy/v1_cover/default/default_{n}.png";
                    }
                }
            }

            return await ReadImageRgba(coverPath);
        }

        public static async Task<Image<Rgba32>> GetInfoV2BannerAsync(
            long osu_uid,
            UserPanelData.CustomMode ColorMode,
            float SideImgBrightness
        )
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

        public static async Task<Image<Rgba32>> GetInfoV2PanelAsync(
            long osu_uid,
            UserPanelData.CustomMode ColorMode
        )
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

        public static async Task<Image<Rgba32>> GetCountryOrRegionFlagAsync(
            string code,
            int version,
            float CountryFlagAlpha = 1.0f,
            float CountryFlagBrightness = 1.0f
        )
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

        public static async Task<Image<Rgba32>> GetBeatmapBackgroundImageAsync(long sid, long bid)
        {
            var bp1bgPath = $"./work/background/{bid}.png";
            if (!File.Exists(bp1bgPath))
            {
                try
                {
                    bp1bgPath = await V2.SayoDownloadBeatmapBackgroundImg(
                        sid,
                        bid,
                        "./work/background/"
                    );
                }
                catch (Exception ex)
                {
                    var msg = $"从API下载背景图片时发生了一处异常\n异常类型: {ex.GetType()}\n异常信息: '{ex.Message}'";
                    Log.Warning(msg);
                }
            }
            return await ReadImageRgba(bp1bgPath!);
        }
    }
}
