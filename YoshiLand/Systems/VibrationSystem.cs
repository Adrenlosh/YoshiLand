using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace YoshiLand.Systems
{
    public static class VibrationSystem
    {
        public static bool IsVibrating
        {
            get => field;
            set
            {
                field = value;
                if (!value)
                    GamePad.SetVibration(_playerIndex, 0f, 0f);
            }
        }
        private static float _timer = 0f;
        private static float _duration = 0f;
        private static readonly PlayerIndex _playerIndex = PlayerIndex.One;

        public static void SetVibration(float leftMotor, float rightMotor, float duration = 0.2f)
        {
            GamePad.SetVibration(_playerIndex, leftMotor, rightMotor);
            IsVibrating = true;
            _duration = duration;
            _timer = 0;
        }

        public static void Update(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _timer += elapsedTime;
            if(_timer >= _duration && IsVibrating)
            {
                IsVibrating = false;
                _timer = 0f;
            }
        }
    }
}