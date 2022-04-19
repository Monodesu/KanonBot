using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KanonBot;
public class KanonError : Exception
{
    public KanonError(string message) : base(message)
    {
        
    }
}