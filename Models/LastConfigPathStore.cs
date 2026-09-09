using System;
using System.IO;

namespace conmaker.Models
{
    /// <summary>
    /// Хранит путь к последнему открытому конфигу в %AppData%, чтобы при следующем
    /// запуске приложение само его подхватило (как это делал старый Form1()).
    /// </summary>
    public static class LastConfigPathStore
    {
        private static string GetStoreFilePath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string myFolder = Path.Combine(appData, "HalfLifeConMaker");

            if (!Directory.Exists(myFolder))
                Directory.CreateDirectory(myFolder);

            return Path.Combine(myFolder, "last_cfg.txt");
        }

        public static void Save(string configPath)
        {
            File.WriteAllText(GetStoreFilePath(), configPath ?? "");
        }

        public static void Clear() => Save("");

        // Возвращает путь только если он реально существует на диске — как в старом коде,
        // где после чтения из last_cfg.txt дополнительно проверялось File.Exists(lastPath).
        public static string? TryLoad()
        {
            string storeFile = GetStoreFilePath();
            if (!File.Exists(storeFile)) return null;

            string path = File.ReadAllText(storeFile).Trim();
            return File.Exists(path) ? path : null;
        }
    }
}