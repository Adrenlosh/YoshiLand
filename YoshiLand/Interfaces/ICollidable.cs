using Microsoft.Xna.Framework;
using YoshiLand.GameObjects;
using YoshiLand.Models;

namespace YoshiLand.Interfaces
{
    public interface ICollidable
    {
        Rectangle CollisionBox { get; }

        void OnCollision(GameObject other, ObjectCollisionResult collision);
    }
}