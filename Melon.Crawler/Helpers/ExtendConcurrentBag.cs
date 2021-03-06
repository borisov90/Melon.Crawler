﻿using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Melon.Crawler.Helpers
{
    public static class ExtendConcurrentBag
    {
        public static void AddRange<T>(this ConcurrentBag<T> @this, IEnumerable<T> toAdd)
        {
            foreach (var element in toAdd)
            {
                @this.Add(element);
            }
        }
    }
}
