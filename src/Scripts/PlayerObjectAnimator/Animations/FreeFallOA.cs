
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

            if (abstractObject.realizedObject == null) continue;
            var realizedObject = abstractObject.realizedObject;

            if (!Hooks.PlayerObjectData.TryGetValue(realizedObject, out var playerObjectModule)) continue;


            realizedObject.gravity = 1.0f;
            realizedObject.CollideWithTerrain = true;

            playerObjectModule.playCollisionSound = true;
        }
    }
}
