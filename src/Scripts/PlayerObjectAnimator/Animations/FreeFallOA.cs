
namespace Pearlcat;

public class FreeFallOA : ObjectAnimation
{
    public FreeFallOA(Player player) : base(player) { }


    public override void Update(Player player)
    {
        base.Update(player);

        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        for (int i = 0; i < playerModule.Inventory.Count; i++)
        {
            AbstractPhysicalObject abstractObject = playerModule.Inventory[i];

            if (abstractObject.realizedObject == null) continue;
            var realizedObject = abstractObject.realizedObject;

            if (!realizedObject.abstractPhysicalObject.TryGetModule(out var playerObjectModule)) continue;

            realizedObject.gravity = 1.0f;
            realizedObject.CollideWithTerrain = true;

            playerObjectModule.PlayCollisionSound = true;
        }
    }
}
