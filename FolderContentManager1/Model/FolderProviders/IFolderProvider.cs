namespace ContentManager.Model.FolderProviders
{
    public interface IFolderProvider<T>
    {
        T GetFolder(string path);
    }
}
