namespace YAP_middle_csharp.Interfaces
{
    public interface IValidator<in T>
    {
        bool IsValid(T item);
        IEnumerable<string> GetErrors(T item);

    }
}
