using System.Collections.Generic;

namespace FileSearcherMVVM.Models
{
    public interface IDirectoryItem
    {
        public IEnumerable<IFileItem> Items { get; set; }
    }
}
