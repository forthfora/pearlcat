
using MoreSlugcats;
using System.Runtime.CompilerServices;

namespace Pearlcat;

public static partial class Hooks 
{
    public static ConditionalWeakTable<Player, PearlpupModule> PearlpupData { get; } = new();

    public static bool TryGetPearlpupModule(this Player pup, out PearlpupModule module)
    {
        if (!pup.IsPearlpup())
        {
            module = null!;
            return false;
        }

        if (!PearlpupData.TryGetValue(pup, out module))
        {
            module = new PearlpupModule(pup);
            PearlpupData.Add(pup, module);
        }

        return true;
    }

    public static bool IsPearlpup(this Player pup) => pup.abstractCreature.Room.world.game.IsPearlcatStory() && pup.abstractCreature.Room.world.game.GetMiscWorld()?.PearlpupID == pup.abstractCreature.ID.number;

    public static void MakePearlpup(this Player pup)
    {
        if (pup.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Slugpup) return;

        if (IsPearlpup(pup)) return;

        var save = pup.abstractCreature.world.game.GetMiscWorld();

        if (save == null) return;

        if (save.PearlpupID != null) return;

        save.PearlpupID = pup.abstractCreature.ID.number;
    }
}
