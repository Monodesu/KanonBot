using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanonBot.Account
{
    public static class Enums
    {
        [DefaultValue(Failed)]
        public enum Operation
        {
            [Description("")]
            Failed,

            [Description("CreateAccount")]
            CreateAccount,

            [Description("BindPlatform")]
            BindPlatform,

            [Description("AppendMail")]
            AppendMail,
        }
    }
}
