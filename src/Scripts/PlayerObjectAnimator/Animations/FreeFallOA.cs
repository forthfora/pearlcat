
namespace Pearlcat;

public class FreeFallOA : ObjectAnimation
{
    public FreeFallOA(Player player) : base(player) { }


    public override void Update(Player player)
    {
        base.Update(player);

        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        for (int i = 0; i < playerModule.abstractInventory.Count; i++)
        {
            AbstractPhysicalObject abstractObject = playerModule.abstractInventory[i];

            abstractObject.realizedObject.gravity = 1.0f;
            abstractObject.realizedObject.CollideWithTerrain = true;
        }
    }
}
