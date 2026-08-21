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
        Task<PaginatedResult<EventContract>> FindAllAsync(string? title = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<EventContract>> FindTop10EventsAsync(CancellationToken cancellationToken = default);
        Task<EventContract> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Guid> CreateAsync(EventRequest eventRequest, CancellationToken cancellationToken = default);
        Task<EventUpdateRequest> UpdateAsync(Guid id, EventUpdateRequest eventUpdateRequest, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
