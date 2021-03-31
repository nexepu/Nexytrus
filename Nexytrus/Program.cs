using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nexytrus
{
    internal class Program
    {
        private static async Task Main()
        {
            Console.WriteLine("Loading config...");
            ConfigManager.LoadConfig();
            var c = new Cytrus(ConfigManager.Config.CytrusUrl, ConfigManager.Config.BaseUrl);
            await c.Initialize();

            if (c.Json.IncomingReleasedGames.Count > 0)
            {
                Console.WriteLine($"1: Available games : {c.Json.Games.Count}");
                Console.WriteLine($"2: Available IncomingReleasedGames : {c.Json.IncomingReleasedGames.Count}");
                redoFirstChoice:
                Console.WriteLine($"Please select a choice, which list of games do you wanna chose?");
                var choice = Console.ReadKey().KeyChar;
                Console.WriteLine();
                if (choice != '1' && choice != '2')
                {
                    Console.WriteLine("Invalid choice.");
                    goto redoFirstChoice;
                }

                if (choice == '1')
                    await SelectGame(false, c);
                else if (choice == '2')
                    await SelectGame(true, c);
            }
            else
                await SelectGame(false, c);

            Console.WriteLine("Program terminated, press any key to exit.");
            Console.ReadKey();
        }

        static async Task SelectGame(bool unreleasedGames, Cytrus c)
        {
            var gamesDict = unreleasedGames
                ? CreateGameChoicesList(c.Json.IncomingReleasedGames)
                : CreateGameChoicesList(c.Json.Games);
            foreach (var game in gamesDict)
            {
                Console.WriteLine($"{game.Key}: {game.Value.Item1}");
            }
            redo:
            Console.WriteLine("Please select a game from the list on top");
            var choice = Console.ReadKey().KeyChar;
            Console.WriteLine();
            var realChoice = int.Parse(choice.ToString());
            if (realChoice > gamesDict.Count || realChoice < 0)
            {
                Console.WriteLine("Invalid choice.");
                goto redo;
            }

            await SelectPlatform(gamesDict[realChoice].Item1, gamesDict[realChoice].Item2, c);
        }
        static async Task SelectPlatform(string game, GameData data, Cytrus c)
        {
            var platformDict = CreatePlatformChoicesList(data.Platforms);
            foreach (var platform in platformDict)
                Console.WriteLine($"{platform.Key}: {platform.Value.Item1}");
            redo:
            Console.WriteLine("Please select a platform from the list on top");
            var choice = Console.ReadKey().KeyChar;
            Console.WriteLine();
            var realChoice = int.Parse(choice.ToString());
            if (realChoice > platformDict.Count || realChoice < 0)
            {
                Console.WriteLine("Invalid choice.");
                goto redo;
            }
            await SelectGameVersion(game, platformDict[realChoice].Item1, platformDict[realChoice].Item2, c);
        }
        static async Task SelectGameVersion(string game, string platform, Platform platformInfos, Cytrus c)
        {
            int count = 1;
            var dict = new Dictionary<int, Tuple<string, string>>();
            if (!string.IsNullOrEmpty(platformInfos.Beta))
                dict.Add(count++, new Tuple<string, string>("beta", platformInfos.Beta));
            if (!string.IsNullOrEmpty(platformInfos.Main))
                dict.Add(count++, new Tuple<string, string>("main", platformInfos.Main));

            dict.Add(count++, new Tuple<string, string>("custom", "you choose the custom version"));

            foreach (var ver in dict)
                Console.WriteLine($"{ver.Key}: {ver.Value.Item1} = {ver.Value.Item2}");
            redo:
            Console.WriteLine("Please select a version from the list on top");
            var choice = Console.ReadKey().KeyChar;
            Console.WriteLine();
            var realChoice = int.Parse(choice.ToString());
            if (realChoice > dict.Count || realChoice < 0)
            {
                Console.WriteLine("Invalid choice.");
                goto redo;
            }


            var build = dict[realChoice].Item1;
            var version = dict[realChoice].Item2;

            if (realChoice == dict.Count) // check if its the last choice aka the custom one.
            {
                Console.WriteLine("Please write your custom version's build");
                build = Console.ReadLine();
                Console.WriteLine("Please write your custom version");
                version = Console.ReadLine();
            }
            await c.DownloadGame(game, platform, build, version);
        }
        static Dictionary<int, Tuple<string, Platform>> CreatePlatformChoicesList(Dictionary<string, Platform> dict)
        {
            int count = 1;
            var choices = new Dictionary<int, Tuple<string, Platform>>();
            foreach (var platform in dict)
            {
                choices.Add(count++, new Tuple<string, Platform>(platform.Key, platform.Value));
            }
            return choices;
        }
        static Dictionary<int, Tuple<string, GameData>> CreateGameChoicesList(Dictionary<string, GameData> dict)
        {
            int count = 1;
            var choices = new Dictionary<int, Tuple<string, GameData>>();
            foreach (var game in dict)
            {
                choices.Add(count++, new Tuple<string, GameData>(game.Key, game.Value));
            }
            return choices;
        }
    }
}
