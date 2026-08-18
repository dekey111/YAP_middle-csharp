using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp_Events.Application.Helper
{
    public static class CacheKeysHelper
    {
        public static string Event(Guid id) => $"event:{id}";

        public static string TopEvents(int count = 10) => $"events:top{count}";
    }
}
