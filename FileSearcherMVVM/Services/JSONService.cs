using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace FileSearcherMVVM.Services
{
    public static class JSONService<T> where T: class
    {
        public static async Task<T> Deserialize(string path)
        {
            using FileStream fs = new FileStream(path, FileMode.Open);

            return await JsonSerializer.DeserializeAsync<T>(fs);
        }

        public static async Task Serialize(T data, string path)
        {
            using FileStream fs = new FileStream(path, FileMode.Create);

            await JsonSerializer.SerializeAsync(fs, data);
        }
    }
}
