﻿using System.Threading.RateLimiting;

namespace Edi.Translator.Configuration;

public class RateLimiterOptionFactory
{
    public static Action<FixedWindowRateLimiterOptions> GetFixedWindowRateLimiterOptions(int permitLimit, TimeSpan period)
    {
        return options =>
        {
            options.PermitLimit = permitLimit;
            options.Window = period;
            options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        };
    }
}