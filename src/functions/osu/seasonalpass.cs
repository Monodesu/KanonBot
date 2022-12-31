using KanonBot.API;
using Polly.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanonBot.functions.osu
{
    public static class Seasonalpass
    {
        //查询seasonal pass放在了get.cs里
        public static async Task Update(long oid, LegacyImage.Draw.ScorePanelData score)
        {
            //只记录ranked谱面
            if (score.scoreInfo.Beatmap!.Status == OSU.Enums.Status.ranked || score.scoreInfo.Beatmap!.Status == OSU.Enums.Status.approved)
            {
                //检查成绩是否已被记录
                if (await Database.Client.SeasonalPass_Query_Score_Status(score.scoreInfo.Mode.ToStr(), score.scoreInfo.Id))
                {
                    double multiply = 0.0;
                    //pt基准值
                    double pt = 10.0;
                    //各难度区间加成
                    if (score.ppInfo.star < 1 && score.ppInfo.star > 0)
                        multiply += 1.0;
                    else if (score.ppInfo.star < 2)
                        multiply += 1.1;
                    else if (score.ppInfo.star < 3)
                        multiply += 1.2;
                    else if (score.ppInfo.star < 4)
                        multiply += 1.3;
                    else if (score.ppInfo.star < 5)
                        multiply += 1.4;
                    else if (score.ppInfo.star < 6)
                        multiply += 1.5;
                    else if (score.ppInfo.star < 7)
                        multiply += 1.6;
                    else if (score.ppInfo.star < 8)
                        multiply += 1.7;
                    else if (score.ppInfo.star < 9)
                        multiply += 1.8;
                    else if (score.ppInfo.star < 10)
                        multiply += 1.9;
                    else multiply += 2.0;
                    //acc加成
                    if (score.scoreInfo.Accuracy < 0.8)
                        multiply += 0.1;
                    else if (score.scoreInfo.Accuracy < 0.81)
                        multiply += 0.2;
                    else if (score.scoreInfo.Accuracy < 0.82)
                        multiply += 0.3;
                    else if (score.scoreInfo.Accuracy < 0.83)
                        multiply += 0.4;
                    else if (score.scoreInfo.Accuracy < 0.84)
                        multiply += 0.5;
                    else if (score.scoreInfo.Accuracy < 0.85)
                        multiply += 0.6;
                    else if (score.scoreInfo.Accuracy < 0.86)
                        multiply += 0.7;
                    else if (score.scoreInfo.Accuracy < 0.87)
                        multiply += 0.8;
                    else if (score.scoreInfo.Accuracy < 0.88)
                        multiply += 0.9;
                    else if (score.scoreInfo.Accuracy < 0.89)
                        multiply += 1.0;
                    else if (score.scoreInfo.Accuracy < 0.90)
                        multiply += 1.1;
                    else if (score.scoreInfo.Accuracy < 0.91)
                        multiply += 1.2;
                    else if (score.scoreInfo.Accuracy < 0.92)
                        multiply += 1.3;
                    else if (score.scoreInfo.Accuracy < 0.93)
                        multiply += 1.4;
                    else if (score.scoreInfo.Accuracy < 0.94)
                        multiply += 1.5;
                    else if (score.scoreInfo.Accuracy < 0.95)
                        multiply += 1.6;
                    else if (score.scoreInfo.Accuracy < 0.96)
                        multiply += 1.7;
                    else if (score.scoreInfo.Accuracy < 0.97)
                        multiply += 1.8;
                    else if (score.scoreInfo.Accuracy < 0.98)
                        multiply += 1.9;
                    else if (score.scoreInfo.Accuracy < 0.99)
                        multiply += 2.0;
                    else if (score.scoreInfo.Accuracy < 1.00)
                        multiply += 2.1;
                    else
                        multiply += 2.2;

                    //combo加成
                    if (score.scoreInfo.MaxCombo < 200)
                        multiply += 0.1;
                    else if (score.scoreInfo.MaxCombo < 400)
                        multiply += 0.2;
                    else if (score.scoreInfo.MaxCombo < 600)
                        multiply += 0.3;
                    else if (score.scoreInfo.MaxCombo < 800)
                        multiply += 0.4;
                    else if (score.scoreInfo.MaxCombo < 1000)
                        multiply += 0.5;
                    else if (score.scoreInfo.MaxCombo < 1200)
                        multiply += 0.6;
                    else if (score.scoreInfo.MaxCombo < 1400)
                        multiply += 0.7;
                    else if (score.scoreInfo.MaxCombo < 1600)
                        multiply += 0.8;
                    else if (score.scoreInfo.MaxCombo < 1800)
                        multiply += 0.9;
                    else if (score.scoreInfo.MaxCombo < 2000)
                        multiply += 1.0;
                    else if (score.scoreInfo.MaxCombo < 2200)
                        multiply += 1.1;
                    else if (score.scoreInfo.MaxCombo < 2400)
                        multiply += 1.2;
                    else if (score.scoreInfo.MaxCombo < 2600)
                        multiply += 1.3;
                    else if (score.scoreInfo.MaxCombo < 2800)
                        multiply += 1.4;
                    else if (score.scoreInfo.MaxCombo < 3000)
                        multiply += 1.5;
                    else if (score.scoreInfo.MaxCombo < 3200)
                        multiply += 1.6;
                    else if (score.scoreInfo.MaxCombo < 3400)
                        multiply += 1.7;
                    else if (score.scoreInfo.MaxCombo < 3600)
                        multiply += 1.8;
                    else if (score.scoreInfo.MaxCombo < 3800)
                        multiply += 1.9;
                    else if (score.scoreInfo.MaxCombo < 4000)
                        multiply += 2.0;
                    else multiply += 2.1;
                    //mod加成
                    foreach (var x in score.scoreInfo.Mods)
                    {
                        switch (x.ToUpper())
                        {
                            case "DT":
                                //DT加成
                                multiply += 0.8;
                                break;
                            case "NC":
                                //DT加成
                                multiply += 0.8;
                                break;
                            case "HD":
                                //HD加成
                                multiply += 0.4;
                                break;
                            case "FL":
                                //FL加成
                                multiply += 1.2;
                                break;
                            case "HR":
                                //HR加成
                                multiply += 0.4;
                                break;
                            case "EZ":
                                //EZ加成
                                multiply += 0.4;
                                break;
                            default:
                                break;
                        }
                    }
                    //计算pt
                    await Database.Client.UpdateSeasonalPass(oid, score.scoreInfo.Mode.ToStr(), (int)(multiply * pt));
                }
            }

        }
    }
}
