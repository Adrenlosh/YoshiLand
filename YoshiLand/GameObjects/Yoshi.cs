using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using System;
using YoshiLand.Enums;
using YoshiLand.Interfaces;
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

    public class Yoshi : GameObject, IDamageable //TODO:飘浮、重摔
    {
        private const float TongueSpeed = 300f;
        private const float MaxTongueLength = 50f;
        private const float CrosshairRadius = 85f;
        private readonly float MaxAngleRadians = MathHelper.ToRadians(130);

        private readonly AnimatedSprite _yoshiSprite;
        private readonly AnimatedSprite _crosshairSprite;
        private readonly Sprite _tongueSprite;
        private float _tongueLength = 0f;

        private GameObject _capturedObject = null;
        private Vector2 _tongueDirection = Vector2.Zero;
        private TongueState _tongueState = TongueState.None;

        private Vector2 _rotatingSpritePosition;
        private float _currentAngle = 0f;
        private float _rotationSpeed = 2f;
        private bool _lastCenterFacingRight = true;
        private Vector2 _throwDirection;
        private bool _hasThrownEgg = false;
        private float _throwingAnimationTimer = 0f;

        private int _lastDirection;

        private bool _isSquating = false;
        private bool _isLookingUp = false;
        private bool _isMouthing = false;
        private bool _isSpitting = false;
        private bool _isHoldingEgg = false;
        public override Point SpriteSize => _yoshiSprite.Size;
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
            _crosshairSprite = new AnimatedSprite(crosshairSpriteSheet);
            _crosshairSprite.SetAnimation("shine");
            _tongueSprite = new Sprite(tongueTexture);
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
            int currentDirection = 0;
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (GameControllerSystem.ActionPressed() &&  IsOnGround && CanThrowEgg) //!_isFloating &&
            {
                if (GameMain.PlayerStatus.Egg > 0)
                {
                    if (_isHoldingEgg)
                    {
                        _throwDirection = GetCurrentThrowDirection();
                        _hasThrownEgg = true;
                        OnThrowEgg?.Invoke(_throwDirection);
                        SFXSystem.Stop("throw");
                        SFXSystem.Play("throw");
                    }
                    else
                    {
                        OnReadyThrowEgg?.Invoke(Position);
                    }
                    _isHoldingEgg = !_isHoldingEgg;
                }
            }
            if (GameControllerSystem.AttackPressed() && _tongueState == 0 && !_isSquating)//!_isHoldingEgg && !_isSquating// && !_isFloating && !_hasThrownEgg && !_isTurning)
            {
                if (!_isMouthing)
                {
                    _tongueLength = 0f;
                    _capturedObject = null;
                    _tongueState = TongueState.Extending;
                    SFXSystem.Play("yoshi-tongue");
                    if (_isLookingUp)
                    {
                        _tongueDirection = new Vector2(0, -1);
                    }
                    else
                    {
                        _tongueDirection = new Vector2(_lastDirection, 0);
                    }
                }
                else
                {
                    //吐出物体
                    if (_capturedObject != null)
                    {
                        if (_isLookingUp)
                        {
                            _capturedObject.Position = CenterPosition - new Vector2(0, 30);
                        }
                        else
                        {
                            _capturedObject.Position = CenterPosition + new Vector2(_lastDirection == 1 ? _capturedObject.Size.X : -5 - _capturedObject.Size.X, 0);
                            _capturedObject.Velocity = new Vector2(4f * _lastDirection, 0);
                        }
                        _capturedObject.IsCaptured = false;
                        _capturedObject.IsActive = true;
                        _capturedObject = null;
                        _isMouthing = false;
                        _isSpitting = true;
                        SFXSystem.Play("yoshi-spit");
                    }
                }
            }
            if (GameControllerSystem.MoveDown() && IsOnGround)
            {
                _isSquating = true;
            }
            else
            {
                _isSquating = false;
            }
            if(GameControllerSystem.MoveUp())
            {
                _isLookingUp = true;
            }
            else
            {
                _isLookingUp = false;
            }
            if (GameControllerSystem.JumpPressed() && !_isSquating)
            {
                Physics.ApplyJump(10f);
                SFXSystem.Play("yoshi-jump");
            }
            if (GameControllerSystem.MoveLeft() && !_isSquating && !_isLookingUp)
            {
                Physics.ApplyAcceleration(-0.5f, 5); //TODO:用常量替代数值
                currentDirection = -1;
            }
            if (GameControllerSystem.MoveRight() && !_isSquating && !_isLookingUp)
            {
                Physics.ApplyAcceleration(0.5f, 5);
                currentDirection = 1;
            }
            if (currentDirection != 0 && !_isHoldingEgg && _tongueState == 0)
            {
                _lastDirection = currentDirection;
            }
        }

        #region Animation
        private void SetYoshiAnimation(string name, bool forceSet = false, bool ignoreMouthingStatus = false)
        {
            if (!forceSet && IsYoshiAnimationEqual(name, ignoreMouthingStatus))
                return;

            string animationName = name;
            if (!ignoreMouthingStatus && _isMouthing)
            {
                animationName += "-mouthing";
            }
            _yoshiSprite.SetAnimation(animationName);
        }

        private bool IsYoshiAnimationEqual(string name, bool ignoreMouthingStatus = false)
        {
            if (_yoshiSprite.CurrentAnimation != null)
            {
                string expectedName = name;
                if (!ignoreMouthingStatus && _isMouthing)
                {
                    expectedName += "-mouthing";
                }
                return _yoshiSprite.CurrentAnimation == expectedName;
            }
            return false;
        }

        private void UpdateAnimation()
        {
            if (_isSquating)
            {
                SetYoshiAnimation("squat", false);
            }
            else
            {
                if (_isHoldingEgg)
                {
                    float absVelocityX = Math.Abs(Velocity.X);
                    if (absVelocityX < 0.2) //TODO:用常量替代数值
                        SetYoshiAnimation("hold-egg");
                    else
                        SetYoshiAnimation("hold-egg-walk");
                    return; 
                }
                if (Velocity.X != 0)
                {
                    if (Math.Abs(Velocity.X) < 2)
                        SetYoshiAnimation(_tongueState != TongueState.None ? "tongue-out-walk" : "walk");
                    else
                        SetYoshiAnimation(_tongueState != TongueState.None ? "tongue-out-run" : "run");
                }
                else
                {
                    SetYoshiAnimation(_tongueState != TongueState.None ? "tongue-out" : "stand");
                }
                if (Velocity.Y < 0)
                {
                    if (_isHoldingEgg)
                    {
                        SetYoshiAnimation("hold-egg-walk");
                    }
                    else if (_tongueState != TongueState.None)
                    {
                        if (_isLookingUp)
                            SetYoshiAnimation("tongue-out-up");
                        else
                            SetYoshiAnimation("tongue-out-jump");
                    }
                    else
                    {
                        SetYoshiAnimation("jump");
                    }
                }
                else if (Velocity.Y > 0)
                {
                    if (_isHoldingEgg)
                    {
                        SetYoshiAnimation("hold-egg-walk");
                    }
                    else if (_tongueState != TongueState.None)
                    {
                        if (_isLookingUp)
                            SetYoshiAnimation("tongue-out-up");
                        else
                            SetYoshiAnimation("tongue-out-jump");
                    }
                    else
                    {
                        SetYoshiAnimation("fall");
                    }
                }
                if (_isLookingUp && IsOnGround)
                {
                    if (_tongueState != TongueState.None)
                        SetYoshiAnimation("tongue-out-up");
                    else if (!_isSpitting)
                        SetYoshiAnimation("look-up");
                }
            }
        }

        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (CanHandleInput)
            {
                HandleInput(gameTime);
            }
            if (_tongueState != TongueState.None)
            {
                UpdateTongueState(gameTime);
            }
            Physics.ApplyPhysics(gameTime);
            UpdateAnimation();
            if (!_isHoldingEgg)
            {
                if (_lastDirection == 1)
                {
                    _yoshiSprite.Effect = SpriteEffects.None;
                }
                else if (_lastDirection == -1)
                {
                    _yoshiSprite.Effect = SpriteEffects.FlipHorizontally;
                }
            }

            if (_isHoldingEgg)
            {
                UpdateCrosshair(gameTime);
            }
            _yoshiSprite.Update(gameTime);
            
        }

        private void UpdateTongueState(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_tongueState == TongueState.Extending)
            {
                _tongueLength += TongueSpeed * elapsedTime;
                if (_tongueLength >= MaxTongueLength)
                {
                    _tongueState = TongueState.Retracting;
                }
                else
                {
                    Vector2 tongueEnd = CenterPosition + _tongueDirection * _tongueLength;
                    Rectangle tongueRect = new Rectangle((int)(tongueEnd.X - 5), (int)(tongueEnd.Y - 5), 10, 10);
                    if (IsCollidingWithTile(tongueRect, out TileCollisionResult result) && !result.TileType.HasFlag(TileType.Penetrable) && !result.TileType.HasFlag(TileType.Platform))
                    {
                        _tongueState = TongueState.Retracting;
                    }
                    else if (_capturedObject == null)
                    {
                        GameObject hitObject = GameObjectsSystem.CheckObjectCollision(tongueRect).CollidedObject;
                        if (hitObject != null && hitObject != this && hitObject.IsCapturable)
                        {
                            _capturedObject = hitObject;
                            _tongueState = TongueState.Retracting;
                        }
                    }
                }
            }
            else if (_tongueState == TongueState.Retracting)
            {
                _tongueLength -= TongueSpeed * elapsedTime;
                if (_capturedObject != null)
                {
                    _capturedObject.Position = CenterPosition + _tongueDirection * _tongueLength;
                }

                if (_tongueLength <= 0f)
                {
                    _tongueState = TongueState.None;
                    if (_capturedObject != null && _capturedObject is not IValuable)
                    {
                        _capturedObject.IsCaptured = true;
                        _capturedObject.IsActive = false;
                        _isMouthing = true;
                    }
                }
            }
        }

        private void UpdateCrosshair(GameTime gameTime)
        {
            bool centerFacingRight = _lastDirection == 1;
            if (centerFacingRight != _lastCenterFacingRight)
            {
                if (centerFacingRight)
                {
                    _currentAngle = MathHelper.Clamp(_currentAngle, 0, MaxAngleRadians);
                }
                else
                {
                    _currentAngle = MathHelper.Clamp(_currentAngle, -MaxAngleRadians, 0);
                }
                _rotationSpeed = Math.Abs(_rotationSpeed) * (centerFacingRight ? 1 : -1);
                _lastCenterFacingRight = centerFacingRight;
            }
            _currentAngle += _rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (centerFacingRight)
            {
                if (_currentAngle > MaxAngleRadians || _currentAngle < 0)
                {
                    _rotationSpeed = -_rotationSpeed;
                    _currentAngle = MathHelper.Clamp(_currentAngle, 0, MaxAngleRadians);
                }
            }
            else
            {
                if (_currentAngle < -MaxAngleRadians || _currentAngle > 0)
                {
                    _rotationSpeed = -_rotationSpeed;
                    _currentAngle = MathHelper.Clamp(_currentAngle, -MaxAngleRadians, 0);
                }
            }
            _rotatingSpritePosition = Position + new Vector2((float)Math.Sin(_currentAngle) * CrosshairRadius, -(float)Math.Cos(_currentAngle) * CrosshairRadius);
            _crosshairSprite.Update(gameTime);
        }
        #endregion

        #region Draw        
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_isHoldingEgg)
            {
                _crosshairSprite.Draw(spriteBatch, _rotatingSpritePosition, 0, Vector2.One);
            }
            _yoshiSprite.Draw(spriteBatch, Position, 0, Vector2.One);
            if (_tongueState != TongueState.None)
            {
                DrawTongue(spriteBatch);
            }
        }

        private void DrawTongue(SpriteBatch spriteBatch)
        {
            Vector2 tongueStart;
            if (_lastDirection == 1)
            {
                if (_isLookingUp)
                {
                    tongueStart = CenterPosition + new Vector2(1, -1);
                }
                else
                {
                    tongueStart = CenterPosition + new Vector2(2, -2);
                }
            }
            else
            {
                if (_isLookingUp)
                {
                    tongueStart = CenterPosition + new Vector2(-6, -1);
                }
                else
                {
                    tongueStart = CenterPosition + new Vector2(-2, 4);
                }
            }

            float rotation = (float)Math.Atan2(_tongueDirection.Y, _tongueDirection.X);
            float baseLength = _tongueLength;
            if (baseLength > 0)
            {
                Rectangle baseSource = new Rectangle(0, 0, 1, _tongueSprite.TextureRegion.Height);
                Vector2 baseScale = new Vector2(baseLength / 1, 1f);
                spriteBatch.Draw(_tongueSprite.TextureRegion.Texture, tongueStart, baseSource, Color.White, rotation, Vector2.Zero, baseScale, SpriteEffects.None, 0f);
            }

            if (_tongueLength > 3)
            {
                Rectangle tipSource = new Rectangle(1, 0, 6, _tongueSprite.TextureRegion.Height);
                Vector2 tipPosition = tongueStart + _tongueDirection * _tongueLength;
                spriteBatch.Draw(_tongueSprite.TextureRegion.Texture, tipPosition, tipSource, Color.White, rotation, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
            }
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
            Vector2 direction = new Vector2((float)Math.Sin(_currentAngle), -(float)Math.Cos(_currentAngle));
            direction.Normalize();
            return direction;
        }

        public void Bounce()
        {
            Physics.ApplyJump(15f);
        }
        #endregion
    }
}