namespace YAP_middle_csharp.Domain.Models
{
    /// <summary>
    /// Базовая модель статусов бронирования из БД
    /// </summary>
    public enum BookingStatusEnum
    {
        Pending,  
        Confirmed, 
        Rejected,
        Cancelled
    }
}
