using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using Img = SixLabors.ImageSharp.Image;
using SixLabors.Fonts;
using SixLabors.ImageSharp.ColorSpaces;
using Flurl;
using Flurl.Http;
using KanonBot.functions.osu.rosupp;
using KanonBot.LegacyImage;
using static KanonBot.LegacyImage.Draw;
using KanonBot.Image;
using SqlSugar;
using System.IO;
using SixLabors.ImageSharp.Formats.Png;
using ResizeOptions = SixLabors.ImageSharp.Processing.ResizeOptions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KanonBot.functions.osubot
{
    public class Set
    {
        public static async Task Execute(Target target, string cmd)
        {
            string rootCmd, childCmd = "";
            try
            {
                rootCmd = cmd[..cmd.IndexOf(" ")].Trim();
                childCmd = cmd[(cmd.IndexOf(" ") + 1)..].Trim();
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
                default:
                    target.reply("!set osumode/osuinfopanelversion/osuinfopanelv1img(not enabled)/osuinfopanelv2img/osuinfopanelv1panel(not enabled)/osuinfopanelv2panel(not enabled)/osuinfopanelv2colormode");
                    return;
            }
        }
        async public static Task Osu_InfoPanelVersion(Target target, string cmd)
        {
            var AccInfo = Accounts.GetAccInfo(target);
            var DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

            cmd = cmd.ToLower().Trim();
            if (int.TryParse(cmd, out _))
            {
                try
                {
                    var t = int.Parse(cmd);
                    if (t < 1 || t > 2)
                    {
                        target.reply("版本号不正确。(1、2)");
                        return;
                    }
                    await Database.Client.SetOsuInfoPanelVersion(DBOsuInfo.osu_uid, t);
                    target.reply("成功设置infopanel版本。");
                }
                catch
                {
                    target.reply("发生了错误，无法设置infopanel版本，请联系管理员。");
                }
            }
            else
            {
                target.reply("版本号不正确。(1、2)");
            }
        }
        async public static Task Osu_InfoPanelV2ColorMode(Target target, string cmd)
        {
            var AccInfo = Accounts.GetAccInfo(target);
            var DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

            cmd = cmd.ToLower().Trim();
            if (int.TryParse(cmd, out _))
            {
                try
                {
                    var t = int.Parse(cmd);
                    if (t < 0 || t > 1)
                    {
                        target.reply("配色方案号码不正确。(0=light、1=dark)");
                        return;
                    }
                    await Database.Client.SetOsuInfoPanelV2ColorMode(DBOsuInfo.osu_uid, t);
                    target.reply("成功设置配色方案。");
                }
                catch
                {
                    target.reply("发生了错误，无法设置配色方案，请联系管理员。");
                }
            }
            else
            {
                target.reply("配色方案号码不正确。(0=light、1=dark)");
            }
        }
        async public static Task Osu_mode(Target target, string cmd)
        {
            var AccInfo = Accounts.GetAccInfo(target);
            var DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            // { target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
            { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

            cmd = cmd.ToLower().Trim();

            var mode = OSU.Enums.ParseMode(cmd);
            if (mode == null)
            {
                target.reply("提供的模式不正确，请重新确认 (osu/taiko/fruits/mania)");
                return;
            }
            else
            {
                try
                {
                    await Database.Client.SetOsuUserMode(DBOsuInfo.osu_uid, mode.Value);
                    target.reply("成功设置模式为 " + cmd);
                }
                catch
                {
                    target.reply("发生了错误，无法设置osu模式，请联系管理员。");
                }
            }
        }

        async public static Task Osu_InfoPanelV2IMG(Target target, string cmd)
        {
            var AccInfo = Accounts.GetAccInfo(target);
            var DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

            cmd = cmd.Trim();
            if (!Utils.IsUrl(cmd))
            {
                if (cmd.ToLower() == "reset" || cmd.ToLower() == "delete")
                {
                    //删除info image
                    try
                    {
                        File.Delete($"./work/panelv2/user_customimg/{DBOsuInfo.osu_uid}.png");
                        target.reply("已重置。");
                        return;
                    }
                    catch
                    {
                        target.reply("重置时发生了错误，请联系管理员。");
                        return;
                    }
                }
                else
                {
                    target.reply("url不正确，请重试。!set osuinfopanelv2img [url]");
                    return;
                }
            }

            //接收
            string randstr = Utils.RandomStr(50);
            Img image;
            var imagePath = $"./work/panelv2/user_customimg/verify/{randstr}.png";
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
            //从url下载
            try
            {
                target.reply("正在处理...");
                imagePath = await cmd.DownloadFileAsync($"./work/panelv2/user_customimg/verify/", $"{randstr}.png");
            }
            catch (Exception ex)
            {
                target.reply($"接收图片失败，请确保提供的链接为直链\n异常信息: '{ex.Message}");
                return;
            }
            try
            {
                image = new Image<Rgba32>(1417, 2518);
                var temppic = Img.Load(imagePath).CloneAs<Rgba32>();
                temppic.Mutate(x => x.Resize(new ResizeOptions() { Size = new Size(0, 2518), Mode = ResizeMode.Max }));
                image.Mutate(x => x.DrawImage(temppic, new Point(0, 0), 1));
                image.Save($"./work/panelv2/user_customimg/verify/{DBOsuInfo.osu_uid}.png", new PngEncoder());
                temppic.Dispose();
                File.Delete(imagePath);
                target.reply("已成功上传，请耐心等待审核。\r\n（*如长时间审核未通过则表示不符合规定，请重新上传或联系管理员）");
                Utils.SendMail("mono@desu.life", "有新的v2 info image需要审核", $"osuid: {DBOsuInfo.osu_uid}  请及时查看\r\n<img src={cmd}>", true);
                Utils.SendMail("fantasyzhjk@qq.com", "有新的v2 info image需要审核", $"osuid: {DBOsuInfo.osu_uid}  请及时查看\r\n<img src={cmd}>", true);
            }
            catch
            {
                target.reply("发生了未知错误，请联系管理员。");
                return;
            }
        }

        async public static Task Osu_InfoPanelV1IMG(Target target, string cmd)
        {
            var AccInfo = Accounts.GetAccInfo(target);
            var DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

            cmd = cmd.Trim();
            if (!Utils.IsUrl(cmd))
            {
                if (cmd.ToLower() == "reset" || cmd.ToLower() == "delete")
                {
                    //删除info image
                    try
                    {
                        File.Delete($"./work/legacy/v1_cover/custom/{DBOsuInfo.osu_uid}.png");
                        target.reply("已重置。");
                        return;
                    }
                    catch
                    {
                        target.reply("重置时发生了错误，请联系管理员。");
                        return;
                    }
                }
                else
                {
                    target.reply("url不正确，请重试。!set osuinfopanelv1img [url]");
                    return;
                }
            }

            //接收
            string randstr = Utils.RandomStr(50);
            Img image;
            var imagePath = $"./work/legacy/v1_cover/custom/verify/{randstr}.png";
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);

            }
            //从url下载
            try
            {
                target.reply("正在处理...");
                imagePath = await cmd.DownloadFileAsync($"./work/legacy/v1_cover/custom/verify/", $"{randstr}.png");
            }
            catch (Exception ex)
            {
                target.reply($"接收图片失败，请确保提供的链接为直链\n异常信息: '{ex.Message}");
                return;
            }
            try
            {
                image = new Image<Rgba32>(1200, 350);
                var temppic = Img.Load(imagePath).CloneAs<Rgba32>();
                temppic.Mutate(x => x.Resize(new ResizeOptions() { Size = new Size(1200, 0), Mode = ResizeMode.Max }));
                image.Mutate(x => x.DrawImage(temppic, new Point(0, 0), 1));
                image.Save($"./work/legacy/v1_cover/custom/verify/{DBOsuInfo.osu_uid}.png", new PngEncoder());
                temppic.Dispose();
                File.Delete(imagePath);
                target.reply("已成功上传，请耐心等待审核。\r\n（*如长时间审核未通过则表示不符合规定，请重新上传或联系管理员）");
                Utils.SendMail("mono@desu.life", "有新的v1 info image需要审核", $"osuid: {DBOsuInfo.osu_uid}  请及时查看\r\n<img src={cmd}>", true);
                Utils.SendMail("fantasyzhjk@qq.com", "有新的v1 info image需要审核", $"osuid: {DBOsuInfo.osu_uid}  请及时查看\r\n<img src={cmd}>", true);
            }
            catch
            {
                target.reply("发生了未知错误，请联系管理员。");
                return;
            }
        }



    }
}
