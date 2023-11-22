
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanonBot.Functions.OSU;

namespace KanonBot.API.OSU
{
    public static class DataStructure
    {
        public class UserPanelData
        {
            public OSU.Models.User? userInfo;
            public OSU.Models.User? prevUserInfo;
            public OSU.Models.PPlusData.UserData? pplusInfo;
            public string? customPanel;
            public int daysBefore = 0;
            public List<int> badgeId = new();
            public CustomMode customMode = CustomMode.Dark; //0=custom 1=light 2=dark
            public string? ColorConfigRaw;

            public enum CustomMode
            {
                Custom = 0,
                Light = 1,
                Dark = 2
            }
        }

        public class ScorePanelData
        {
            public PerformanceCalculator.PPInfo ppInfo;
            public OSU.Models.Score? scoreInfo;
        }

        public class PPVSPanelData
        {
            public string? u1Name;
            public string? u2Name;
            public OSU.Models.PPlusData.UserData? u1;
            public OSU.Models.PPlusData.UserData? u2;
        }
    }
}
