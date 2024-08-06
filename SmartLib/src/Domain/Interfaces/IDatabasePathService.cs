namespace SmartLib.src.Domain.Interfaces
{
    public interface IDatabasePathService
    {
        string GetDataBasePath();
        Task EnsureDatabaseFileExistsAsync(string dbFilePath);
    }

}
