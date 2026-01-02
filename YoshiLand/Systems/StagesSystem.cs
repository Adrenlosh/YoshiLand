using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using YoshiLand.Models;

namespace YoshiLand.Systems
{
    public static class StagesSystem
    {
        private static ContentManager _contentManager;

        public const string StagesDirectory = "Stages";
        public const string WorldsDirectory = "Worlds";

        public static List<World> Worlds { get; set; } = new List<World>();

        public static void Initialize(ContentManager contentManager)
        {
            _contentManager = contentManager;
            string worldListPath = Path.Combine(_contentManager.RootDirectory, WorldsDirectory, "Worlds.txt");
          
            List<string> worldFiles = new List<string>();
            using Stream stream = TitleContainer.OpenStream(worldListPath);
            using StreamReader reader = new StreamReader(stream);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    worldFiles.Add(line.Trim());
                }
            }

            foreach(var worldFile in worldFiles)
            {
                World world = LoadWorldFromFile(worldFile);
                Worlds.Add(world);
            }
            //foreach (var stageFile in stageFiles)
            //{
            //    Stage stage = LoadStageFromFile(stageFile);
            //    Stages.Add(stage);
            //}
        }

        public static Stage GetStageByName(string worldName, string stageName)
        {
            return GetWorldByName(worldName).Stages.Find(s => s.Name == stageName);
        }

        public static World GetWorldByName(string worldName)
        {
            return Worlds.Find(s => s.Name == worldName);
        }

        public static Stage LoadStageFromFile(string filePath)
        {
            using Stream stream = TitleContainer.OpenStream(filePath);
            using XmlReader xmlReader = XmlReader.Create(stream);
            XDocument doc = XDocument.Load(xmlReader);
            XElement root = doc.Root;
            string name = root.Attribute("name")?.Value ?? string.Empty;
            string displayName = root.Attribute("displayName")?.Value ?? string.Empty;
            string entryMap = root.Attribute("entryMap")?.Value ?? string.Empty;
            List<string> tmps = new List<string>();
            var tilemaps = root.Element("Tilemaps")?.Elements("Tilemap");
            if (tilemaps != null)
            {
                foreach (var tilemap in tilemaps)
                {
                    string file = tilemap.Attribute("file")?.Value ?? string.Empty;
                    tmps.Add(file);
                }
            }
            return new Stage(name, displayName, entryMap, tmps, _contentManager);
        }

        public static World LoadWorldFromFile(string filePath)
        {
            using Stream stream = TitleContainer.OpenStream(Path.Combine(_contentManager.RootDirectory, WorldsDirectory, filePath, "world.xml"));
            using XmlReader xmlReader = XmlReader.Create(stream);
            XDocument doc = XDocument.Load(xmlReader);
            XElement root = doc.Root;
            List<Stage> stages = new List<Stage>();
            List<Vector2> thumbnailPositions = new List<Vector2>();
            string name = root.Attribute("name")?.Value ?? string.Empty;
            string displayName = root.Attribute("displayName")?.Value ?? string.Empty;
            IEnumerable<XElement> stageElements = root.Element("Stages")?.Elements("Stage");
            if (stageElements != null)
            {
                foreach (var stageElement in stageElements)
                {
                    string stageFile = stageElement.Attribute("file")?.Value ?? string.Empty;

                    string thumbnailPosStr = stageElement.Attribute("thumbnailPosition")?.Value ?? "0, 0";
                    string[] posParts = thumbnailPosStr.Split(',');
                    thumbnailPositions.Add(new Vector2(posParts.Length > 0 ? float.Parse(posParts[0]) : 0f, posParts.Length > 1 ? float.Parse(posParts[1]) : 0f));

                    Stage stage = LoadStageFromFile(Path.Combine(_contentManager.RootDirectory, StagesDirectory, filePath, stageFile + ".xml"));
                    if (stage != null)
                    {
                        stages.Add(stage);
                    }
                }
            }
            return new World(name, displayName, stages, _contentManager.Load<Texture2D>(Path.Combine(WorldsDirectory, filePath, "thumbnail")), thumbnailPositions);
        }
    }
}