using System;
using System.Diagnostics;
using System.Threading.Tasks;

using LinkEngine.Assets;
using LinkEngine.Components;
using LinkEngine.Engines;
using LinkEngine.GameObjects;
using LinkEngine.Graphics;
using LinkEngine.Math;
using LinkEngine.Time;

namespace MindWorld.Core
{
    class AIShip : IShip
    {
        public ITransform2D Transform { get; private set; }

        private const float Speed = 10f;
        private const float RotationSpeedInDegrees = 270f;
        
        private readonly IEngine _engine;
        private readonly IShip _target;

        private bool _initialized;
        private Vector2 _inputDirection;
        private ISpriteRenderer _spriteRenderer;

        public AIShip(IEngine engine, IShip target)
        {
            _engine = engine;
            _target = target;
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
            Transform.Position = new Vector2(2f, 2f);

            _spriteRenderer = await gameObject.Components.AddAsync<ISpriteRenderer>().ConfigureAwait(false);
            _spriteRenderer.Sprite = sprite;

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
            if (_target == null) return;
            
            _targetPosition = _target.Transform.Position;

            var direction = Transform.Position.GetDirectionTo(_targetPosition);

            _targetPosition -= direction * Vector2.Half;

            _targetRotation = MathF.Atan2(direction.Y, direction.X) - MathFunctions.Deg90InRad;

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