namespace Project.Core.Interfaces
{
    public interface ISoftDelete
    {
        bool IsDelete { get; set; }
        bool IsSoftDelete { get; set; }
    }
}
