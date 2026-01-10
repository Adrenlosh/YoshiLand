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
    public class PhysicsSystem
    {
        private readonly GameObject _targetObject;
        private readonly TiledMap _tilemap;

        public float Gravity { get; set; } = 30f;
        public float MaxFallSpeed { get; set; } = 60f;
        public float GroundFriction { get; set; } = 0.6f;
        public float AirFriction { get; set; } = 0.7f;
        public float Deceleration { get; set; } = 30f;
        public bool HasCollisions { get; set; }  = true;
        public bool HasGravity
        {
            get => field;
            set
            {
                field = value;
                if (!field)
                {
                    _targetObject.Velocity = Vector2.Zero;
                }
            }
        } = true;
        

        public PhysicsSystem(GameObject targetObject, TiledMap tilemap)
        {
            _targetObject = targetObject;
            _tilemap = tilemap;
        }

        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // _targetObject.IsOnGround = CheckOnGround();
            _targetObject.IsOnGround = false;
            ApplyCollisions(elapsedTime);
            if(HasGravity) ApplyGravity(elapsedTime);
            ApplyFriction(elapsedTime);
 
        }

        private void ApplyFriction(float elapsedTime)
        {
            if (_targetObject.Velocity.X == 0) return;
            float friction = _targetObject.IsOnGround ? GroundFriction : AirFriction;
            float reduction = Deceleration * friction * elapsedTime;
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


        private bool CheckOnGround() //FIXME: 地面检查有问题
        {
            Rectangle collisionBox = _targetObject.GetCollisionBox(_targetObject.Position);
            Rectangle testRectangle = new Rectangle(collisionBox.X, collisionBox.Y + collisionBox.Height, collisionBox.Width, 2);
            return _targetObject.IsCollidingWithTile(testRectangle, out TileCollisionResult collisionResult) && !collisionResult.TileType.HasFlag(TileType.Penetrable);
        }

        private void ApplyGravity(float elapsedTime)
        {
            if (!_targetObject.IsOnGround)
            {
                float newVelocityY = _targetObject.Velocity.Y + Gravity * elapsedTime;
                newVelocityY = Math.Min(newVelocityY, MaxFallSpeed);
                _targetObject.Velocity = new Vector2(_targetObject.Velocity.X, newVelocityY);
            }
            else if (_targetObject.Velocity.Y > 0 && HasCollisions)
            {
                _targetObject.Velocity = new Vector2(_targetObject.Velocity.X, 0);
            }
        }

        private void ApplyCollisions(float deltaTime) //TODO: 重写碰撞检测
        {
            //Vector2 newPosition = _targetObject.Position;
            //Vector2 velocity = _targetObject.Velocity * deltaTime * 60f;

            //if(_targetObject.Velocity.X != 0) //水平碰撞检测
            //{
            //    Vector2 testPosition = new Vector2(newPosition.X + velocity.X, newPosition.Y);
            //    bool isCollided = _targetObject.IsCollidingWithTile(_targetObject.GetCollisionBox(testPosition), out TileCollisionResult collisionResult);
            //    if(isCollided && !collisionResult.TileType.HasFlag(TileType.Penetrable) && !collisionResult.TileType.HasFlag(TileType.Platform))
            //    {
            //        _targetObject.Velocity = new Vector2(0, _targetObject.Velocity.Y);
            //    }
            //    else
            //    {
            //        newPosition = testPosition;
            //    }
            //}
            //if(_targetObject.Velocity.Y != 0) //垂直碰撞检测
            //{
            //    Vector2 testPosition = new Vector2(newPosition.X, _targetObject.Position.Y + velocity.Y);
            //    Rectangle testRectangle = _targetObject.GetCollisionBox(testPosition);
            //    //if()
            //    //if (velocity.Y > 0) //下落
            //    //{

            //    //}
            //    //else if (velocity.Y < 0) //上升
            //    //{

            //    //}
            //    newPosition = testPosition;
            //}



            //_targetObject.Position = newPosition;


            Vector2 newPosition = _targetObject.Position;
            Vector2 velocity = _targetObject.Velocity * deltaTime * 60f;
            if (_targetObject.Velocity.X != 0) //水平碰撞检测
            {
                Vector2 horizontalMove = new Vector2(_targetObject.Velocity.X, 0);
                Vector2 testPosition = newPosition + horizontalMove;
                Rectangle testRect = _targetObject.GetCollisionBox(testPosition);
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

                if (testPosition.Y < 0 && HasCollisions) // 瓦片地图顶部
                {
                    newPosition.Y = 0;
                    _targetObject.Velocity = new Vector2(_targetObject.Velocity.X, 0);
                }
                else
                {
                    Rectangle testRect = _targetObject.GetCollisionBox(testPosition);
                    if (_targetObject.IsCollidingWithTile(testRect, out TileCollisionResult result) && HasCollisions)
                    {
                        if (result.TileType.HasFlag(TileType.Penetrable))
                        {
                            Vector2 penetratedPosition = newPosition + verticalMove;
                            Rectangle penetratedRect = _targetObject.GetCollisionBox(penetratedPosition);

                            bool willHitBlockingGround = false;
                            TileCollisionResult groundResult = new TileCollisionResult();

                            if (_targetObject.Velocity.Y > 0)
                            {
                                Rectangle groundTestRect = new Rectangle(penetratedRect.X, penetratedRect.Y + penetratedRect.Height, penetratedRect.Width, (int)Math.Abs(_targetObject.Velocity.Y) + 3);
                                willHitBlockingGround = _targetObject.IsCollidingWithTile(groundTestRect, out groundResult) && !groundResult.TileType.HasFlag(TileType.Penetrable);
                            }

                            if (willHitBlockingGround)
                            {
                                float tileTop = groundResult.TileRectangle.Top;
                                newPosition.Y = (int)(tileTop - _targetObject.SpriteSize.Y);
                                _targetObject.IsOnGround = true; //在Penterable中掉落在Blocking或Platform上
                            }
                            else
                            {
                                newPosition += verticalMove;
                                _targetObject.IsOnGround = false;
                            }
                        }
                        else
                        {
                            if (_targetObject.Velocity.Y > 3f && HasCollisions)
                            {
                                if (result.TileType.HasFlag(TileType.Platform))
                                {
                                    float characterBottom = newPosition.Y + _targetObject.SpriteSize.Y;
                                    float platformTop = result.TileRectangle.Top;
                                    if (characterBottom <= platformTop + 3)
                                    {
                                        newPosition.Y = (int)(platformTop - _targetObject.SpriteSize.Y);
                                        _targetObject.Velocity = new Vector2(_targetObject.Velocity.X, 0);
                                        _targetObject.IsOnGround = true; //从上方掉落在Platform瓦片
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
                                    newPosition.Y = (int)(tileTop - _targetObject.SpriteSize.Y);
                                    _targetObject.Velocity = new Vector2(_targetObject.Velocity.X, 0);
                                    _targetObject.IsOnGround = true; //从上方掉落在Blocking瓦片
                                }
                            }
                            else if (_targetObject.Velocity.Y < 0 && HasCollisions)
                            {
                                if (result.TileType.HasFlag(TileType.Platform))
                                {
                                    newPosition += verticalMove;
                                    _targetObject.IsOnGround = false;
                                }
                                else
                                {
                                    newPosition.Y = result.TileRectangle.Bottom; //撞到瓦片底部
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
                if (_targetObject.IsCollidingWithTile(new Rectangle(collisionBox.X, collisionBox.Y + collisionBox.Height, collisionBox.Width, 3), out TileCollisionResult groundResult) && HasCollisions)
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


        public void ApplyJump(float jumpForce, bool ignoreGroundCheck = false)
        {
            if (ignoreGroundCheck || _targetObject.IsOnGround)
            {
                _targetObject.Velocity = new Vector2(_targetObject.Velocity.X, -jumpForce);
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