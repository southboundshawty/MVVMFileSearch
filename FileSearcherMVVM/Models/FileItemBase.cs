namespace FileSearcherMVVM.Models
{
    public class FileItemBase : IFileItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
