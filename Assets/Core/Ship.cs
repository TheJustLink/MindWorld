﻿using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;

using LinkEngine.Assets;
using LinkEngine.Components;
using LinkEngine.Engines;
using LinkEngine.GameObjects;
using LinkEngine.IO;
using LinkEngine.Ticks;
using LinkEngine.Time;

namespace MindWorld
{
    class Ship : ITickable
    {
        private const float Speed = 10f;

        private readonly IInput<Vector2> _input;
        private readonly IEngine _engine;

        private ITransform2D _transform;

        private bool _initialized;
        private Vector2 _inputDirection;

        public Ship(IInput<Vector2> input, IEngine engine)
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

            _engine.Logger.Write($"Game object {gameObjectTask.Result} created for {elapsedTime}ms");
            _engine.Logger.Write($"Sprite {spriteTask.Result} loaded for {elapsedTime}ms");
            _engine.Logger.Write($"Ship initialized for {elapsedTime}ms");

            sw.Stop();

            _transform = gameObjectTask.Result.Transform;

            _initialized = true;
        }
        private Task<IGameObject> CreateGameObjectAsync()
        {
            return _engine.GameObjectFactory.CreateAsync();
        }
        private Task<ISprite> GetSpriteAsync()
        {
            return _engine.AssetProvider.GetAsync<ISprite>("Ship/Sprite");
        }

        public void Tick(ElapsedTime time)
        {
            if (_initialized == false) return;

            var rawInput = _input.Read();
            var targetInputDirection = rawInput == Vector2.Zero
                ? Vector2.Zero
                : Vector2.Normalize(rawInput);

            _inputDirection = Vector2.Lerp(_inputDirection, targetInputDirection, time.DeltaSeconds * 10f);

            var targetPosition = _transform.Position + _inputDirection;
            _transform.Position = Vector2.Lerp(_transform.Position, targetPosition, Speed * time.DeltaSeconds);

            var changeScaleX = Math.Clamp(MathF.Abs(_inputDirection.X * 0.2f), 0f, 1f);
            var changeScaleY = Math.Clamp(MathF.Abs(_inputDirection.Y * 0.2f), 0f, 1f);
            var targetScale = new Vector2(1 + changeScaleX, 1 + changeScaleY);

            _transform.LocalScale = Vector2.Lerp(_transform.LocalScale, targetScale, 10f * time.DeltaSeconds);
        }
    }
}