using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FileSearcherMVVM.Models
{
    public class FileSearchProvider
    {
        public delegate void SearchHandler(int count);

        public event SearchHandler OnFileFound;

        private FileSearchProvider() { }

        private static FileSearchProvider instance;

        public static FileSearchProvider GetInstance()
        {
            if (instance == null)
            {
                instance = new FileSearchProvider();
            }

            return instance;
        }

        public async IAsyncEnumerable<IFileItem> Search(string path, string pattern, [EnumeratorCancellation]CancellationToken cancellationToken, PauseToken pauseToken)
        {
            pattern ??= "";

            DirectoryInfo dirInfo = new DirectoryInfo(path);

            foreach (DirectoryInfo directory in dirInfo.GetDirectories())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }

                if (pauseToken.IsPaused)
                {
                    await pauseToken.WaitWhilePausedAsync();
                }

                IEnumerable<IFileItem> searchedItems = await Search(directory.FullName, pattern, cancellationToken, pauseToken).ToListAsync();

                if (searchedItems.Count() > 0)
                {
                    DirectoryItem item = new DirectoryItem
                    {
                        Name = directory.Name,
                        Path = directory.FullName,
                        Items = searchedItems
                    };

                    // Имитация задержки поиска файлов
                    await Task.Delay(1);

                    yield return item;
                }
            }

            foreach (FileInfo file in dirInfo.GetFiles().Where(n => Regex.IsMatch(n.Name, pattern)))
            {
                FileItem item = new FileItem
                {
                    Name = file.Name,
                    Path = file.FullName
                };

                OnFileFound?.Invoke(1);

                yield return item;
            }
        }
    }
}
