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

    public static bool IsPearlpup(this Player player) => player.abstractCreature.IsPearlpup();
    public static bool IsPearlpup(this AbstractCreature crit) => crit.Room.world.game.IsPearlcatStory() && crit.Room.world.game.GetMiscWorld()?.PearlpupID == crit.ID.number;

    public static void MakePearlpup(this AbstractCreature crit)
    {
        if (crit.creatureTemplate.type != MoreSlugcatsEnums.CreatureTemplateType.SlugNPC) return;

        if (IsPearlpup(crit)) return;

        var save = crit.world.game.GetMiscWorld();

        if (save == null) return;

        if (save.PearlpupID != null) return;

        save.PearlpupID = crit.ID.number;
    }
}
