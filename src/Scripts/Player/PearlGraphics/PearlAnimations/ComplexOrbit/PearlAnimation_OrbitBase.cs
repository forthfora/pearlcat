namespace Pearlcat;

public abstract class PearlAnimation_OrbitBase : PearlAnimation
{
    // num = amount
    protected List<AbstractPhysicalObject> OrbitPearls_2 { get; } = [];
    protected List<AbstractPhysicalObject> OrbitPearls_3 { get; } = [];
    protected List<AbstractPhysicalObject> OrbitPearls_Rest { get; } = [];

    public PearlAnimation_OrbitBase(Player player) : base(player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        UpdateOrbitPearls(playerModule);
    }

    protected void UpdateOrbitPearls(PlayerModule playerModule)
    {
        OrbitPearls_2.Clear();
        OrbitPearls_3.Clear();
        OrbitPearls_Rest.Clear();

        foreach (var item in playerModule.Inventory)
        {
            if (item == playerModule.ActivePearl)
            {
                continue;
            }

            if (OrbitPearls_2.Count < 2)
            {
                OrbitPearls_2.Add(item);
            }
            else if (OrbitPearls_3.Count < 3)
            {
                OrbitPearls_3.Add(item);
            }
            else
            {
                OrbitPearls_Rest.Add(item);
            }
        }
    }
}
