
using FileSearcherMVVM.Services;

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace FileSearcherMVVM.Models
{
    public class AppSettingsProvider : INotifyPropertyChanged
    {
        private AppSettingsProvider() { }

        private static AppSettingsProvider instance;

        public static AppSettingsProvider GetInstance()
        {
            if (instance == null)
            {
                instance = new AppSettingsProvider();
            }

            return instance;
        }

        public void LoadSettings()
        {
            try
            {
                if (File.Exists(path))
                {
                    instance = JSONService<AppSettingsProvider>.Deserialize(path).Result;
                }
            }
            catch { }
        }

        public async void SaveSettings()
        {
            await JSONService<AppSettingsProvider>.Serialize(instance, path);
        }

        private readonly string path = Path.Combine(Environment.CurrentDirectory, "settings.json");

        private string searchPath;

        public string SearchPath
        {
            get => searchPath;
            set
            {
                searchPath = value;
                OnPropertyChanged();
            }
        }

        private string searchPattern;

        public string SearchPattern
        {
            get => searchPattern;
            set
            {
                searchPattern = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
