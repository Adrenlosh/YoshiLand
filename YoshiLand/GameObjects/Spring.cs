using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using YoshiLand.Models;
using YoshiLand.Systems;

namespace YoshiLand.GameObjects
{
    public enum SpringStatus
    {
        Normal,
        Compressing,
        CompressedToMax,
        Expanding
    }

    public class Spring : GameObject
    {
        private const float Gravity = 0.5f;
        private const float MaxGravity = 8f;
        private const float Friction = 0.5f;

        private readonly Point _normalCollisionBox = new Point(16, 16);
        private readonly Point _minimumCollisionBox = new Point(16, 8);
        private float _keepTimer = 0f;
        private bool _isOnGround = false;
        private Vector2 _velocity = Vector2.Zero;
        private const float KeepDuration = 0.1f;
        private Vector2 _basePosition;
        AnimatedSprite _sprite;

        public SpringStatus Status { get; private set; } = SpringStatus.Normal;

        public override Vector2 Velocity
        {
            get => _velocity;
            set => _velocity = value;
        }

        public override Rectangle CollisionBox => GetCollisionBox(Position);

        public Spring(SpriteSheet spriteSheet, TiledMap tilemap) : base(tilemap)
        {
            _sprite = new AnimatedSprite(spriteSheet);
            _sprite.SetAnimation("normal");
            Size = _normalCollisionBox;
            IsCapturable = true;
        }

        public override void OnCollision(GameObject other, ObjectCollisionResult collision)
        {
            base.OnCollision(other, collision);
            other.Physics.ApplyJump(100f);
            if (collision.Direction == CollisionDirection.Top && Status == SpringStatus.Normal)
            {
                //Compress();
            }
        }

        public void Compress()
        {
            if (Status != SpringStatus.Normal) return;
            Status = SpringStatus.Compressing;
            _basePosition = Position;
            _keepTimer = KeepDuration;
            if (_sprite.CurrentAnimation != "compress")
            {
                _sprite.SetAnimation("compress");
            }
        }

        public void Release()
        {
            if (Status == SpringStatus.CompressedToMax)
            {
                Status = SpringStatus.Expanding;
                if (_sprite.CurrentAnimation != "expand")
                {
                    _sprite.SetAnimation("expand");
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch, Position, 0, Vector2.One);
        }

        public override void Update(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 newPosition = Position;
            switch (Status)
            {
                case SpringStatus.Compressing:
                    UpdateCompressing();
                    break;

                case SpringStatus.CompressedToMax:
                    UpdateCompressedToMax(elapsedTime);
                    break;

                case SpringStatus.Expanding:
                    UpdateExpanding();
                    break;

                case SpringStatus.Normal:
                    break;
            }
            Physics.ApplyPhysics(gameTime);
            _sprite.Update(gameTime);
        }

        private void UpdateCompressing()
        {
            int newHeight = Size.Y - 2;
            if (newHeight <= _minimumCollisionBox.Y)
            {
                Size = _minimumCollisionBox;
                Status = SpringStatus.CompressedToMax;
                _keepTimer = KeepDuration;
            }
            else
            {
                Size = new Point(Size.X, newHeight);
            }
        }

        private void UpdateCompressedToMax(float deltaTime)
        {
            _keepTimer -= deltaTime;
            if (_keepTimer <= 0)
            {
                Release();
            }
        }

        private void UpdateExpanding()
        {
            int newHeight = Size.Y + 4;
            if (newHeight >= _normalCollisionBox.Y)
            {
                Size = _normalCollisionBox;
                Position = _basePosition;
                Status = SpringStatus.Normal;
                if (_sprite.CurrentAnimation != "normal")
                    _sprite.SetAnimation("normal");
            }
            else
            {
                Size = new Point(Size.X, newHeight);
            }
        }

        public void Reset()
        {
            Status = SpringStatus.Normal;
            Size = _normalCollisionBox;
            Position = _basePosition;
            _sprite.SetAnimation("normal");
        }
    }
}