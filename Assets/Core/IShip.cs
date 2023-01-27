using LinkEngine.Components;
using LinkEngine.Ticks;

namespace MindWorld.Core
{
    interface IShip : ITickable
    {
        ITransform2D Transform { get; }
    }
}