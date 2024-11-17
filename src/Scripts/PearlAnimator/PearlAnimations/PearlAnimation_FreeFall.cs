
namespace Pearlcat;

public class PearlAnimation_FreeFall(Player player) : PearlAnimation(player)
{
    public override void Update(Player player)
    {
        base.Update(player);

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        foreach (var abstractObject in playerModule.Inventory)
        {
            if (ModOptions.HidePearls.Value && abstractObject != playerModule.ActiveObject)
            {
                continue;
            }

            if (abstractObject.realizedObject == null)
            {
                continue;
            }

            var realizedObject = abstractObject.realizedObject;

            if (!realizedObject.abstractPhysicalObject.TryGetPlayerPearlModule(out var pearlModule))
            {
                continue;
            }

            realizedObject.gravity = 1.0f;
            realizedObject.CollideWithTerrain = true;

            pearlModule.PlayCollisionSound = true;
        }
    }
}
