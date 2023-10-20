using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanonBot
{
    public static class RetryHelper
    {
        public static async Task<T> RetryOnExceptionAsync<T>(int times, TimeSpan delay, Func<Task<T>> operation)
        {
            if (times <= 0)
                throw new ArgumentOutOfRangeException(nameof(times), "Retry times must be greater than 0!");

            for (int i = 0; ; i++)
            {
                try
                {
                    return await operation();
                }
                catch
                {
                    if (i == times - 1)
                        throw;
                    await Task.Delay(delay);
                }
            }
        }
    }
}
