using Microsoft.Xna.Framework;

using YoshiLand.GameObjects;
using YoshiLand.Systems;
using System;

namespace YoshiLand.Models
{
    public struct ObjectCollisionResult
    {
        public GameObject CollidedObject { get; set;  }

        public Rectangle Intersection { get; set; }

        public CollisionDirection Direction { get; set; }

        public ObjectCollisionResult(GameObject obj, Rectangle intersection, Rectangle originalRect, Rectangle otherRect)
        {
            CollidedObject = obj;
            Intersection = intersection;
            Direction = CalculateCollisionDirection(originalRect, otherRect, intersection);
        }

        private CollisionDirection CalculateCollisionDirection(Rectangle rectA, Rectangle rectB, Rectangle intersection)
        {
            float overlapLeft = Math.Abs(rectA.Right - rectB.Left);
            float overlapRight = Math.Abs(rectB.Right - rectA.Left);
            float overlapTop = Math.Abs(rectA.Bottom - rectB.Top);
            float overlapBottom = Math.Abs(rectB.Bottom - rectA.Top);
            float minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight), Math.Min(overlapTop, overlapBottom));
            if (minOverlap == overlapTop) return CollisionDirection.Top;
            if (minOverlap == overlapBottom) return CollisionDirection.Bottom;
            if (minOverlap == overlapLeft) return CollisionDirection.Left;
            return CollisionDirection.Right;
        }
    }
}