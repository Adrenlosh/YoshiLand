using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Screens.Transitions;
using System;

namespace YoshiLand.Transitions
{
    public class TransitionTimer
    {
        private readonly float _halfDurationInOut;
        private float _currentSeconds;
        private TransitionState _previousState;

        public TransitionState State { get; private set; } = TransitionState.Out;
        public float Duration { get; }
        public float Value => _halfDurationInOut <= 0f ? 0f : MathHelper.Clamp(_currentSeconds / _halfDurationInOut, 0f, 1f);

        public event EventHandler StateChanged;
        public event EventHandler Completed;

        public TransitionTimer(float duration)
        {
            Duration = duration;
            _halfDurationInOut = Duration / 2f;
        }

        public void Update(GameTime gameTime)
        {
            var elapsedSeconds = gameTime.GetElapsedSeconds();

            switch (State)
            {
                case TransitionState.Out:
                    {
                        float next = _currentSeconds + elapsedSeconds;
                        if (next >= _halfDurationInOut)
                        {
                            _currentSeconds = _halfDurationInOut;
                            State = TransitionState.In;
                            StateChanged?.Invoke(this, EventArgs.Empty);
                        }
                        else
                        {
                            _currentSeconds = next;
                        }
                    }
                    break;

                case TransitionState.In:
                    {
                        float next = _currentSeconds - elapsedSeconds;
                        if (next <= 0f)
                        {
                            _currentSeconds = 0f;
                            State = TransitionState.Out;
                            Completed?.Invoke(this, EventArgs.Empty);
                        }
                        else
                        {
                            _currentSeconds = next;
                        }
                    }
                    break;

                case TransitionState.Keep:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Reset()
        {
            _currentSeconds = 0f;
            State = TransitionState.Out;
        }

        public void Start()
        {
            Reset();
        }

        public void Pause()
        {
            _previousState = State;
            State = TransitionState.Keep;
        }

        public void Resume()
        {
            if (State == TransitionState.Keep)
                State = _previousState;
        }
    }
}