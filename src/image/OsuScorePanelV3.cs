using System.IO;
using System.Numerics;
using KanonBot.API;
using KanonBot.functions.osu.rosupp;
using KanonBot.Image;
using KanonBot.LegacyImage;
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
using static KanonBot.LegacyImage.Draw;
using Img = SixLabors.ImageSharp.Image;
using ResizeOptions = SixLabors.ImageSharp.Processing.ResizeOptions;
using System.Collections.Generic;
using SixLabors.ImageSharp.Diagnostics;

namespace KanonBot.DrawV3
{
    public static class OsuScorePanelV3
    {
        public static async Task<Img> Draw(ScorePanelData data)
        {
            var scoreimg = new Image<Rgba32>(2848, 1602);

            var ppInfo = data.ppInfo;

            //下载BG
            var bgPath = $"./work/background/{data.scoreInfo.Beatmap!.BeatmapId}.png";
            if (!File.Exists(bgPath))
            {
                try
                {
                    bgPath = await OSU.SayoDownloadBeatmapBackgroundImg(
                        data.scoreInfo.Beatmap.BeatmapsetId,
                        data.scoreInfo.Beatmap.BeatmapId,
                        "./work/background/"
                    );
                }
                catch (Exception ex)
                {
                    var msg = $"从API下载背景图片时发生了一处异常\n异常类型: {ex.GetType()}\n异常信息: '{ex.Message}'";
                    Log.Warning(msg);
                }
            }

            //下载头像
            var avatarPath = $"./work/avatar/{data.scoreInfo.UserId}.png";
            using var avatar = await TryAsync(ReadImageRgba(avatarPath))
                .IfFail(async () =>
                {
                    try
                    {
                        avatarPath = await data.scoreInfo.User!.AvatarUrl.DownloadFileAsync(
                            "./work/avatar/",
                            $"{data.scoreInfo.UserId}.png"
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

            //panel
            using var panel = data.scoreInfo.Passed ?
                await Img.LoadAsync("./work/panelv2/score_panel/Score_v3_Passed_Panel.png") :
                await Img.LoadAsync("./work/panelv2/score_panel/Score_v3_Failed_Panel.png");
            if (data.scoreInfo.Passed)
                scoreimg.Mutate(x => x.DrawImage(panel, 1));
            else
                scoreimg.Mutate(x => x.DrawImage(panel, 1));

            // bg
            using var bg = await TryAsync(ReadImageRgba(bgPath!))
                .IfFail(async () =>
                {
                    return await ReadImageRgba("./work/legacy/load-failed-img.png"); // 下载后再读取
                });

            using var bgarea = new Image<Rgba32>(631, 444);
            bgarea.Mutate(x => x.Fill(Color.ParseHex("#f2f2f2")).RoundCorner(new Size(631, 444), 20));

            //beatmap status
            using var bgstatus = new Image<Rgba32>(619, 80);
            var beatmap_status_color = data.scoreInfo.Beatmap.Status switch
            {
                OSU.Enums.Status.approved => Color.ParseHex("#14b400"),
                OSU.Enums.Status.ranked => Color.ParseHex("#66bdff"),
                OSU.Enums.Status.loved => Color.ParseHex("#ff66aa"),
                _ => Color.ParseHex("#e08918")
            };
            bgstatus.Mutate(x => x.Fill(beatmap_status_color).RoundCorner(new Size(619, 80), 20));
            bgarea.Mutate(x => x.DrawImage(bgstatus, new Point(6, 338), 1));
            bgarea.Mutate(x => x.DrawImage(bg.Clone(x => x.RoundCorner(new Size(619, 401), 20)), new Point(6, 6), 1));
            scoreimg.Mutate(x => x.DrawImage(bgarea, new Point(70, 51), 1));






















            return scoreimg;
        }
    }
}
