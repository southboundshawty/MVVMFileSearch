using FileSearcherMVVM.Commands;
using FileSearcherMVVM.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Input;

using Timer = System.Timers.Timer;

namespace FileSearcherMVVM.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            AppSettingsProvider = AppSettingsProvider.GetInstance();
            AppSettingsProvider.SearchPath = "";
            AppSettingsProvider.SearchPattern = "";
            FileSearchProvider = FileSearchProvider.GetInstance();

            Files = new ObservableCollection<IFileItem>();

            timer = new Timer
            {
                Interval = 1
            };

            ResetSearchInfo();

            FileSearchProvider.OnFileFound += FileSearchProvider_OnFileFound;
            timer.Elapsed += Timer1_Elapsed;
        }

        private void Timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
            SearchTime++;
        }

        private void FileSearchProvider_OnFileFound(int count)
        {
            FoundedDirectoryFiles += count;
        }

        private CancellationTokenSource cancelSearchTokenSource;
        private PauseTokenSource pauseSearchTokenSource;
        private CancellationToken tokenCancelSearch;
        private PauseToken tokenPauseSearch;

        private readonly Timer timer;

        #region SearchInfo
        private int searchTime;

        public int SearchTime
        {
            get => searchTime;
            set
            {
                searchTime = value;
                OnPropertyChanged();
            }
        }

        private string searchDirectoryName;

        public string SearchDirectoryName
        {
            get => searchDirectoryName;
            set
            {
                searchDirectoryName = value;
                OnPropertyChanged();
            }
        }

        private int totalDirectoryFiles;

        public int TotalDirectoryFiles
        {
            get => totalDirectoryFiles;
            set
            {
                totalDirectoryFiles = value;
                OnPropertyChanged();
            }
        }

        private int foundedDirectoryFiles;

        public int FoundedDirectoryFiles
        {
            get => foundedDirectoryFiles;
            set
            {
                foundedDirectoryFiles = value;
                OnPropertyChanged();
            }
        }
        #endregion

        private bool isSearchInProcess;

        public bool IsSearchInProcess
        {
            get => isSearchInProcess;
            set
            {
                isSearchInProcess = value;

                cancelSearchTokenSource = new CancellationTokenSource();
                pauseSearchTokenSource = new PauseTokenSource();
                tokenCancelSearch = cancelSearchTokenSource.Token;
                tokenPauseSearch = pauseSearchTokenSource.Token;

                OnPropertyChanged();
            }
        }

        private ObservableCollection<IFileItem> files;

        public ObservableCollection<IFileItem> Files
        {
            get => files;
            set
            {
                files = value;
                OnPropertyChanged();
            }
        }

        private AppSettingsProvider appSettingsProvider;

        public AppSettingsProvider AppSettingsProvider
        {
            get => appSettingsProvider;
            set
            {
                appSettingsProvider = value;
                OnPropertyChanged();
            }
        }

        private FileSearchProvider fileSearchProvider;

        public FileSearchProvider FileSearchProvider
        {
            get => fileSearchProvider;
            set
            {
                fileSearchProvider = value;
                OnPropertyChanged();
            }
        }

        private ICommand searchCommand;
        public ICommand SearchCommand => searchCommand ??= new RelayCommand(async obj =>
                {
                    try
                    {
                        ResetSearchInfo();

                        timer.Start();

                        AppSettingsProvider.SaveSettings();

                        IsSearchInProcess = true;

                        Files.Clear();

                        IAsyncEnumerable<IFileItem> items = fileSearchProvider.Search(appSettingsProvider.SearchPath, appSettingsProvider.SearchPattern, tokenCancelSearch, tokenPauseSearch);

                        await foreach (IFileItem item in items)
                        {
                            Files.Add(item);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        IsSearchInProcess = false;

                        timer.Stop();

                        MessageBox.Show("Поиск завершен!", "Поиск", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }, c => !IsSearchInProcess);

        private ICommand browseCommand;
        public ICommand BrowseCommand => browseCommand ??= new RelayCommand(obj =>
        {
            //FolderBrowserDialog 

            appSettingsProvider.SearchPath = @"C:\Users\steve\Desktop\Visual Studio projects";

            ResetSearchInfo();
        }, c => !IsSearchInProcess);

        private ICommand abortSearchCommand;
        public ICommand AbortSearchCommand => abortSearchCommand ??= new RelayCommand(obj =>
        {
            pauseSearchTokenSource.IsPaused = false;

            cancelSearchTokenSource.Cancel();
        }, c => IsSearchInProcess);

        private ICommand pauseSearchCommand;
        public ICommand PauseSearchCommand => pauseSearchCommand ??= new RelayCommand(obj =>
        {
            pauseSearchTokenSource.IsPaused = !pauseSearchTokenSource.IsPaused;

            if (pauseSearchTokenSource.IsPaused)
            {
                timer.Stop();
            }
            else
            {
                timer.Start();
            }

        }, c => IsSearchInProcess);

        private void ResetSearchInfo()
        {
            try
            {
                FoundedDirectoryFiles = 0;
                SearchDirectoryName = new DirectoryInfo(appSettingsProvider.SearchPath).Name;
                TotalDirectoryFiles = Directory.GetFiles(appSettingsProvider.SearchPath, "", SearchOption.AllDirectories).Length;
                SearchTime = 0;
            }
            catch { }
        }
    }
}
