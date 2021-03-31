using System.Collections.Generic;

namespace Nexytrus
{
    public record CytrusJson
    {
        public int Version { get; set; }
        public string Name { get; set; }
        public Dictionary<string, GameData> Games { get; set; } = new ();
        public Dictionary<string, GameData> IncomingReleasedGames { get; set; } = new ();
    }
    public record GameData
    {
        public string Name { get; set; }
        public int GameId { get; set; }
        public Dictionary<string, Platform> Platforms { get; set; } = new ();
    }

    public record Platform
    {
        public string Beta { get; set; }
        public string Main { get; set; }
    }

}
