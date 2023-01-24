using LinkEngine.Engines;
using LinkEngine.Ticks;
using LinkEngine.Time;

namespace MindWorld
{
    class Game : ITickable
    {
        private readonly IEngine _engine;

        private Ship? _ship;

        private bool _initialized;
        private bool _isInitializing;
        
        public Game(IEngine engine) => _engine = engine;
        
        public void Tick(ElapsedTime time)
        {
            if (_isInitializing) return;
            
            if (_initialized == false)
                Initialize();
            else _ship.Tick(time);
        }

        private async void Initialize()
        {
            _isInitializing = true;

            _ship = new Ship(_engine.Input, _engine);
            await _ship.InitializeAsync().ConfigureAwait(false);

            _initialized = true;
            _isInitializing = false;
        }
    }
}