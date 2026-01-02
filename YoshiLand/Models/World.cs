using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace YoshiLand.Models
{
    public class World(string name, string displayName, List<Stage> stages, Texture2D thumbnail, List<Vector2> thumbnailPositions)
    {
        public string Name { get; set; } = name;
        public string DisplayName { get; set; } = displayName;
        public List<Stage> Stages { get; } = stages;
        public Texture2D Thumbnail { get; } = thumbnail;
        public List<Vector2> ThumbnailPositions { get; } = thumbnailPositions;
    }
}