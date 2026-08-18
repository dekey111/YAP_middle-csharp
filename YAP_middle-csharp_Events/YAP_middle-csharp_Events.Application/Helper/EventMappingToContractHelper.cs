using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp.Contracts.EventModels;
using YAP_middle_csharp_Events.Domain.Models;

namespace YAP_middle_csharp_Events.Application.Helper
{
    public static class EventMappingToContractHelper
    {
        public static EventContract MapToContract(this EventModel eventModel) => new()
        {
            Id = eventModel.Id,
            Title = eventModel.Title,
            StartAt = eventModel.StartAt,
            EndAt = eventModel.EndAt,
            AvailableSeats = eventModel.AvailableSeats
        };
    }
}
