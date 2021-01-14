using System.Collections.Generic;

namespace FileSearcherMVVM.Models
{
    public class DirectoryItem : FileItemBase, IDirectoryItem
    {
        public IEnumerable<IFileItem> Items { get; set; }
    }
}
