-- --------------------------------------------------------
-- 主机:                           127.0.0.1
-- 服务器版本:                        10.6.15-MariaDB - mariadb.org binary distribution
-- 服务器操作系统:                      Win64
-- HeidiSQL 版本:                  12.6.0.6765
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

-- 导出  表 kanonbot_new.badge_expiration_date_rec 结构
CREATE TABLE IF NOT EXISTS `badge_expiration_date_rec` (
  `uid` int(11) DEFAULT NULL,
  `badge_id` int(11) DEFAULT NULL,
  `expire_at` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- 数据导出被取消选择。

-- 导出  表 kanonbot_new.badge_list 结构
CREATE TABLE IF NOT EXISTS `badge_list` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` text NOT NULL,
  `name_chinese` text NOT NULL,
  `description` text NOT NULL,
  `expire_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=166 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- 数据导出被取消选择。

-- 导出  表 kanonbot_new.badge_redemption_code 结构
CREATE TABLE IF NOT EXISTS `badge_redemption_code` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `badge_id` int(11) NOT NULL,
  `can_repeatedly` tinyint(1) DEFAULT 0,
  `redeem_count` int(11) NOT NULL DEFAULT 0,
  `expire_at` datetime DEFAULT NULL,
  `gen_time` datetime DEFAULT NULL,
  `redeem_time` datetime DEFAULT NULL,
  `redeem_user` longtext DEFAULT NULL,
  `code` text NOT NULL,
  `badge_expiration_day` int(11) DEFAULT 0,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=3540 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- 数据导出被取消选择。

-- 导出  表 kanonbot_new.bottle 结构
CREATE TABLE IF NOT EXISTS `bottle` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `time` datetime DEFAULT NULL,
  `platform` text DEFAULT NULL,
  `user` text DEFAULT NULL,
  `message` longtext DEFAULT NULL,
  `pickedcount` smallint(6) DEFAULT NULL,
  `haspickedup` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- 数据导出被取消选择。

-- 导出  表 kanonbot_new.chatbot 结构
CREATE TABLE IF NOT EXISTS `chatbot` (
  `uid` int(11) NOT NULL,
  `botdefine` longtext DEFAULT NULL,
  `openaikey` text DEFAULT NULL,
  `organization` text DEFAULT NULL,
  PRIMARY KEY (`uid`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- 数据导出被取消选择。

-- 导出  表 kanonbot_new.mail_verify 结构
CREATE TABLE IF NOT EXISTS `mail_verify` (
  `mailAddr` text NOT NULL,
  `verify` text NOT NULL,
  `gen_time` bigint(20) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- 数据导出被取消选择。

-- 导出  表 kanonbot_new.osu_archived_record 结构
CREATE TABLE IF NOT EXISTS `osu_archived_record` (
  `uid` int(11) NOT NULL COMMENT '此处为osu!uid，并非Kanonuid',
  `play_count` int(11) NOT NULL,
  `ranked_score` bigint(20) NOT NULL,
  `total_score` bigint(20) NOT NULL,
  `total_hit` bigint(20) NOT NULL,
  `level` int(11) NOT NULL,
  `level_percent` int(11) NOT NULL,
  `performance_point` float NOT NULL,
  `accuracy` float NOT NULL,
  `count_SSH` int(11) NOT NULL,
  `count_SS` int(11) NOT NULL,
  `count_SH` int(11) NOT NULL,
  `count_S` int(11) NOT NULL,
  `count_A` int(11) NOT NULL,
  `playtime` int(11) NOT NULL,
  `country_rank` bigint(20) NOT NULL,
  `global_rank` bigint(20) NOT NULL,
  `gamemode` text NOT NULL,
  `lastupdate` date NOT NULL,
  PRIMARY KEY (`uid`,`gamemode`(20),`lastupdate`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- 数据导出被取消选择。

-- 导出  表 kanonbot_new.osu_performancepointplus_record 结构
CREATE TABLE IF NOT EXISTS `osu_performancepointplus_record` (
  `uid` bigint(20) NOT NULL COMMENT '这里是指osu!uid，并非Kanonuid',
  `pp` float NOT NULL,
  `jump` int(11) NOT NULL,
  `flow` int(11) NOT NULL,
  `pre` int(11) NOT NULL,
  `acc` int(11) NOT NULL,
  `spd` int(11) NOT NULL,
  `sta` int(11) NOT NULL,
  PRIMARY KEY (`uid`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- 数据导出被取消选择。

-- 导出  表 kanonbot_new.osu_seasonalpass_2022_s4 结构
CREATE TABLE IF NOT EXISTS `osu_seasonalpass_2022_s4` (
  `uid` int(11) DEFAULT NULL COMMENT '此处是指osu! uid，并非kanonuid',
  `mode` text DEFAULT NULL,
  `tth` bigint(20) DEFAULT NULL,
  `inittth` bigint(20) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- 数据导出被取消选择。

-- 导出  表 kanonbot_new.osu_standard_beatmap_tech_data 结构
CREATE TABLE IF NOT EXISTS `osu_standard_beatmap_tech_data` (
  `bid` bigint(20) NOT NULL,
  `total` int(11) NOT NULL,
  `stars` double NOT NULL,
  `aim` int(11) NOT NULL,
  `speed` int(11) NOT NULL,
  `acc` int(11) NOT NULL,
  `mod` text NOT NULL,
  `pp_99acc` int(11) NOT NULL,
  `pp_98acc` int(11) NOT NULL,
  `pp_97acc` int(11) NOT NULL,
  `pp_95acc` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- 数据导出被取消选择。

-- 导出  表 kanonbot_new.seasonalpass_2023_s1 结构
CREATE TABLE IF NOT EXISTS `seasonalpass_2023_s1` (
  `osu_id` bigint(20) DEFAULT NULL,
  `mode` text DEFAULT NULL,
  `point` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- 数据导出被取消选择。

-- 导出  表 kanonbot_new.seasonalpass_2023_s1_copy1 结构
CREATE TABLE IF NOT EXISTS `seasonalpass_2023_s1_copy1` (
  `osu_id` bigint(20) DEFAULT NULL,
  `mode` text DEFAULT NULL,
  `point` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- 数据导出被取消选择。

-- 导出  表 kanonbot_new.seasonalpass_2023_s2 结构
CREATE TABLE IF NOT EXISTS `seasonalpass_2023_s2` (
  `osu_id` bigint(20) DEFAULT NULL,
  `mode` text DEFAULT NULL,
  `point` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- 数据导出被取消选择。

-- 导出  表 kanonbot_new.seasonalpass_scorerecords 结构
CREATE TABLE IF NOT EXISTS `seasonalpass_scorerecords` (
  `score_id` bigint(20) NOT NULL,
  `mode` text DEFAULT NULL,
  PRIMARY KEY (`score_id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- 数据导出被取消选择。

-- 导出  表 kanonbot_new.users 结构
CREATE TABLE IF NOT EXISTS `users` (
  `uid` int(11) NOT NULL AUTO_INCREMENT COMMENT '用户id，不对外展示',
  `email` text NOT NULL COMMENT '邮箱',
  `passwd` longtext DEFAULT NULL COMMENT '加密的密码',
  `qq_id` bigint(20) DEFAULT NULL COMMENT 'qq号',
  `qq_guild_uid` text DEFAULT NULL COMMENT 'qq频道uid',
  `kook_uid` text DEFAULT NULL COMMENT '开黑啦uid',
  `discord_uid` text DEFAULT NULL COMMENT 'discord uid',
  `permissions` text DEFAULT NULL COMMENT '权限列表',
  `last_login_ip` text DEFAULT NULL COMMENT '最后登录的ip',
  `last_login_time` datetime DEFAULT NULL COMMENT '最后登录的时间',
  `status` int(11) DEFAULT 0 COMMENT '用户状态，-1被禁用，0正常，1在线',
  `displayed_badge_ids` longtext DEFAULT NULL COMMENT '要显示的badgeids，多个badge用,分割',
  `owned_badge_ids` longtext DEFAULT NULL COMMENT '拥有的badgeids，多个badge用,分割',
  PRIMARY KEY (`uid`,`email`(512)) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=8358 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- 数据导出被取消选择。

-- 导出  表 kanonbot_new.users_osu 结构
CREATE TABLE IF NOT EXISTS `users_osu` (
  `uid` int(11) NOT NULL COMMENT 'uid对应表users中的uid',
  `osu_uid` bigint(20) NOT NULL COMMENT 'osu! 用户id',
  `osu_mode` text NOT NULL COMMENT 'osu!默认查询的模式',
  `customInfoEngineVer` int(11) NOT NULL DEFAULT 2,
  `InfoPanelV2_Mode` int(11) NOT NULL DEFAULT 1 COMMENT '1=light 2=dark',
  `InfoPanelV2_CustomMode` longtext DEFAULT NULL,
  PRIMARY KEY (`uid`,`osu_uid`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci ROW_FORMAT=DYNAMIC;

-- 数据导出被取消选择。

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
