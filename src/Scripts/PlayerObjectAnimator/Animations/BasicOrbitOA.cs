using System.Collections.Generic;

namespace Pearlcat;

public class BasicOrbitOA : ObjectAnimation
{
    public BasicOrbitOA(Player player) : base(player) { }


    public const float ANGLE_FRAME_ADDITION = 0.02f;
    public const float RADIUS = 25.0f;

    public override void Update(Player player)
    {   
        base.Update(player);

        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        var orbitObjects = new List<AbstractPhysicalObject>();
        orbitObjects.AddRange(playerModule.Inventory);

        var activeObject = playerModule.ActiveObject;
        
        if (activeObject != null)
        {
            orbitObjects.Remove(activeObject);
            activeObject.MoveToTargetPos(player, GetActiveObjectPos(player));
        }

        AnimateOrbit(player, ((PlayerGraphics)player.graphicsModule).head.pos, RADIUS, ANGLE_FRAME_ADDITION, orbitObjects);
    }
}
