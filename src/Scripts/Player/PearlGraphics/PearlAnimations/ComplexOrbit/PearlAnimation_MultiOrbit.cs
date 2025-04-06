namespace Pearlcat;

public sealed class PearlAnimation_MultiOrbit(Player player) : PearlAnimation_OrbitBase(player)
{
    public readonly Vector2 OrbitPearls_5_Offset = new(-25.0f, 7.0f);
    public readonly Vector2 OrbitPearls_3_Offset = new(25.0f, 25.0f);
    public readonly Vector2 OrbitPearls_2_Offset = new(23.0f, -10.0f);

    public const float RADIUS_5 = 15.0f;
    public const float RADIUS_3 = 10.0f;
    public const float RADIUS_2 = 7.5f;

    public const float F_ADDITION_5 = 0.04f;
    public const float F_ADDITION_3 = -0.08f;
    public const float F_ADDITION_2 = 0.16f;

    public override void Update(Player player)
    {
        base.Update(player);

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        UpdateOrbitPearls(playerModule);

        var headPos = ((PlayerGraphics)player.graphicsModule).head.pos;

        AnimateOrbit(player, headPos + OrbitPearls_5_Offset, RADIUS_5, F_ADDITION_5, OrbitPearls_Rest);
        AnimateOrbit(player, headPos + (OrbitPearls_Rest.Count == 0 ? OrbitPearls_5_Offset : OrbitPearls_3_Offset), RADIUS_3, F_ADDITION_3, OrbitPearls_3);
        AnimateOrbit(player, headPos + OrbitPearls_2_Offset, RADIUS_2, F_ADDITION_2, OrbitPearls_2);

        playerModule.ActivePearl?.TryAnimateToTargetPos(player, player.GetActivePearlPos());
    }
}
