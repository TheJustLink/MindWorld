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

        public Game(IEngine engine) => _engine = engine;

        public void Tick(ElapsedTime time)
        {
            if (_initialized == false)
                Initialize();

            _ship.Tick(time);
        }

        private void Initialize()
        {
            _ship = new Ship(_engine.Input, _engine);
            _ship.InitializeAsync().Wait();

            _initialized = true;
        }
    }
}