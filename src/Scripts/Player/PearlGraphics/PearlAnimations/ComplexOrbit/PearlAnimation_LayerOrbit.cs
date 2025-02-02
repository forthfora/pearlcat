namespace Pearlcat;

public sealed class PearlAnimation_LayerOrbit(Player player) : PearlAnimation_OrbitBase(player)
{
    public const float RADIUS_5 = 30.0f;
    public const float RADIUS_3 = 20.0f;
    public const float RADIUS_2 = 10.0f;

    public const float F_ADDITION_5 = -0.03f;
    public const float F_ADDITION_3 = 0.06f;
    public const float F_ADDITION_2 = -0.1f;

    public override void Update(Player player)
    {
        base.Update(player);

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        UpdateOrbitPearls(playerModule);

        var headPos = ((PlayerGraphics)player.graphicsModule).head.pos;

        AnimateOrbit(player, headPos, RADIUS_5, F_ADDITION_5, OrbitPearls_Rest);
        AnimateOrbit(player, headPos, RADIUS_3, F_ADDITION_3, OrbitPearls_3);
        AnimateOrbit(player, headPos, RADIUS_2, F_ADDITION_2, OrbitPearls_2);

        playerModule.ActivePearl?.TryAnimateToTargetPos(player, player.GetActivePearlPos());
    }
}
