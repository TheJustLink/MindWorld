using System;
using System.Diagnostics;
using System.Threading.Tasks;

using LinkEngine.Assets;
using LinkEngine.Components;
using LinkEngine.Engines;
using LinkEngine.GameObjects;
using LinkEngine.Graphics;
using LinkEngine.IO;
using LinkEngine.Math;
using LinkEngine.Time;

namespace MindWorld.Core
{
    class PlayerShip : IShip
    {
        public ITransform2D Transform { get; private set; }

        private const float Speed = 4f;
        private const float RotationSpeedInDegrees = 270f;

        private readonly IInput<Vector2> _input;
        private readonly IEngine _engine;

        private bool _initialized;
        //private Vector2 _inputDirection;
        private ISpriteRenderer _spriteRenderer;

        public PlayerShip(IInput<Vector2> input, IEngine engine)
        {
            _input = input;
            _engine = engine;
        }

        public async Task InitializeAsync()
        {
            var sw = Stopwatch.StartNew();

            var gameObjectTask = CreateGameObjectAsync();
            var spriteTask = GetSpriteAsync();

            await Task.WhenAll(gameObjectTask, spriteTask).ConfigureAwait(false);

            var elapsedTime = sw.ElapsedMilliseconds;
            var gameObject = gameObjectTask.Result;
            var sprite = spriteTask.Result;

            _engine.Logger.Write($"Game object {gameObject} created for {elapsedTime}ms");
            _engine.Logger.Write($"Sprite {spriteTask.Result} loaded for {elapsedTime}ms");
            _engine.Logger.Write($"Ship initialized for {elapsedTime}ms");

            sw.Stop();
            
            Transform = await gameObject.Components.AddAsync<ITransform2D>().ConfigureAwait(false);
            
            _spriteRenderer = await gameObject.Components.AddAsync<ISpriteRenderer>().ConfigureAwait(false);
            _spriteRenderer.Sprite = sprite;
            //_spriteRenderer.Color = Color.Black;

            _initialized = true;
        }
        private Task<IGameObject> CreateGameObjectAsync()
        {
            return _engine.GameObjectFactory.CreateAsync();
        }
        private Task<ISprite> GetSpriteAsync()
        {
            return _engine.AssetProvider.GetAsync<ISprite>("Ship/Sprite1");
        }

        private Vector2 _targetPosition;
        private float _targetRotation;

        public void Tick(ElapsedTime time)
        {
            if (_initialized == false) return;

            //var rawInput = _input.Read();
            //var targetInputDirection = rawInput == Vector2.Zero
            //    ? Vector2.Zero
            //    : Vector2.Normalize(rawInput);

            //_inputDirection = Vector2.Lerp(_inputDirection, targetInputDirection, time.DeltaSeconds * 10f);

            Vector2 direction = Vector2.Zero;

            if (_engine.Mouse.RightButton.IsDown)
            {
                _targetPosition = _engine.Mouse.PositionInWorld;

                direction = Transform.Position.GetDirectionTo(_targetPosition);

                _targetPosition -= direction * Vector2.Half;

                _targetRotation = MathF.Atan2(direction.Y, direction.X) - MathFunctions.Deg90InRad;
            }

            Transform.Position = Vector2.Lerp(Transform.Position, _targetPosition, Speed * time.DeltaSeconds);
            Transform.RotationInRadians = Interpolations.LinearAngleInRadians(Transform.RotationInRadians, _targetRotation, MathFunctions.DegToRad * RotationSpeedInDegrees * Speed * time.DeltaSeconds);

            var changeScaleX = Math.Clamp(MathF.Abs(direction.X * 0.2f), 0f, 1f);
            var changeScaleY = Math.Clamp(MathF.Abs(direction.Y * 0.2f), 0f, 1f);
            var targetScale = new Vector2(1 + changeScaleX, 1 + changeScaleY);
            
            Transform.LocalScale = Vector2.Lerp(Transform.LocalScale, targetScale, 10f * time.DeltaSeconds);

            _spriteRenderer.Color = Color.Lerp(Color.Black, Color.White, (1 - MathF.Sin(time.TotalSeconds)) / 2f);
        }
    }
}