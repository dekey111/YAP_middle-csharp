using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Tests
{
    internal class EventFilter
    {
        public IEnumerable<EventResponse> FilterByStreet(IEnumerable<EventResponse> events, string title)
        {
            return events.Where(address =>
              address.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<EventResponse> FilterByStartDate(IEnumerable<EventResponse> events, DateTime from)
        {
            return events.Where(address => address.StartAt.Date >= from.Date);
        }

        public IEnumerable<EventResponse> FilterByEndDate(IEnumerable<EventResponse> events, DateTime to)
        {
            return events.Where(address => address.EndAt.Date <= to.Date);
        }
    }
}
