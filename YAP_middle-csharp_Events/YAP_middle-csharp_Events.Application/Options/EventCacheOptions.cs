using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp_Events.Application.Options
{
    public class EventCacheOptions
    {
        public const string SectionName = "EventCache";

        public TimeSpan DefaultTTL { get; set; } = TimeSpan.FromMinutes(10);
        public TimeSpan Top10EventsTTL { get; set; } = TimeSpan.FromMinutes(2);
    }
}
