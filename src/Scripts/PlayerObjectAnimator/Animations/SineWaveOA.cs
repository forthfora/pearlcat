using System.Collections.Generic;

namespace Pearlcat;

public class SineWaveOA : ObjectAnimation
{
    public readonly List<float> floatOffsets = new(); 

    public SineWaveOA(Player player) : base(player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule)) return;
    }

    public override void Update(Player player)
    {
        base.Update(player);

        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        var floatingObjects = new List<AbstractPhysicalObject>();
        floatingObjects.AddRange(playerModule.abstractInventory);

        var activeObject = playerModule.ActiveObject;

        if (activeObject != null)
        {
            floatingObjects.Remove(activeObject);
            activeObject.MoveToTargetPos(player, GetActiveObjectPos(player));
        }
        

    }
}
