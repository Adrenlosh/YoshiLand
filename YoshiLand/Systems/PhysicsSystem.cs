using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using System;
using YoshiLand.Enums;
using YoshiLand.GameObjects;
using YoshiLand.Models;

namespace YoshiLand.Systems
{
    public struct PhysicsConfig
    {
        public PhysicsConfig() { }

        //public const float Gravity = 0.5f;
        //public const float MaxFallSpeed = 12f;
        //public const float GroundFriction = 0.8f;
        //public const float AirFriction = 0.95f;
        //public const float MaxRunSpeed = 5f;
        //public const float Acceleration = 0.5f;
        //public const float JumpStrength = 10f;
        public float Gravity { get; set; } = 0.5f;
        public float MaxFallSpeed { get; set; } = 12f;
        public float Fiction { get; set; } = 0.8f;
        public float AirFiction { get; set; } = 0.95f;
    }
    public class PhysicsSystem
    {
        private GameObject _targetObject;
        private TiledMap _tilemap;

        public PhysicsConfig Config { get; set; } = new PhysicsConfig();

        public PhysicsSystem(GameObject targetObject, TiledMap tilemap) 
        { 
            _targetObject = targetObject;
            _tilemap = tilemap;
        }

        public void Apply(GameTime gameTime, bool applyImmediately = true)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _targetObject.IsOnGround = CheckOnGround();
            if (_targetObject.Velocity.Y != 0)
            {
                VerticalCollision();
            }

            if(_targetObject.Velocity.X != 0)
            {
                HorizontalCollision();
            }

            ApplyFicition();
        }

        private void HorizontalCollision() // X axis
        {

        }

        private void VerticalCollision() // Y axis
        {
            if (!_targetObject.IsOnGround)
            {
                _targetObject.Velocity += new Vector2(0, Config.Gravity);
            }
            else
            {
                _targetObject.Velocity.SetY(0);
            }
            Math.Clamp(_targetObject.Velocity.Y, 0, Config.MaxFallSpeed);
        }

        private void ApplyFicition()
        {

        }

        private bool CheckOnGround()
        {
            Rectangle collisionBox = _targetObject.GetCollisionBox(_targetObject.Position);
            Rectangle testRectangle = new Rectangle(collisionBox.X, collisionBox.Y + collisionBox.Height, collisionBox.Width, 3);
            return _targetObject.IsCollidingWithTile(testRectangle, out TileCollisionResult collisionResult) && !collisionResult.TileType.HasFlag(TileType.Penetrable);
        }
    }
}