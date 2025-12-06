using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using System;
using System.Diagnostics;
using YoshiLand.Models;
using YoshiLand.Systems;

namespace YoshiLand.GameObjects
{
    public enum TongueState
    {
        None,
        Extending,
        Retracting
    }

    public enum PlummetState
    {
        None,
        TurnAround,
        FastFall
    }

    public class Yoshi : GameObject, IDamageable
    {
        private AnimatedSprite _yoshiSprite;
        private GameObject _capturedObject;
        private int _lastDirection;

        public override Vector2 CenterBottomPosition
        {
            get => new Vector2(Position.X + _yoshiSprite.Size.X / 2, Position.Y + _yoshiSprite.Size.Y);
            set => Position = new Vector2(value.X - _yoshiSprite.Size.X / 2, value.Y - _yoshiSprite.Size.Y);
        }

        public Vector2 CenterPosition
        {
            get => new Vector2(Position.X + _yoshiSprite.Size.X / 2, Position.Y + _yoshiSprite.Size.Y / 2);
            set => Position = new Vector2(value.X - _yoshiSprite.Size.X / 2, value.Y - _yoshiSprite.Size.Y / 2);
        }

        public Vector2 EggHoldingPosition
        {
            get
            {
                if (_yoshiSprite.Effect == SpriteEffects.FlipHorizontally)
                {
                    return Position + new Vector2(8, 8);
                }
                else
                {
                    return Position + new Vector2(0, 8);
                }
            }
        }

        public bool CanThrowEgg { get; set; } = true;

        public bool CanHandleInput { get; set; } = true;

        public int FaceDirection => _lastDirection;

        public GameObject CapturedObject => _capturedObject;

        public int Health { get; private set; } = 4;

        public int MaxHealth { get; private set; } = 4;

        public override Rectangle CollisionBox => GetCollisionBoxBottomCenter(Position, _yoshiSprite.Size);

        public event Action<Vector2> OnThrowEgg;
        public event Action<Vector2> OnReadyThrowEgg;
        public event Action<Vector2> OnPlummeted;
        public event Action OnDie;
        public event Action OnDieComplete;

        public Yoshi(SpriteSheet yoshiSpriteSheet, SpriteSheet crosshairSpriteSheet, Texture2D tongueTexture, TiledMap tilemap) : base(tilemap)
        {
            _yoshiSprite = new AnimatedSprite(yoshiSpriteSheet);
            SetYoshiAnimation("stand", true);
            //_crosshairSprite = new AnimatedSprite(crosshairSpriteSheet);
            //_crosshairSprite.SetAnimation("shine");
            //_tongueSprite = new Sprite(tongueTexture);
            Size = new Point(16, 32);
        }

        public override void OnCollision(GameObject other, ObjectCollisionResult collision)
        {
            if (other == _capturedObject || other.IsCaptured)
                return;

            base.OnCollision(other, collision);
        }

        public void TakeDamage(int damage, GameObject source)
        {
        }

        public void Hurt()
        {
        }

        public void Die(bool clearHealth = false)
        {
        }

        public void HandleInput(GameTime gameTime)
        {
            int currentInputDirection = 0;
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;


            if (currentInputDirection != 0 )
            {
                _lastDirection = currentInputDirection;
            }

            if(GameControllerSystem.JumpPressed())
            {
                Velocity += new Vector2(Velocity.X, -1f);
            }
        }

        #region Animation
        private void SetYoshiAnimation(string name, bool forceSet = false, bool ignoreMouthingStatus = false)
        {
            if (!forceSet && IsYoshiAnimationEqual(name, ignoreMouthingStatus))
                return;

            string animationName = name;
            //if (!ignoreMouthingStatus && _isMouthing)
            //{
            //    animationName += "-mouthing";
            //}
            _yoshiSprite.SetAnimation(animationName);
        }

        private bool IsYoshiAnimationEqual(string name, bool ignoreMouthingStatus = false)
        {
            if (_yoshiSprite.CurrentAnimation != null)
            {
                string expectedName = name;
                //if (!ignoreMouthingStatus && _isMouthing)
                //{
                //    expectedName += "-mouthing";
                //}
                return _yoshiSprite.CurrentAnimation == expectedName;
            }
            return false;
        }

        private void UpdateAnimation()
        {

        }

        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (CanHandleInput)
                HandleInput(gameTime);
            Physics.Apply(gameTime);
            Position += Velocity;
            //Position = new Vector2(100, 100);
            Debug.WriteLine(Velocity);
            _yoshiSprite.Update(gameTime);
        }



        #endregion

        #region Draw        
        public override void Draw(SpriteBatch spriteBatch)
        {
            _yoshiSprite.Draw(spriteBatch, Position, 0, Vector2.One);
        }
        #endregion

        #region Misc
        public override Rectangle GetCollisionBox(Vector2 position)
        {
            int X = (int)(position.X + _yoshiSprite.Size.X / 2 - Size.X / 2);
            int Y = (int)(position.Y + _yoshiSprite.Size.Y - Size.Y);
            return new Rectangle(X, Y, Size.X, Size.Y);
        }

        private Vector2 GetCurrentThrowDirection()
        {
            return new Vector2(FaceDirection, 0);
        }

        public void Bounce()
        {
        }

        public void ResetVelocity(bool resetAllMovement = false)
        {
            
        }

        public void ResetJumpStatus()
        {
        }
        #endregion
    }
}