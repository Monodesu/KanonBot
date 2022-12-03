using System.IO;
using System.Numerics;
using Flurl;
using Flurl.Http;
using Flurl.Util;
using KanonBot.API;
using KanonBot.Drivers;
using KanonBot.functions.osu.rosupp;
using KanonBot.Image;
using KanonBot.LegacyImage;
using KanonBot.Message;
using Serilog;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SqlSugar;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static KanonBot.LegacyImage.Draw;
using Img = SixLabors.ImageSharp.Image;
using ResizeOptions = SixLabors.ImageSharp.Processing.ResizeOptions;

namespace KanonBot.functions.osubot
{
    public class Setter
    {
        public static async Task<bool> RestrictedDetect(Target target, string permissions)
        {
            if (permissions == "restricted")
            {
                await target.reply("账户已被限制。");
                return false;
            }
            return true;
        }
        public static async Task Execute(Target target, string cmd)
        {
            string rootCmd, childCmd = "";
            try
            {
                var tmp = cmd.SplitOnFirstOccurence(" ");
                rootCmd = tmp[0].Trim();
                childCmd = tmp[1].Trim();
            }
            catch { rootCmd = cmd; }
            switch (rootCmd.ToLower())
            {
                case "osumode":
                    await Osu_mode(target, childCmd);
                    break;
                case "osuinfopanelversion":
                    await Osu_InfoPanelVersion(target, childCmd);
                    break;
                case "osuinfopanelv2colormode":
                    await Osu_InfoPanelV2ColorMode(target, childCmd);
                    break;
                case "osuinfopanelv2img":
                    await Osu_InfoPanelV2IMG(target, childCmd);
                    break;
                case "osuinfopanelv1img":
                    await Osu_InfoPanelV1IMG(target, childCmd);
                    break;
                case "osuinfopanelv2panel":
                    await Osu_InfoPanelV2Panel(target, childCmd);
                    break;
                case "osuinfopanelv1panel":
                    await Osu_InfoPanelV1Panel(target, childCmd);
                    break;
                case "osuinfopanelv2colorcustom":
                    await Osu_InfoPanelV2CustomColorValue(target, childCmd);
                    break;
                default:
                    await target.reply(
                        """
                        !set osumode
                             osuinfopanelversion
                             osuinfopanelv1img
                             osuinfopanelv2img
                             osuinfopanelv1panel
                             osuinfopanelv2panel
                             osuinfopanelv2colormode
                             osuinfopanelv2colorcustom
                        """
                    );
                    return;
            }
        }
        async public static Task Osu_InfoPanelVersion(Target target, string cmd)
        {
            var AccInfo = Accounts.GetAccInfo(target);
            var DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { await target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

            cmd = cmd.ToLower().Trim();
            if (int.TryParse(cmd, out _))
            {
                try
                {
                    var t = int.Parse(cmd);
                    if (t < 1 || t > 2)
                    {
                        await target.reply("版本号不正确。(1、2)");
                        return;
                    }
                    await Database.Client.SetOsuInfoPanelVersion(DBOsuInfo.osu_uid, t);
                    await target.reply("成功设置infopanel版本。");
                }
                catch
                {
                    await target.reply("发生了错误，无法设置infopanel版本，请联系管理员。");
                }
            }
            else
            {
                await target.reply("版本号不正确。(1、2)");
            }
        }
        async public static Task Osu_InfoPanelV2ColorMode(Target target, string cmd)
        {
            var AccInfo = Accounts.GetAccInfo(target);
            var DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { await target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

            cmd = cmd.ToLower().Trim();
            if (int.TryParse(cmd, out _))
            {
                try
                {
                    var t = int.Parse(cmd);
                    if (t < 0 || t > 2)
                    {
                        await target.reply("配色方案号码不正确。(0=custom、1=light、2=dark)");
                        return;
                    }
                    if (t == 0)
                        if (DBOsuInfo.InfoPanelV2_CustomMode == null || DBOsuInfo.InfoPanelV2_CustomMode == "")
                        {
                            await target.reply("请先设置自定义配色方案后再将此配置项更改为custom。");
                            return;
                        }
                    await Database.Client.SetOsuInfoPanelV2ColorMode(DBOsuInfo.osu_uid, t);
                    await target.reply("成功设置配色方案。");
                }
                catch
                {
                    await target.reply("发生了错误，无法设置配色方案，请联系管理员。");
                }
            }
            else
            {
                await target.reply("配色方案号码不正确。(0=light、1=dark)");
            }
        }
        async public static Task Osu_mode(Target target, string cmd)
        {
            var AccInfo = Accounts.GetAccInfo(target);
            var DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            // { await target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
            { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { await target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

            cmd = cmd.ToLower().Trim();

            var mode = OSU.Enums.String2Mode(cmd);
            if (mode == null)
            {
                await target.reply("提供的模式不正确，请重新确认 (osu/taiko/fruits/mania)");
                return;
            }
            else
            {
                try
                {
                    await Database.Client.SetOsuUserMode(DBOsuInfo.osu_uid, mode.Value);
                    await target.reply("成功设置模式为 " + cmd);
                }
                catch
                {
                    await target.reply("发生了错误，无法设置osu模式，请联系管理员。");
                }
            }
        }

        async public static Task Osu_InfoPanelV2IMG(Target target, string cmd)
        {
            var AccInfo = Accounts.GetAccInfo(target);
            var DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { await target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

            //权限检测
            if (!await RestrictedDetect(target, DBUser.permissions!)) return;

            cmd = cmd.Trim();
            if (!Utils.IsUrl(cmd))
            {
                if (cmd.ToLower() == "reset" || cmd.ToLower() == "delete")
                {
                    //删除info image
                    try
                    {
                        File.Delete($"./work/panelv2/user_customimg/{DBOsuInfo.osu_uid}.png");
                        await target.reply("已重置。");
                        return;
                    }
                    catch
                    {
                        await target.reply("重置时发生了错误，请联系管理员。");
                        return;
                    }
                }
                else
                {
                    await target.reply("url不正确，请重试。!set osuinfopanelv2img [url]");
                    return;
                }
            }

            //接收
            string randstr = Utils.RandomStr(50);
            Img image;
            var imagePath = $"./work/tmp/{randstr}.png";
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
            //从url下载
            try
            {
                await target.reply("正在处理...");
                imagePath = await cmd.DownloadFileAsync($"./work/tmp/", $"{randstr}.png");
            }
            catch (Exception ex)
            {
                await target.reply($"接收图片失败，请确保提供的链接为直链\n异常信息: '{ex.Message}");
                return;
            }
            try
            {
                image = new Image<Rgba32>(1382, 2456);
                var temppic = Img.Load(imagePath).CloneAs<Rgba32>();
                temppic.Mutate(x => x.Resize(new ResizeOptions() { Size = new Size(0, 2456), Mode = ResizeMode.Max }));
                if (temppic.Width < 1382)
                {
                    temppic.Dispose();
                    File.Delete(imagePath);
                    await target.reply("图像长度太小了，自己裁剪一下再上传吧~（9:16，推荐1382x2456)");
                    return;
                }
                image.Mutate(x => x.DrawImage(temppic, new Point(0, 0), 1));
                image.Save($"./work/panelv2/user_customimg/verify/{DBOsuInfo.osu_uid}.png", new PngEncoder());
                temppic.Dispose();
                File.Delete(imagePath);
                await target.reply("已成功上传，请耐心等待审核。\n（*如长时间审核未通过则表示不符合规定，请重新上传或联系管理员）");
                Utils.SendMail("mono@desu.life", "有新的v2 info image需要审核", $"osuid: {DBOsuInfo.osu_uid}  请及时查看\n<img src={cmd}>", true);
                Utils.SendMail("fantasyzhjk@qq.com", "有新的v2 info image需要审核", $"osuid: {DBOsuInfo.osu_uid}  请及时查看\n<img src={cmd}>", true);
            }
            catch
            {
                await target.reply("发生了未知错误，请联系管理员。");
                return;
            }
        }

        async public static Task Osu_InfoPanelV1IMG(Target target, string cmd)
        {
            var AccInfo = Accounts.GetAccInfo(target);
            var DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { await target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

            //权限检测
            if (!await RestrictedDetect(target, DBUser.permissions!)) return;

            cmd = cmd.Trim();
            if (!Utils.IsUrl(cmd))
            {
                if (cmd.ToLower() == "reset" || cmd.ToLower() == "delete")
                {
                    //删除info image
                    try
                    {
                        File.Delete($"./work/legacy/v1_cover/custom/{DBOsuInfo.osu_uid}.png");
                        await target.reply("已重置。");
                        return;
                    }
                    catch
                    {
                        await target.reply("重置时发生了错误，请联系管理员。");
                        return;
                    }
                }
                else
                {
                    await target.reply("url不正确，请重试。!set osuinfopanelv1img [url]");
                    return;
                }
            }

            //接收
            string randstr = Utils.RandomStr(50);
            Img image;
            var imagePath = $"./work/tmp/{randstr}.png";
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);

            }
            //从url下载
            try
            {
                await target.reply("正在处理...");
                imagePath = await cmd.DownloadFileAsync($"./work/tmp/", $"{randstr}.png");
            }
            catch (Exception ex)
            {
                await target.reply($"接收图片失败，请确保提供的链接为直链\n异常信息: '{ex.Message}");
                return;
            }
            try
            {
                image = new Image<Rgba32>(1200, 350);
                var temppic = Img.Load(imagePath).CloneAs<Rgba32>();
                temppic.Mutate(x => x.Resize(new ResizeOptions() { Size = new Size(1200, 0), Mode = ResizeMode.Max }));
                if (temppic.Height < 350)
                {
                    temppic.Dispose();
                    File.Delete(imagePath);
                    await target.reply("图像长度太小了，自己裁剪一下再上传吧~（1200x350)");
                    return;
                }
                image.Mutate(x => x.DrawImage(temppic, new Point(0, 0), 1));
                image.Save($"./work/legacy/v1_cover/custom/verify/{DBOsuInfo.osu_uid}.png", new PngEncoder());
                temppic.Dispose();
                File.Delete(imagePath);
                await target.reply("已成功上传，请耐心等待审核。\n（*如长时间审核未通过则表示不符合规定，请重新上传或联系管理员）");
                Utils.SendMail("mono@desu.life", "有新的v1 info image需要审核", $"osuid: {DBOsuInfo.osu_uid}  请及时查看\n<img src={cmd}>", true);
                Utils.SendMail("fantasyzhjk@qq.com", "有新的v1 info image需要审核", $"osuid: {DBOsuInfo.osu_uid}  请及时查看\n<img src={cmd}>", true);
            }
            catch
            {
                await target.reply("发生了未知错误，请联系管理员。");
                return;
            }
        }

        async public static Task Osu_InfoPanelV2Panel(Target target, string cmd)
        {
            var AccInfo = Accounts.GetAccInfo(target);
            var DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { await target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

            //权限检测
            if (!await RestrictedDetect(target, DBUser.permissions!)) return;

            cmd = cmd.Trim();
            if (!Utils.IsUrl(cmd))
            {
                if (cmd.ToLower() == "reset" || cmd.ToLower() == "delete")
                {
                    //删除info image
                    try
                    {
                        File.Delete($"./work/panelv2/user_infopanel/{DBOsuInfo.osu_uid}.png");
                        await target.reply("已重置。");
                        return;
                    }
                    catch
                    {
                        await target.reply("重置时发生了错误，请联系管理员。");
                        return;
                    }
                }
                else
                {
                    await target.reply("url不正确，请重试。!set osuinfopanelv2 [url]");
                    return;
                }
            }

            //接收
            string randstr = Utils.RandomStr(50);
            var imagePath = $"./work/tmp/{randstr}.png";
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
            //从url下载
            try
            {
                await target.reply("正在处理...");
                imagePath = await cmd.DownloadFileAsync($"./work/tmp/", $"{randstr}.png");
            }
            catch (Exception ex)
            {
                await target.reply($"接收图片失败，请确保提供的链接为直链\n异常信息: '{ex.Message}");
                return;
            }
            try
            {
                var temppic = Img.Load(imagePath, out IImageFormat format).CloneAs<Rgba32>();
                //检测上传的infopanel尺寸、开孔是否正确
                bool isok = true;
                string errormsg = "上传的图像不符合infopanel的条件，请重新上传。\n";
                if (temppic.Height != 2640)
                {
                    errormsg += $"图像宽为{temppic.Height}px，要求为2640px\n";
                    isok = false;
                }
                if (temppic.Width != 4000)
                {
                    errormsg += $"图像长为{temppic.Width}px，要求为4000px\n";
                    isok = false;
                }
                if (format.DefaultMimeType.Trim()[(format.DefaultMimeType.Trim().IndexOf("/") + 1)..] != "png")
                {
                    errormsg += $"图像编码为{format.DefaultMimeType.Trim()[(format.DefaultMimeType.Trim().IndexOf("/") + 1)..]}，要求为png\n";
                    isok = false;
                }
                //检测开孔
                if (isok) temppic.ProcessPixelRows(x =>
                {
                    //pp
                    Span<Rgba32> row = x.GetRowSpan(445);
                    if (row[3100].A > 250)
                    {
                        errormsg += $"pp进度条区域被非透明颜色填充，要求保持透明\n";
                        isok = false;
                    }
                    //acc
                    row = x.GetRowSpan(645);
                    if (row[3100].A > 250)
                    {
                        errormsg += $"acc进度条区域被非透明颜色填充，要求保持透明\n";
                        isok = false;
                    }
                    //level
                    row = x.GetRowSpan(600);
                    if (row[3910].A > 250)
                    {
                        errormsg += $"level进度条区域被非透明颜色填充，要求保持透明\n";
                        isok = false;
                    }
                    //bpimg
                    row = x.GetRowSpan(1650);
                    if (row[1800].A > 250)
                    {
                        errormsg += $"mainbp图片区域被非透明颜色填充，要求保持透明\n";
                        isok = false;
                    }
                });

                if (!isok)
                {
                    temppic.Dispose();
                    File.Delete(imagePath);
                    await target.reply(errormsg);
                    return;
                }
                temppic.Save($"./work/panelv2/user_infopanel/verify/{DBOsuInfo.osu_uid}.png", new PngEncoder());
                temppic.Dispose();
                File.Delete(imagePath);
                await target.reply("已成功上传，请耐心等待审核。\n（*如长时间审核未通过则表示不符合规定，请重新上传或联系管理员）");
                Utils.SendMail("mono@desu.life", "有新的v2 info panel需要审核", $"osuid: {DBOsuInfo.osu_uid}  请及时查看\n<img src={cmd}>", true);
                Utils.SendMail("fantasyzhjk@qq.com", "有新的v2 info panel需要审核", $"osuid: {DBOsuInfo.osu_uid}  请及时查看\n<img src={cmd}>", true);
            }
            catch
            {
                await target.reply("发生了未知错误，请联系管理员。");
                return;
            }
        }
        async public static Task Osu_InfoPanelV1Panel(Target target, string cmd)
        {
            var AccInfo = Accounts.GetAccInfo(target);
            var DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { await target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

            //权限检测
            if (!await RestrictedDetect(target, DBUser.permissions!)) return;

            cmd = cmd.Trim();
            if (!Utils.IsUrl(cmd))
            {
                if (cmd.ToLower() == "reset" || cmd.ToLower() == "delete")
                {
                    //删除info image
                    try
                    {
                        File.Delete($"./work/legacy/v1_infopanel/{DBOsuInfo.osu_uid}.png");
                        await target.reply("已重置。");
                        return;
                    }
                    catch
                    {
                        await target.reply("重置时发生了错误，请联系管理员。");
                        return;
                    }
                }
                else
                {
                    await target.reply("url不正确，请重试。!set osuinfopanelv1 [url]");
                    return;
                }
            }

            //接收
            string randstr = Utils.RandomStr(50);
            var imagePath = $"./work/tmp/{randstr}.png";
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
            //从url下载
            try
            {
                await target.reply("正在处理...");
                imagePath = await cmd.DownloadFileAsync($"./work/tmp/", $"{randstr}.png");
            }
            catch (Exception ex)
            {
                await target.reply($"接收图片失败，请确保提供的链接为直链\n异常信息: '{ex.Message}");
                return;
            }
            try
            {
                var temppic = Img.Load(imagePath, out IImageFormat format).CloneAs<Rgba32>();
                //检测上传的infopanel尺寸是否正确
                if (temppic.Height != 857 && temppic.Width != 1200 && format.DefaultMimeType.Trim()[(format.DefaultMimeType.Trim().IndexOf("/") + 1)..] != "png")
                {
                    temppic.Dispose();
                    File.Delete(imagePath);
                    await target.reply("上传的图像不符合infopanel的条件，请重新上传。");
                    return;
                }
                temppic.Save($"./work/legacy/v1_infopanel/verify/{DBOsuInfo.osu_uid}.png", new PngEncoder());
                temppic.Dispose();
                File.Delete(imagePath);
                await target.reply("已成功上传，请耐心等待审核。\n（*如长时间审核未通过则表示不符合规定，请重新上传或联系管理员）");
                Utils.SendMail("mono@desu.life", "有新的v1 info panel需要审核", $"osuid: {DBOsuInfo.osu_uid}  请及时查看\n<img src={cmd}>", true);
                Utils.SendMail("fantasyzhjk@qq.com", "有新的v1 info panel需要审核", $"osuid: {DBOsuInfo.osu_uid}  请及时查看\n<img src={cmd}>", true);
            }
            catch
            {
                await target.reply("发生了未知错误，请联系管理员。");
                return;
            }
        }

        async public static Task Osu_InfoPanelV2CustomColorValue(Target target, string cmd)
        {
            var AccInfo = Accounts.GetAccInfo(target);
            var DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { await target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

            //权限检测
            if (!await RestrictedDetect(target, DBUser.permissions!)) return;

            #region Colors
            Color UsernameColor = new(),
                  ModeIconColor = new(),
                  RankColor = new(),
                  CountryRankColor = new(),

                  CountryRankDiffColor = new(),
                  CountryRankDiffIconColor = new(),

                  RankLineChartColor = new(),
                  RankLineChartTextColor = new(),
                  RankLineChartDotColor = new(),
                  RankLineChartDotStrokeColor = new(),
                  RankLineChartDashColor = new(),
                  RankLineChartDateTextColor = new(),
                  ppMainColor = new(),

                  ppDiffColor = new(),
                  ppDiffIconColor = new(),

                  ppProgressBarColorTextColor = new(),
                  ppProgressBarColor = new(),
                  ppProgressBarBackgroundColor = new(),
                  accMainColor = new(),

                  accDiffColor = new(),
                  accDiffIconColor = new(),

                  accProgressBarColorTextColor = new(),
                  accProgressBarColor = new(),
                  accProgressBarBackgroundColor = new(),
                  GradeStatisticsColor_XH = new(),
                  GradeStatisticsColor_X = new(),
                  GradeStatisticsColor_SH = new(),
                  GradeStatisticsColor_S = new(),
                  GradeStatisticsColor_A = new(),
                  Details_PlayTimeColor = new(),
                  Details_TotalHitsColor = new(),
                  Details_PlayCountColor = new(),
                  Details_RankedScoreColor = new(),

                  DetailsDiff_PlayTimeColor = new(),
                  DetailsDiff_TotalHitsColor = new(),
                  DetailsDiff_PlayCountColor = new(),
                  DetailsDiff_RankedScoreColor = new(),
                  DetailsDiff_PlayTimeIconColor = new(),
                  DetailsDiff_TotalHitsIconColor = new(),
                  DetailsDiff_PlayCountIconColor = new(),
                  DetailsDiff_RankedScoreIconColor = new(),

                  LevelTitleColor = new(),
                  LevelProgressBarColor = new(),
                  LevelProgressBarBackgroundColor = new(),
                  MainBPTitleColor = new(),
                  MainBPArtistColor = new(),
                  MainBPMapperColor = new(),
                  MainBPBIDColor = new(),
                  MainBPStarsColor = new(),
                  MainBPAccColor = new(),
                  MainBPRankColor = new(),
                  MainBPppMainColor = new(),
                  MainBPppTitleColor = new(),
                  SubBp2ndModeColor = new(),
                  SubBp2ndBPTitleColor = new(),
                  SubBp2ndBPVersionColor = new(),
                  SubBp2ndBPBIDColor = new(),
                  SubBp2ndBPStarsColor = new(),
                  SubBp2ndBPAccColor = new(),
                  SubBp2ndBPRankColor = new(),
                  SubBp2ndBPppMainColor = new(),
                  SubBp3rdModeColor = new(),
                  SubBp3rdBPTitleColor = new(),
                  SubBp3rdBPVersionColor = new(),
                  SubBp3rdBPBIDColor = new(),
                  SubBp3rdBPStarsColor = new(),
                  SubBp3rdBPAccColor = new(),
                  SubBp3rdBPRankColor = new(),
                  SubBp3rdBPppMainColor = new(),
                  SubBp4thModeColor = new(),
                  SubBp4thBPTitleColor = new(),
                  SubBp4thBPVersionColor = new(),
                  SubBp4thBPBIDColor = new(),
                  SubBp4thBPStarsColor = new(),
                  SubBp4thBPAccColor = new(),
                  SubBp4thBPRankColor = new(),
                  SubBp4thBPppMainColor = new(),
                  SubBp5thModeColor = new(),
                  SubBp5thBPTitleColor = new(),
                  SubBp5thBPVersionColor = new(),
                  SubBp5thBPBIDColor = new(),
                  SubBp5thBPStarsColor = new(),
                  SubBp5thBPAccColor = new(),
                  SubBp5thBPRankColor = new(),
                  SubBp5thBPppMainColor = new(),
                  footerColor = new(),
                  SubBpInfoSplitColor = new();

            float SideImgBrightness = 1.0f,
                  AvatarBrightness = 1.0f,
                  BadgeBrightness = 1.0f,
                  MainBPImgBrightness = 1.0f,
                  CountryFlagBrightness = 1.0f,
                  ModeCaptionBrightness = 1.0f,
                  ModIconBrightness = 1.0f,
                  ScoreModeIconBrightness = 1.0f,
                  OsuSupporterIconBrightness = 1.0f;

            float CountryFlagAlpha = 1.0f,
                  OsuSupporterIconAlpha = 1.0f,
                  BadgeAlpha = 1.0f,
                  AvatarAlpha = 1.0f,
                  ModIconAlpha = 1.0f;
            bool FixedScoreModeIconColor = false, 
                 DisplaySupporterStatus = true;
            #endregion
            string[] argstemp;
            try
            {
                argstemp = cmd.Split("\n");
                if (argstemp.Length < 103)
                    throw new ArgumentException("颜色参数缺失或错误");
            }
            catch
            {
                await target.reply($"!set osuinfopanelv2colorcustom [args]");
                return;
            }
            foreach (string arg in argstemp)
            {
                try
                {
                    switch (arg.Split(":")[0].Trim())
                    {
                        case "DetailsDiff_PlayTimeColor":
                            DetailsDiff_PlayTimeColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "DetailsDiff_PlayTimeIconColor":
                            DetailsDiff_PlayTimeIconColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "DetailsDiff_TotalHitsColor":
                            DetailsDiff_TotalHitsColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "DetailsDiff_TotalHitsIconColor":
                            DetailsDiff_TotalHitsIconColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "DetailsDiff_PlayCountColor":
                            DetailsDiff_PlayCountColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "DetailsDiff_PlayCountIconColor":
                            DetailsDiff_PlayCountIconColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "DetailsDiff_RankedScoreColor":
                            DetailsDiff_RankedScoreColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "DetailsDiff_RankedScoreIconColor":
                            DetailsDiff_RankedScoreIconColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "ppDiffColor":
                            ppDiffColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "ppDiffIconColor":
                            ppDiffIconColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "accDiffColor":
                            accDiffColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "accDiffIconColor":
                            accDiffIconColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "CountryRankDiffColor":
                            CountryRankDiffColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "CountryRankDiffIconColor":
                            CountryRankDiffIconColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "OsuSupporterIconBrightness":
                            OsuSupporterIconBrightness = float.Parse($"{arg.Split(":")[1].Trim()}");
                            break;
                        case "CountryFlagAlpha":
                            CountryFlagAlpha = float.Parse($"{arg.Split(":")[1].Trim()}");
                            break;
                        case "OsuSupporterIconAlpha":
                            OsuSupporterIconAlpha = float.Parse($"{arg.Split(":")[1].Trim()}");
                            break;
                        case "BadgeAlpha":
                            BadgeAlpha = float.Parse($"{arg.Split(":")[1].Trim()}");
                            break;
                        case "AvatarAlpha":
                            AvatarAlpha = float.Parse($"{arg.Split(":")[1].Trim()}");
                            break;
                        case "ModIconAlpha":
                            ModIconAlpha = float.Parse($"{arg.Split(":")[1].Trim()}");
                            break;
                        case "UsernameColor":
                            UsernameColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "ModeIconColor":
                            ModeIconColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "RankColor":
                            RankColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "CountryRankColor":
                            CountryRankColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "RankLineChartColor":
                            RankLineChartColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "RankLineChartTextColor":
                            RankLineChartTextColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "RankLineChartDotColor":
                            RankLineChartDotColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "RankLineChartDotStrokeColor":
                            RankLineChartDotStrokeColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "RankLineChartDashColor":
                            RankLineChartDashColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "RankLineChartDateTextColor":
                            RankLineChartDateTextColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "ppMainColor":
                            ppMainColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "ppProgressBarColorTextColor":
                            ppProgressBarColorTextColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "ppProgressBarColor":
                            ppProgressBarColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "ppProgressBarBackgroundColor":
                            ppProgressBarBackgroundColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "accMainColor":
                            accMainColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "accProgressBarColorTextColor":
                            accProgressBarColorTextColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "accProgressBarColor":
                            accProgressBarColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "accProgressBarBackgroundColor":
                            accProgressBarBackgroundColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "GradeStatisticsColor_XH":
                            GradeStatisticsColor_XH = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "GradeStatisticsColor_X":
                            GradeStatisticsColor_X = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "GradeStatisticsColor_SH":
                            GradeStatisticsColor_SH = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "GradeStatisticsColor_S":
                            GradeStatisticsColor_S = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "GradeStatisticsColor_A":
                            GradeStatisticsColor_A = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "Details_PlayTimeColor":
                            Details_PlayTimeColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "Details_TotalHitsColor":
                            Details_TotalHitsColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "Details_PlayCountColor":
                            Details_PlayCountColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "Details_RankedScoreColor":
                            Details_RankedScoreColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "LevelTitleColor":
                            LevelTitleColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "LevelProgressBarColor":
                            LevelProgressBarColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "LevelProgressBarBackgroundColor":
                            LevelProgressBarBackgroundColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "MainBPTitleColor":
                            MainBPTitleColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "MainBPArtistColor":
                            MainBPArtistColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "MainBPMapperColor":
                            MainBPMapperColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "MainBPBIDColor":
                            MainBPBIDColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "MainBPStarsColor":
                            MainBPStarsColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "MainBPAccColor":
                            MainBPAccColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "MainBPRankColor":
                            MainBPRankColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "MainBPppMainColor":
                            MainBPppMainColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "MainBPppTitleColor":
                            MainBPppTitleColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp2ndModeColor":
                            SubBp2ndModeColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp2ndBPTitleColor":
                            SubBp2ndBPTitleColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp2ndBPVersionColor":
                            SubBp2ndBPVersionColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp2ndBPBIDColor":
                            SubBp2ndBPBIDColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp2ndBPStarsColor":
                            SubBp2ndBPStarsColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp2ndBPAccColor":
                            SubBp2ndBPAccColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp2ndBPRankColor":
                            SubBp2ndBPRankColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp2ndBPppMainColor":
                            SubBp2ndBPppMainColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp3rdModeColor":
                            SubBp3rdModeColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp3rdBPTitleColor":
                            SubBp3rdBPTitleColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp3rdBPVersionColor":
                            SubBp3rdBPVersionColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp3rdBPBIDColor":
                            SubBp3rdBPBIDColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp3rdBPStarsColor":
                            SubBp3rdBPStarsColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp3rdBPAccColor":
                            SubBp3rdBPAccColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp3rdBPRankColor":
                            SubBp3rdBPRankColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp3rdBPppMainColor":
                            SubBp3rdBPppMainColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp4thModeColor":
                            SubBp4thModeColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp4thBPTitleColor":
                            SubBp4thBPTitleColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp4thBPVersionColor":
                            SubBp4thBPVersionColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp4thBPBIDColor":
                            SubBp4thBPBIDColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp4thBPStarsColor":
                            SubBp4thBPStarsColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp4thBPAccColor":
                            SubBp4thBPAccColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp4thBPRankColor":
                            SubBp4thBPRankColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp4thBPppMainColor":
                            SubBp4thBPppMainColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp5thModeColor":
                            SubBp5thModeColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp5thBPTitleColor":
                            SubBp5thBPTitleColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp5thBPVersionColor":
                            SubBp5thBPVersionColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp5thBPBIDColor":
                            SubBp5thBPBIDColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp5thBPStarsColor":
                            SubBp5thBPStarsColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp5thBPAccColor":
                            SubBp5thBPAccColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp5thBPRankColor":
                            SubBp5thBPRankColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBp5thBPppMainColor":
                            SubBp5thBPppMainColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "SubBpInfoSplitColor":
                            SubBpInfoSplitColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "footerColor":
                            footerColor = Color.ParseHex(arg.Split(":")[1].Trim());
                            break;
                        case "FixedScoreModeIconColor":
                            FixedScoreModeIconColor = bool.Parse($"{arg.Split(":")[1].Trim()}");
                            break;
                        case "DisplaySupporterStatus":
                            DisplaySupporterStatus = bool.Parse($"{arg.Split(":")[1].Trim()}");
                            break;
                        case "SideImgBrightness":
                            SideImgBrightness = float.Parse($"{arg.Split(":")[1].Trim()}");
                            break;
                        case "AvatarBrightness":
                            AvatarBrightness = float.Parse($"{arg.Split(":")[1].Trim()}");
                            break;
                        case "BadgeBrightness":
                            BadgeBrightness = float.Parse($"{arg.Split(":")[1].Trim()}");
                            break;
                        case "MainBPImgBrightness":
                            MainBPImgBrightness = float.Parse($"{arg.Split(":")[1].Trim()}");
                            break;
                        case "CountryFlagBrightness":
                            CountryFlagBrightness = float.Parse($"{arg.Split(":")[1].Trim()}");
                            break;
                        case "ModeCaptionBrightness":
                            ModeCaptionBrightness = float.Parse($"{arg.Split(":")[1].Trim()}");
                            break;
                        case "ModIconBrightness":
                            ModIconBrightness = float.Parse($"{arg.Split(":")[1].Trim()}");
                            break;
                        case "ScoreModeIconBrightness":
                            ScoreModeIconBrightness = float.Parse($"{arg.Split(":")[1].Trim()}");
                            break;
                        default:
                            break;
                    }
                }
                catch
                {
                    await target.reply($"在解析{arg}时出错了，请重新检查此参数");
                    return;
                }
            }
            //检查通过，写入数据库
            try
            {
                await Database.Client.UpdateInfoPanelV2CustomCmd(DBOsuInfo.osu_uid, cmd.Trim());
                await target.reply($"已成功设置，您或许还需要将osuinfopanelv2colormode改为0才可生效。");
                return;
            }
            catch
            {
                await target.reply($"设置失败，请稍后重试或联系管理员。");
                return;
            }
        }



    }
}
