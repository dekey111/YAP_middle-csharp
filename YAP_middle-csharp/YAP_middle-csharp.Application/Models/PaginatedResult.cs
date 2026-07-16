namespace YAP_middle_csharp.Application.Models
{
    /// <summary>
    /// Кастомный класс для возврата списка данных с пагинацией страниц и элементов
    /// </summary>
    /// <typeparam name="T">Список сущностей</typeparam>
    public class PaginatedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
