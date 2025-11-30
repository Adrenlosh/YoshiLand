using YoshiLand.GameObjects;

namespace YoshiLand.Interfaces
{
    public interface IProjectile
    {
        int Damage { get; }

        GameObject Owner { get; }
    }
}