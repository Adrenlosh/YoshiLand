using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using System;
using System.Diagnostics;
using YoshiLand.Enums;
using YoshiLand.GameObjects;
using YoshiLand.Models;

namespace YoshiLand.Systems
{
    public struct PhysicsConfig
    {
        public PhysicsConfig() { }

        public float Gravity { get; set; } = 0.5f;
        public float MaxFallSpeed { get; set; } = 12f;
        public float GroundFriction { get; set; } = 0.8f;
        public float AirFriction { get; set; } = 0.95f;
        public float MaxHorizontalSpeed { get; set; } = 8f;
        public float Acceleration { get; set; } = 0.5f;
        public float Deceleration { get; set; } = 0.5f;
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

        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _targetObject.IsOnGround = CheckOnGround();
            ApplyFriction(elapsedTime);
            ApplyGravity(elapsedTime);
            ApplyCollisions(elapsedTime);
        }

        private void ApplyFriction(float deltaTime)
        {
            if (_targetObject.Velocity.X == 0) return;

            float friction = _targetObject.IsOnGround ?
                Config.GroundFriction : Config.AirFriction;
            float reduction = Config.Deceleration * friction * deltaTime * 60f;
            if (_targetObject.Velocity.X > 0)
            {
                _targetObject.Velocity = new Vector2(
                    Math.Max(_targetObject.Velocity.X - reduction, 0),
                    _targetObject.Velocity.Y);
            }
            else if (_targetObject.Velocity.X < 0)
            {
                _targetObject.Velocity = new Vector2(
                    Math.Min(_targetObject.Velocity.X + reduction, 0),
                    _targetObject.Velocity.Y);
            }
        }

        private void ApplyGravity(float deltaTime)
        {
            if (!_targetObject.IsOnGround)
            {
                float newVelocityY = _targetObject.Velocity.Y + Config.Gravity * deltaTime * 60f;
                newVelocityY = Math.Min(newVelocityY, Config.MaxFallSpeed);
                _targetObject.Velocity = new Vector2(_targetObject.Velocity.X, newVelocityY);
            }
            else if (_targetObject.Velocity.Y > 0)
            {
                _targetObject.Velocity = new Vector2(_targetObject.Velocity.X, 0);
            }
        }

