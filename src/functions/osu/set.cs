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
            string rootCmd,
                childCmd = "";
            try
            {
                var tmp = cmd.SplitOnFirstOccurence(" ");
                rootCmd = tmp[0].Trim();
                childCmd = tmp[1].Trim();
            }
            catch
            {
                rootCmd = cmd;
            }
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
            {
                await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。");
                return;
            }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            {
                await target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。");
                return;
            }

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
            {
                await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。");
                return;
            }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            {
                await target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。");
                return;
            }

            cmd = cmd.ToLower().Trim();
            if (int.TryParse(cmd, out _))
            {
                var t = int.Parse(cmd);
                if (t < 0 || t > 2)
                {
                    await target.reply("配色方案号码不正确。(0=custom、1=light、2=dark)");
                    return;
                }
                if (t == 0)
                    if (string.IsNullOrEmpty(DBOsuInfo.InfoPanelV2_CustomMode))
                    {
                        await target.reply("请先设置自定义配色方案后再将此配置项更改为custom。");
                        return;
                    }
                await Database.Client.SetOsuInfoPanelV2ColorMode(DBOsuInfo.osu_uid, t);
                await target.reply("成功设置配色方案。");
            }
            else
            {
                await target.reply("配色方案号码不正确。(0=custom、1=light、2=dark)");
            }
        }

        async public static Task Osu_mode(Target target, string cmd)
        {
            var AccInfo = Accounts.GetAccInfo(target);
            var DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            // { await target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
            {
                await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。");
                return;
            }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            {
                await target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。");
                return;
            }

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
            {
                await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。");
                return;
            }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            {
                await target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。");
                return;
            }

            //权限检测
            if (!await RestrictedDetect(target, DBUser.permissions!))
                return;

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
                temppic.Mutate(
                    x =>
                        x.Resize(
                            new ResizeOptions() { Size = new Size(0, 2456), Mode = ResizeMode.Max }
                        )
                );
                if (temppic.Width < 1382)
                {
                    temppic.Dispose();
                    File.Delete(imagePath);
                    await target.reply("图像长度太小了，自己裁剪一下再上传吧~（9:16，推荐1382x2456)");
                    return;
                }
                image.Mutate(x => x.DrawImage(temppic, new Point(0, 0), 1));
                image.Save(
                    $"./work/panelv2/user_customimg/verify/{DBOsuInfo.osu_uid}.png",
                    new PngEncoder()
                );
                temppic.Dispose();
                File.Delete(imagePath);
                await target.reply("已成功上传，请耐心等待审核。\n（*如长时间审核未通过则表示不符合规定，请重新上传或联系管理员）");
                Utils.SendMail(
                    "mono@desu.life",
                    "有新的v2 info image需要审核",
                    $"osuid: {DBOsuInfo.osu_uid}  请及时查看\n<img src={cmd}>",
                    true
                );
                Utils.SendMail(
                    "fantasyzhjk@qq.com",
                    "有新的v2 info image需要审核",
                    $"osuid: {DBOsuInfo.osu_uid}  请及时查看\n<img src={cmd}>",
                    true
                );
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
            {
                await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。");
                return;
            }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            {
                await target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。");
                return;
            }

            //权限检测
            if (!await RestrictedDetect(target, DBUser.permissions!))
                return;

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
                temppic.Mutate(
                    x =>
                        x.Resize(
                            new ResizeOptions() { Size = new Size(1200, 0), Mode = ResizeMode.Max }
                        )
                );
                if (temppic.Height < 350)
                {
                    temppic.Dispose();
                    File.Delete(imagePath);
                    await target.reply("图像长度太小了，自己裁剪一下再上传吧~（1200x350)");
                    return;
                }
                image.Mutate(x => x.DrawImage(temppic, new Point(0, 0), 1));
                image.Save(
                    $"./work/legacy/v1_cover/custom/verify/{DBOsuInfo.osu_uid}.png",
                    new PngEncoder()
                );
                temppic.Dispose();
                File.Delete(imagePath);
                await target.reply("已成功上传，请耐心等待审核。\n（*如长时间审核未通过则表示不符合规定，请重新上传或联系管理员）");
                Utils.SendMail(
                    "mono@desu.life",
                    "有新的v1 info image需要审核",
                    $"osuid: {DBOsuInfo.osu_uid}  请及时查看\n<img src={cmd}>",
                    true
                );
                Utils.SendMail(
                    "fantasyzhjk@qq.com",
                    "有新的v1 info image需要审核",
                    $"osuid: {DBOsuInfo.osu_uid}  请及时查看\n<img src={cmd}>",
                    true
                );
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
            {
                await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。");
                return;
            }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            {
                await target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。");
                return;
            }

            //权限检测
            if (!await RestrictedDetect(target, DBUser.permissions!))
                return;

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
                if (
                    format.DefaultMimeType.Trim()[
                        (format.DefaultMimeType.Trim().IndexOf("/") + 1)..
                    ] != "png"
                )
                {
                    errormsg +=
                        $"图像编码为{format.DefaultMimeType.Trim()[(format.DefaultMimeType.Trim().IndexOf("/") + 1)..]}，要求为png\n";
                    isok = false;
                }
                //检测开孔
                if (isok)
                    temppic.ProcessPixelRows(x =>
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
                temppic.Save(
                    $"./work/panelv2/user_infopanel/verify/{DBOsuInfo.osu_uid}.png",
                    new PngEncoder()
                );
                temppic.Dispose();
                File.Delete(imagePath);
                await target.reply("已成功上传，请耐心等待审核。\n（*如长时间审核未通过则表示不符合规定，请重新上传或联系管理员）");
                Utils.SendMail(
                    "mono@desu.life",
                    "有新的v2 info panel需要审核",
                    $"osuid: {DBOsuInfo.osu_uid}  请及时查看\n<img src={cmd}>",
                    true
                );
                Utils.SendMail(
                    "fantasyzhjk@qq.com",
                    "有新的v2 info panel需要审核",
                    $"osuid: {DBOsuInfo.osu_uid}  请及时查看\n<img src={cmd}>",
                    true
                );
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
            {
                await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。");
                return;
            }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            {
                await target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。");
                return;
            }

            //权限检测
            if (!await RestrictedDetect(target, DBUser.permissions!))
                return;

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
                if (
                    temppic.Height != 857
                    && temppic.Width != 1200
                    && format.DefaultMimeType.Trim()[
                        (format.DefaultMimeType.Trim().IndexOf("/") + 1)..
                    ] != "png"
                )
                {
                    temppic.Dispose();
                    File.Delete(imagePath);
                    await target.reply("上传的图像不符合infopanel的条件，请重新上传。");
                    return;
                }
                temppic.Save(
                    $"./work/legacy/v1_infopanel/verify/{DBOsuInfo.osu_uid}.png",
                    new PngEncoder()
                );
                temppic.Dispose();
                File.Delete(imagePath);
                await target.reply("已成功上传，请耐心等待审核。\n（*如长时间审核未通过则表示不符合规定，请重新上传或联系管理员）");
                Utils.SendMail(
                    "mono@desu.life",
                    "有新的v1 info panel需要审核",
                    $"osuid: {DBOsuInfo.osu_uid}  请及时查看\n<img src={cmd}>",
                    true
                );
                Utils.SendMail(
                    "fantasyzhjk@qq.com",
                    "有新的v1 info panel需要审核",
                    $"osuid: {DBOsuInfo.osu_uid}  请及时查看\n<img src={cmd}>",
                    true
                );
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
            {
                await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。");
                return;
            }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            {
                await target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。");
                return;
            }

            //权限检测
            if (!await RestrictedDetect(target, DBUser.permissions!))
                return;

            var tmp = cmd.Trim();
            if (string.IsNullOrEmpty(tmp))
            {
                await target.reply(
                    "请输入正确的配置。\nset osuinfopanelv2color [配置]\n具体格式可以在 https://info.desu.life/?page_id=407 查询到。"
                );
                return;
            }
            if (Try(() => image.OsuInfoPanelV2.InfoCustom.ParseColors(tmp, None)).IsFail())
            {
                await target.reply("配置不正确，请检查后重试。\n!set osuinfopanelv2customcolorvalue [配置]");
                return;
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
