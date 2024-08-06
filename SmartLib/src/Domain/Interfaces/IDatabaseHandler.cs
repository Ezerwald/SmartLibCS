namespace SmartLib.src.Domain.Interfaces
{
    public interface IDatabaseHandler : IDisposable
    {
        void AddBook(string title, string author);
        List<Tuple<int, string, string>> GetAllBooks();
        void CleanDatabase();
        void CloseConnection();
    }
}
