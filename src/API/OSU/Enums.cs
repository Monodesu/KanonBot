// #pragma warning disable CS8604 // 引用类型参数可能为 null。
// #pragma warning disable CS8602 // 解引用可能出现空引用。
// #pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
#pragma warning disable CS8618 // 非null 字段未初始化
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using Serilog;
using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Converters;
using System.ComponentModel;


namespace KanonBot.API
{
    static class OSUExtensions
    {
        public static string ToScoreTypeStr(this OSU.Enums.UserScoreType type)
        {
            return Utils.GetObjectDescription(type)!;
        }
        public static string ToModeStr(this OSU.Enums.Mode mode)
        {
            return Utils.GetObjectDescription(mode)!;
        }

        public static int ToModeNum(this OSU.Enums.Mode mode)
        {
            return mode switch {
                OSU.Enums.Mode.OSU => 0,
                OSU.Enums.Mode.Taiko => 1,
                OSU.Enums.Mode.Fruits => 2,
                OSU.Enums.Mode.Mania => 3,
                _ => throw new ArgumentException("UNKNOWN MODE")
            };
        }
    }

    public partial class OSU
    {
        public class Enums
        {
            // 方法部分

            public static Mode? ParseMode(string? value)
            {
                value = value?.ToLower();    // 大写字符转小写
                return value switch {
                    "osu" => OSU.Enums.Mode.OSU,
                    "taiko" => OSU.Enums.Mode.Taiko,
                    "fruits" => OSU.Enums.Mode.Fruits,
                    "mania" => OSU.Enums.Mode.Mania,
                    _ => null
                };
            }

            public static Mode? ParseMode(int value)
            {
                return value switch {
                    0 => OSU.Enums.Mode.OSU,
                    1 => OSU.Enums.Mode.Taiko,
                    2 => OSU.Enums.Mode.Fruits,
                    3 => OSU.Enums.Mode.Mania,
                    _ => null
                };
            }

            // 枚举部分
            [DefaultValue(OSU)] // 解析失败就osu
            public enum Mode
            {
                [Description("osu")]
                OSU,
                [Description("taiko")]
                Taiko,
                [Description("fruits")]
                Fruits,
                [Description("mania")]
                Mania,
            }
            
            // 成绩类型，用作API查询
            // 共可以是 best, firsts, recent
            // 默认为best（bp查询）
            [DefaultValue(Best)] 
            public enum UserScoreType
            {
                [Description("best")]
                Best,
                [Description("firsts")]
                Firsts,
                [Description("recent")]
                Recent,
            }

            [DefaultValue(Unknown)]
            public enum Status
            {
                /// <summary>
                /// 未知，在转换错误时为此值
                /// </summary>
                [Description("")]
                Unknown,

                [Description("graveyard")]
                Graveyard,

                [Description("wip")]
                WIP,

                [Description("pending")]
                pending,

                [Description("ranked")]
                ranked,

                [Description("approved")]
                approved,

                [Description("qualified")]
                qualified,
                [Description("loved")]
                loved
            }
        }
    }
}
