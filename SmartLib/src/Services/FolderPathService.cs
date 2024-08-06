using SmartLib.src.Domain.Interfaces;

public class FolderPathService : IFolderPathService
{
    public string GetFolderPathFromInput()
    {
        Console.WriteLine("Please specify the path to your books folder:");
        return Console.ReadLine();
    }
}