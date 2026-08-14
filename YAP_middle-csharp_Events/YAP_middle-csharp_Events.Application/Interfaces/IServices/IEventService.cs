using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp.Contracts.EventModels;
using YAP_middle_csharp_Events.Application.Models;
using YAP_middle_csharp_Events.Domain.Models;

namespace YAP_middle_csharp_Events.Application.Interfaces.IServices
{
    public interface IEventService
    {
        Task<EventContract?> GetEventContractByIdAsync(Guid id);
        Task<bool> ReserveSeatAsync(Guid eventId, int count = 1);
        Task<bool> ReleaseSeatAsync(Guid eventId, int count = 1);


        Task<PaginatedResult<EventModel>> FindAllAsync(string? title = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 10);
        Task<EventModel> FindByIdAsync(Guid id);
        Task<EventModel> CreateAsync(EventRequest eventRequest);
        Task<EventModel> UpdateAsync(Guid id, EventUpdateRequest eventUpdateRequest);
        Task DeleteAsync(Guid id);
    }
}
