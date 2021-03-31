using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nexytrus
{
    public class Cytrus
    {
        private bool _initialized;
        private WebClient _client;
        private readonly string _cytrusUrl;
        private readonly string _baseUrl;
        public CytrusJson Json;
        private List<FileInfos> _files = new();
        private readonly List<FileInfos> _filesToRetry = new();
        private int _filesToDownloadCount;
        private int _downloadedFiles;

        public Cytrus(string cytrusUrl, string baseUrl)
        {
            _cytrusUrl = cytrusUrl;
            _baseUrl = baseUrl;
        }

        public async Task Initialize()
        {
            if (_initialized)
                return;

            Console.WriteLine("Initializing Cytrus...");
            _client = new WebClient();
            var result = await _client.DownloadStringTaskAsync(_cytrusUrl);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
            };
            Json = JsonSerializer.Deserialize<CytrusJson>(result, options);

            _initialized = true;
            Console.WriteLine("Cytrus initialized!");

        }

        public async Task DownloadGame(string game, string platform, string build, string version)
        {
            PrintGame(game, platform, build, version);
            var gamePath = $"./{game}_{platform}_{build}_{version}/";
            if (!Directory.Exists(gamePath))
                Directory.CreateDirectory(gamePath);

            var result = await _client.DownloadStringTaskAsync(_baseUrl + $"{game}/releases/{build}/{platform}/{version}.json");
            var doc = JsonDocument.Parse(result);

            foreach (var path in doc.RootElement.EnumerateObject())
                foreach (var obj in path.Value.GetProperty("files").EnumerateObject())
                    _files.Add(new FileInfos(gamePath + obj.Name, obj.Name, obj.Value.GetProperty("hash").ToString(), long.Parse(obj.Value.GetProperty("size").ToString())));


            _files = _files.GroupBy(p => p.FilePath).Select(g => g.First()).ToList();

            _filesToDownloadCount = _files.Count;
            Parallel.ForEach(
                _files,
                new ParallelOptions { MaxDegreeOfParallelism = ConfigManager.Config.MaxConcurrency },
                f =>
                {
                    DownloadFile(f, game);
                });

            while (_filesToRetry.Count > 0)
            {
                Parallel.ForEach(
                    _filesToRetry.ToArray(),
                    new ParallelOptions {MaxDegreeOfParallelism = ConfigManager.Config.MaxConcurrency },
                    f => { DownloadFile(f, game, true); });
            }
            Console.WriteLine($"Download finished! All {_downloadedFiles}/{_filesToDownloadCount} files were downloaded.");
            _files.Clear();
        }

        private void DownloadFile(FileInfos infos, string game, bool retry = false)
        {
            var url = $"{_baseUrl}{game}/hashes/{infos.Hash[..2]}/{infos.Hash}";

            if (!Directory.Exists(Path.GetDirectoryName(infos.FilePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(infos.FilePath));

            Console.WriteLine($"[Progress : {_downloadedFiles} files downloaded out of {_filesToDownloadCount}] Downloading {infos.OriginalFilePath} ...");
            try
            {
                new WebClient().DownloadFile(url, infos.FilePath);// we have to create a WebClient each time we download a file since it is not thread safe
                if (retry)
                    _filesToRetry.RemoveAll(x=>x.FilePath == infos.FilePath);
                Interlocked.Increment(ref _downloadedFiles);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    Console.Title = $"[{_downloadedFiles}/{_filesToDownloadCount}] downloaded";
            }
            catch (Exception e)
            {
                Console.WriteLine($"Caught exception while downloading file {infos.FilePath} : {e.Message}. will try to redownload it later");
                if (!retry)
                    _filesToRetry.Add(infos);
            }
        }

        private static void PrintGame(string game, string platform, string build, string version)
        {
            Console.Write("Downloading ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(game);
            Console.ResetColor();
            Console.Write(" in platform ");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(platform);
            Console.ResetColor();
            Console.Write(" with build ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(build);
            Console.ResetColor();
            Console.Write(" of version ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(version);
            Console.ResetColor();
        }
    }
}
