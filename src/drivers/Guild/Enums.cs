using Newtonsoft.Json;
using System.ComponentModel;
using KanonBot.Message;
using KanonBot.Serializer;
using Newtonsoft.Json.Linq;


namespace KanonBot.Drivers;
public partial class Guild
{
    public class Enums
    {
        /// <summary>
        /// 操作码
        /// </summary>
        public enum OperationCode
        {
            Dispatch,
            Heartbeat,
            Identify,
            Resume,
            Reconnect,
            InvalidSession,
            Hello,
            HeartbeatACK,
            HTTPCallbackACK,
        }

        /// <summary>
        /// 事件类型
        /// </summary>
        [DefaultValue(Unknown)]
        public enum EventType
        {
            /// <summary>
            /// 未知，在转换错误时为此值
            /// </summary>
            [Description("")]
            Unknown,

            // GUILDS (1 << 0)

            /// <summary>
            /// 当机器人加入新guild时
            /// </summary>
            [Description("GUILD_CREATE")]
            GuildCreate,
            /// <summary>
            /// 当guild资料发生变更时
            /// </summary>
            [Description("GUILD_UPDATE")]
            GuildUpdate,
            /// <summary>
            /// 当机器人退出guild时
            /// </summary>
            [Description("GUILD_DELETE")]
            GuildDelete,
            /// <summary>
            /// 当channel被创建时
            /// </summary>
            [Description("CHANNEL_CREATE")]
            ChannelCreate,
            /// <summary>
            /// 当channel被更新时
            /// </summary>
            [Description("CHANNEL_UPDATE")]
            ChannelUpdate,
            /// <summary>
            /// 当channel被删除时
            /// </summary>
            [Description("CHANNEL_DELETE")]
            ChannelDelete,

            // GUILD_MEMBERS (1 << 1)

            /// <summary>
            /// 当成员加入时
            /// </summary>
            [Description("GUILD_MEMBER_ADD")]
            GuildMemberAdd,
            /// <summary>
            /// 当成员资料发生变更时
            /// </summary>
            [Description("GUILD_MEMBER_UPDATE")]
            GuildMemberUpdate,
            /// <summary>
            /// 当成员被移除时
            /// </summary>
            [Description("GUILD_MEMBER_REMOVE")]
            GuildMemberRemove,

            // GUILD_MESSAGES (1 << 9) 消息事件，仅 *私域* 机器人能够设置此 intents。

            /// <summary>
            /// 发送消息事件，代表频道内的全部消息，而不只是 at 机器人的消息。内容与 AT_MESSAGE_CREATE 相同
            /// </summary>
            [Description("MESSAGE_CREATE")]
            MessageCreate,
            /// <summary>
            /// 删除（撤回）消息事件
            /// </summary>
            [Description("MESSAGE_DELETE")]
            MessageDelete,

            // GUILD_MESSAGE_REACTIONS (1 << 10)

            /// <summary>
            /// 为消息添加表情表态
            /// </summary>
            [Description("MESSAGE_REACTION_ADD")]
            MessageReactionAdd,
            /// <summary>
            /// 删除消息表情表态
            /// </summary>
            [Description("MESSAGE_REACTION_REMOVE")]
            MessageReactionRemove,

            // DIRECT_MESSAGE (1 << 12)

            /// <summary>
            /// 当收到用户发给机器人的私信消息时
            /// </summary>
            [Description("DIRECT_MESSAGE_CREATE")]
            DirectMessageCreate,
            /// <summary>
            /// 删除（撤回）消息事件
            /// </summary>
            [Description("DIRECT_MESSAGE_DELETE")]
            DirectMessageDelete,

            // INTERACTION (1 << 26)

            /// <summary>
            /// 互动事件创建时
            /// </summary>
            [Description("INTERACTION_CREATE")]
            InteractionCreate,

            // MESSAGE_AUDIT (1 << 27)

            /// <summary>
            /// 消息审核通过
            /// </summary>
            [Description("MESSAGE_AUDIT_PASS")]
            MessageAuditPass,
            /// <summary>
            /// 消息审核不通过
            /// </summary>
            [Description("MESSAGE_AUDIT_REJECT")]
            MessageAuditReject,

            // FORUMS_EVENT (1 << 28) 论坛事件，仅 *私域* 机器人能够设置此 intents。

            /// <summary>
            /// 当用户创建主题时
            /// </summary>
            [Description("FORUM_THREAD_CREATE")]
            ForumThreadCreate,
            /// <summary>
            /// 当用户更新主题时
            /// </summary>
            [Description("FORUM_THREAD_UPDATE")]
            ForumThreadUpdate,
            /// <summary>
            /// 当用户删除主题时
            /// </summary>
            [Description("FORUM_THREAD_DELETE")]
            ForumThreadDelete,
            /// <summary>
            /// 当用户创建帖子时
            /// </summary>
            [Description("FORUM_POST_CREATE")]
            ForumPostCreate,
            /// <summary>
            /// 当用户删除帖子时
            /// </summary>
            [Description("FORUM_POST_DELETE")]
            ForumPostDelete,
            /// <summary>
            /// 当用户回复评论时
            /// </summary>
            [Description("FORUM_REPLY_CREATE")]
            ForumReplyCreate,
            /// <summary>
            /// 当用户删除评论时
            /// </summary>
            [Description("FORUM_REPLY_DELETE")]
            ForumReplyDelete,
            /// <summary>
            /// 当用户发表审核通过时
            /// </summary>
            [Description("FORUM_PUBLISH_AUDIT_RESULT")]
            ForumPublishAuditResult,

            // AUDIO_ACTION (1 << 29)

            /// <summary>
            /// 音频开始播放时
            /// </summary>
            [Description("AUDIO_START")]
            AudioStart,
            /// <summary>
            /// 音频播放完成时
            /// </summary>
            [Description("AUDIO_FINISH")]
            AudioFinish,
            /// <summary>
            /// 上麦时
            /// </summary>
            [Description("AUDIO_ON_MIC")]
            AudioOnMic,
            /// <summary>
            /// 下麦时
            /// </summary>
            [Description("AUDIO_OFF_MIC")]
            AudioOffMic,

            // PUBLIC_GUILD_MESSAGES (1 << 30) 消息事件，此为公域的消息事件

            /// <summary>
            /// 当收到@机器人的消息时
            /// </summary>
            [Description("AT_MESSAGE_CREATE")]
            AtMessageCreate,
            /// <summary>
            /// 当频道的消息被删除时
            /// </summary>
            [Description("PUBLIC_MESSAGE_DELETE")]
            PublicMessageDelete,




        }

    }
}