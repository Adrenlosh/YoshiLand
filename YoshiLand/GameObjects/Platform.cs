using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using YoshiLand.Interfaces;
using YoshiLand.Models;

namespace YoshiLand.GameObjects
{
    public class Platform : GameObject, ICollidable
    {
        private readonly Sprite _sprite;

        public override Rectangle CollisionBox => GetCollisionBox(Position);

        public Platform(Texture2D texture, TiledMap tilemap) : base(tilemap)
        {
            _sprite = new Sprite(texture);
            Size = _sprite.Size;
            IsCapturable = false;
            Physics.HasGravity = false;
        }

        public override void OnCollision(GameObject other, ObjectCollisionResult collision)
        {
            if(other is Yoshi player)
            {
                player.Velocity = Velocity;
                player.IsOnGround = true;
            }
            base.OnCollision(other, collision);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_sprite, Position);
        }

        public override void Update(GameTime gameTime)
        {
            Physics.ApplyPhysics(gameTime);
            if ((int)gameTime.TotalGameTime.TotalSeconds % 2 == 0)
            {
                Velocity = new Vector2(0.5f, Velocity.Y);
            }
            else
            {
                Velocity = new Vector2(-0.5f, Velocity.Y);
            }
        }
    }
}
