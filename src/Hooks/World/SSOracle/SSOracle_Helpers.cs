namespace Pearlcat;

public static class SSOracle_Helpers
{
    public static bool IsPebbles(this SSOracleBehavior? behavior)
    {
        return behavior?.oracle?.IsPebbles() ?? false;
    }
    public static bool IsPebbles(this Oracle? oracle)
    {
        return oracle?.ID == Oracle.OracleID.SS;
    }
}
