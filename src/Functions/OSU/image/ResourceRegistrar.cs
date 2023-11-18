using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanonBot.Image.OSU
{
    public static class ResourceRegistrar
    {
        public static FontCollection fonts = new();
        public static FontFamily Exo2SemiBold = fonts.Add("./work/fonts/Exo2/Exo2-SemiBold.ttf");
        public static FontFamily Exo2Regular = fonts.Add("./work/fonts/Exo2/Exo2-Regular.ttf");
        public static FontFamily HarmonySans = fonts.Add(
            "./work/fonts/HarmonyOS_Sans_SC/HarmonyOS_Sans_SC_Regular.ttf"
        );
        public static FontFamily TorusRegular = fonts.Add("./work/fonts/Torus-Regular.ttf");
        public static FontFamily TorusSemiBold = fonts.Add("./work/fonts/Torus-SemiBold.ttf");
        public static FontFamily avenirLTStdMedium = fonts.Add(
            "./work/fonts/AvenirLTStd-Medium.ttf"
        );

    }
}
