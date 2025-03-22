namespace Pearlcat;

public abstract class PearlAnimation_SineBase(Player player) : PearlAnimation(player)
{
    protected virtual float YPeriod => 0.0f;

    public override void Update(Player player)
    {
        base.Update(player);

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        var floatingObjects = new List<AbstractPhysicalObject>();
        floatingObjects.AddRange(playerModule.Inventory);

        var activePearl = playerModule.ActivePearl;

        if (activePearl is not null)
        {
            floatingObjects.Remove(activePearl);
            activePearl.TryAnimateToTargetPos(player, player.GetActivePearlPos());
        }

        for (var i = 0; i < floatingObjects.Count; i++)
        {
            var abstractObject = floatingObjects[i];

            var targetPos = Vector2.zero;

            var spacing = 10.0f;

            targetPos.x = player.firstChunk.pos.x + spacing * i - floatingObjects.Count / 2.0f * spacing + (spacing / 2.0f);
            targetPos.y = player.firstChunk.pos.y + 20.0f * Mathf.Sin(AnimTimer / 30.0f + i * (YPeriod / floatingObjects.Count));

            abstractObject.TryAnimateToTargetPos(player, targetPos);
        }
    }
}