        private void ApplyCollisions(float deltaTime)
        {
            Vector2 newPosition = _targetObject.Position;
            Vector2 velocity = _targetObject.Velocity * deltaTime * 60f;
            if (_targetObject.Velocity.X != 0) //水平碰撞检测
            {
                Vector2 horizontalMove = new Vector2(_targetObject.Velocity.X, 0);
                Vector2 testPosition = newPosition + horizontalMove;
                Rectangle testRect = _targetObject.GetCollisionBox(testPosition);
                //if (!IsOutOfTilemapSideBox(testRect))
                //{
                bool isCollided = _targetObject.IsCollidingWithTile(testRect, out TileCollisionResult result);

                if (isCollided && !result.TileType.HasFlag(TileType.Penetrable) && !result.TileType.HasFlag(TileType.Platform))
                {
                    _targetObject.Velocity = new Vector2(0, _targetObject.Velocity.Y);
                }
                else
                {
                    newPosition += horizontalMove;
                }
            }

            if (_targetObject.Velocity.Y != 0) //垂直碰撞检测
            {
                Vector2 verticalMove = new Vector2(0, _targetObject.Velocity.Y);
                Vector2 testPosition = newPosition + verticalMove;

                if (testPosition.Y < 0)
                {
                    newPosition.Y = 0;
                    _targetObject.Velocity = new Vector2(_targetObject.Velocity.X, 0);
                }
                else
                {
                    Rectangle testRect = _targetObject.GetCollisionBox(testPosition);
                    if (_targetObject.IsCollidingWithTile(testRect, out TileCollisionResult result))
                    {
                        if (result.TileType.HasFlag(TileType.Penetrable))
                        {
                            Vector2 penetratedPosition = newPosition + verticalMove;
                            Rectangle penetratedRect = _targetObject.GetCollisionBox(penetratedPosition);

                            bool willHitBlockingGround = false;
                            TileCollisionResult groundResult = new TileCollisionResult();

                            if (_targetObject.Velocity.Y > 0)
                            {
                                Rectangle groundTestRect = new Rectangle(penetratedRect.X, penetratedRect.Y + penetratedRect.Height, penetratedRect.Width, (int)Math.Abs(_targetObject.Velocity.Y) + 1);
                                willHitBlockingGround = _targetObject.IsCollidingWithTile(groundTestRect, out groundResult) && !groundResult.TileType.HasFlag(TileType.Penetrable);
                            }

                            if (willHitBlockingGround)
                            {
                                float tileTop = groundResult.TileRectangle.Top;
                                newPosition.Y = tileTop - _targetObject.SpriteSize.Y;
                                _targetObject.IsOnGround = true;
                            }
                            else
                            {
                                newPosition += verticalMove;
                                _targetObject.IsOnGround = false;
                            }
                        }
                        else
                        {
                            if (_targetObject.Velocity.Y > 0.6)
                            {
                                if (result.TileType.HasFlag(TileType.Platform))
                                {
                                    float characterBottom = newPosition.Y + _targetObject.SpriteSize.Y;
                                    float platformTop = result.TileRectangle.Top;
                                    if (characterBottom <= platformTop + 3)
                                    {
                                        newPosition.Y = platformTop - _targetObject.SpriteSize.Y;
                                        _targetObject.Velocity = new Vector2(_targetObject.Velocity.X, 0);
                                        _targetObject.IsOnGround = true;
                                    }
                                    else
                                    {
                                        newPosition += verticalMove;
                                        _targetObject.IsOnGround = false;
                                    }
                                }
                                else
                                {
                                    float tileTop = result.TileRectangle.Top;
                                    newPosition.Y = tileTop - _targetObject.SpriteSize.Y;
                                    _targetObject.Velocity = new Vector2(_targetObject.Velocity.X, 0);
                                    _targetObject.IsOnGround = true;
                                }
                            }
                            else if (_targetObject.Velocity.Y < 0)
                            {
                                if (result.TileType.HasFlag(TileType.Platform))
                                {
                                    newPosition += verticalMove;
                                    _targetObject.IsOnGround = false;
                                }
                                else
                                {
                                    newPosition.Y = result.TileRectangle.Bottom;
                                    _targetObject.Velocity = new Vector2(_targetObject.Velocity.X, 0);
                                }
                            }
                        }

                    }
                    else
                    {
                        newPosition += verticalMove;
                        _targetObject.IsOnGround = false;
                    }
                }
            }
            else
            {
                Rectangle collisionBox = _targetObject.GetCollisionBox(newPosition);
                if (_targetObject.IsCollidingWithTile(new Rectangle(collisionBox.X, collisionBox.Y + collisionBox.Height, collisionBox.Width, 3), out TileCollisionResult groundResult))
                {
                    if (!groundResult.TileType.HasFlag(TileType.Penetrable))
                    {
                        _targetObject.IsOnGround = true;
                    }
                    else
                    {
                        _targetObject.IsOnGround = false;
                    }
                }
                else
                {
                    _targetObject.IsOnGround = false;
                }
            }
            _targetObject.Position = newPosition;
        }

        private bool CheckOnGround()
        {
            Rectangle collisionBox = _targetObject.GetCollisionBox(_targetObject.Position);
            Rectangle testRectangle = new Rectangle(collisionBox.X, collisionBox.Y + collisionBox.Height, collisionBox.Width, 1);
            return _targetObject.IsCollidingWithTile(testRectangle, out TileCollisionResult collisionResult) && !collisionResult.TileType.HasFlag(TileType.Penetrable);
        }

        public void ApplyJump(float jumpStrength)
        {
            if (_targetObject.IsOnGround)
            {
                _targetObject.Velocity = new Vector2(_targetObject.Velocity.X, -jumpStrength);
                _targetObject.IsOnGround = false;
            }
        }

        public void ApplyAcceleration(float acceleration, float maxSpeed)
        {
            float newVelocityX = _targetObject.Velocity.X + acceleration;
            newVelocityX = Math.Clamp(newVelocityX, -maxSpeed, maxSpeed);
            _targetObject.Velocity = new Vector2(newVelocityX, _targetObject.Velocity.Y);
        }
    }
}