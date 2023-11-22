using System.ComponentModel;

namespace KanonBot.Drivers;

public partial class Guild
{
    public class Enums
    {
        public enum DefaultRole
        {
            // 全体成员
            Member = 1,

            // 管理员
            GuildAdmin = 2,

            // 群主/创建者
            Owner = 3,

            // 子频道管理员
            ChannelAdmin = 4,
        }

        /// <summary>
        /// 操作码
        /// 0	Dispatch	Receive	服务端进行消息推送
        /// 1	Heartbeat	Send/Receive	客户端或服务端发送心跳
        /// 2	Identify	Send	客户端发送鉴权
        /// 6	Resume	Send	客户端恢复连接
        /// 7	Reconnect	Receive	服务端通知客户端重新连接
        /// 9	Invalid Session	Receive	当identify或resume的时候，如果参数有错，服务端会返回该消息
        /// 10	Hello	Receive	当客户端与网关建立ws连接之后，网关下发的第一条消息
        /// 11	Heartbeat ACK	Receive/Reply	当发送心跳成功之后，就会收到该消息
        /// 12	HTTP Callback ACK	Reply	仅用于 http 回调模式的回包，代表机器人收到了平台推送的数据
        /// </summary>
        public enum OperationCode
        {
            Dispatch = 0,
            Heartbeat = 1,
            Identify = 2,
            Resume = 6,
            Reconnect = 7,
            InvalidSession = 9,
            Hello = 10,
            HeartbeatACK = 11,
            HttpCallbackAck = 12,
        }

        /// <summary>
        /// websocket intent 声明
        /// </summary>
        public enum Intent
        {
            // Guilds 包含
            // - GUILD_CREATE
            // - GUILD_UPDATE
            // - GUILD_DELETE
            // - GUILD_ROLE_CREATE
            // - GUILD_ROLE_UPDATE
            // - GUILD_ROLE_DELETE
            // - CHANNEL_CREATE
            // - CHANNEL_UPDATE
            // - CHANNEL_DELETE
            // - CHANNEL_PINS_UPDATE
            Guilds = 1 << 0,

            // GuildMembers 包含
            // - GUILD_MEMBER_ADD
            // - GUILD_MEMBER_UPDATE
            // - GUILD_MEMBER_REMOVE
            GuildMembers = 1 << 1,

            GuildBans = 1 << 2,
            GuildEmojis = 1 << 3,
            GuildIntegrations = 1 << 4,
            GuildWebhooks = 1 << 5,
            GuildInvites = 1 << 6,
            GuildVoiceStates = 1 << 7,
            GuildPresences = 1 << 8,
            GuildMessages = 1 << 9,

            // GuildMessageReactions 包含
            // - MESSAGE_REACTION_ADD
            // - MESSAGE_REACTION_REMOVE
            GuildMessageReactions = 1 << 10,

            GuildMessageTyping = 1 << 11,
            DirectMessages = 1 << 12,
            DirectMessageReactions = 1 << 13,
            DirectMessageTyping = 1 << 14,

            Interaction = 1 << 26, // 互动事件

            Audit = 1 << 27, // 审核事件

            // Forum 论坛事件
            //  - THREAD_CREATE     // 当用户创建主题时
            //  - THREAD_UPDATE     // 当用户更新主题时
            //  - THREAD_DELETE     // 当用户删除主题时
            //  - POST_CREATE       // 当用户创建帖子时
            //  - POST_DELETE       // 当用户删除帖子时
            //  - REPLY_CREATE      // 当用户回复评论时
            //  - REPLY_DELETE      // 当用户回复评论时
            //  - FORUM_PUBLISH_AUDIT_RESULT      // 当用户发表审核通过时
            Forum = 1 << 28, // 论坛事件

            // Audio
            //  - AUDIO_START           // 音频开始播放时
            //  - AUDIO_FINISH          // 音频播放结束时
            Audio = 1 << 29, // 音频机器人事件
            GuildAtMessage = 1 << 30, // 只接收@消息事件

            None = 0,
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

            /// <summary>
            /// 鉴权成功
            /// </summary>
            [Description("READY")]
            Ready,

            /// <summary>
            /// 恢复连接成功
            /// </summary>
            [Description("RESUMED")]
            Resumed,

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
