using System.Threading.Tasks;

using LinkEngine.Engines;
using LinkEngine.Ticks;
using LinkEngine.Time;

using MindWorld.Core;

namespace MindWorld
{
    class Game : ITickable
    {
        private readonly IEngine _engine;
        private readonly IShip[] _ships = new IShip[10];

        private bool _initialized;
        private bool _isInitializing;
        
        public Game(IEngine engine) => _engine = engine;
        
        public void Tick(ElapsedTime time)
        {
            if (_isInitializing) return;
            
            if (_initialized == false)
            {
                Initialize();
            }
            else
            {
                foreach (var ship in _ships)
                    ship.Tick(time);
            }
        }

        private async void Initialize()
        {
            _isInitializing = true;

            var tasks = new Task[_ships.Length];

            var playerShip = new PlayerShip(_engine.Input, _engine);
            _ships[0] = playerShip;
            tasks[0] = playerShip.InitializeAsync();

            for (var i = 1; i < _ships.Length; i++)
            {
                var ship = new AIShip(_engine, _ships[i - 1]);

                _ships[i] = ship;
                tasks[i] = ship.InitializeAsync();
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            _initialized = true;
            _isInitializing = false;
        }
    }
}