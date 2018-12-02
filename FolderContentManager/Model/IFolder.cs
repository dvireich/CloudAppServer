namespace FolderContentManager.Model
{
    public interface IFolder : IFolderContent
    {
        //<summary>
        //Since the JavaScriptSerializer throwing exception when serialize large object I decided to divide the content of folder to 
        //several physical pages on the disk. This NumOfPhysicalPages property indicates the actual number of physical pages. 
        //Combining all this physical pages data will give us all the folder content.
        //</summary>
        int NumOfPhysicalPages { get; set; }
        //<summary>
        //Gives us the next physical page on the disk we should write to. default value is: 1.
        //</summary>
        int NextPhysicalPageToWrite { get; set; }
        SortType SortType { get; set; }
        //<summary>
        //Indicate the number of folder content to show per page
        //</summary>
        int NumberOfElementPerPage { get; set; }
    }
}
