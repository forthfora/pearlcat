using System.Linq;
using MoreSlugcats;

namespace Pearlcat;

public static partial class Utils
{
    // Player Check
    public static bool IsPearlcat(this Player player)
    {
        return player.SlugCatClass == Enums.Pearlcat;
    }

    public static bool IsFirstPearlcat(this Player player)
    {
        return player.playerState.playerNumber == GetFirstPearlcatIndex(player.room?.game);
    }


    // Pearlpup Check
    public static bool IsPearlpup(this AbstractCreature crit) => crit.Room?.world?.game?.IsPearlcatStory() == true && crit.Room.world.game.GetMiscWorld()?.PearlpupID == crit.ID.number;

    public static bool IsPearlpup(this Player player) => player.abstractCreature.IsPearlpup();


    // Misc
    public static int GraspsHasType(this Player self, AbstractPhysicalObject.AbstractObjectType type)
    {
        for (int i = 0; i < self.grasps.Length; i++)
        {
            Creature.Grasp? grasp = self.grasps[i];

            if (grasp == null) continue;

            if (grasp.grabbed.abstractPhysicalObject.type == type)
                return i;
        }

        return -1;
    }

    public static bool IsHostileToMe(this Creature self, Creature creature)
    {
        // trust no one, not even yourself?
        if (creature == self)
        {
            return false;
        }

        if (creature is Player pup && pup.IsPearlpup())
        {
            return false;
        }

        // Possessed Creature
        if (self is Player && creature.abstractCreature.controlled)
        {
            return false;
        }

        var AI = creature.abstractCreature.abstractAI?.RealAI;

        // Player vs Aggressive Creature
        if (self is Player && AI is LizardAI or ScavengerAI or BigNeedleWormAI or DropBugAI or CicadaAI or InspectorAI)
        {
            var aggression = AI.CurrentPlayerAggression(self.abstractCreature);

            var rep = AI.tracker.RepresentationForCreature(self.abstractCreature, false);

            if (rep?.dynamicRelationship == null)
                return false;

            return AI switch
            {
                LizardAI => aggression > 0.0f,
                ScavengerAI => aggression > 0.5f,
                BigNeedleWormAI => aggression > 0.0f,
                CicadaAI => aggression > 0.0f,
                DropBugAI => true,
                InspectorAI => aggression > 0.0f,
                _ => false
            };
        }

        // Player vs Player
        if (self is Player && creature is Player player2 && !player2.isSlugpup)
        {
            var game = self.abstractCreature.world.game;

            if (game.IsArenaSession && game.GetArenaGameSession.GameTypeSetup.spearsHitPlayers)
            {
                return true;
            }
        }

        var myRelationship = self.abstractCreature.creatureTemplate.CreatureRelationship(self.abstractCreature.creatureTemplate);
        var creatureRelationship = creature.abstractCreature.creatureTemplate.CreatureRelationship(self.abstractCreature.creatureTemplate);

        return myRelationship.GoForKill || creatureRelationship.GoForKill;
    }

    public static bool InDeathPit(this Player self)
    {
        var belowRoom = self.mainBodyChunk.pos.y < -300.0f;

        var roomHasWater = self.room.water && !self.room.waterInverted && self.room.defaultWaterLevel >= -10;

        var deadOrStunned = self.dead || self.Stunned;

        var canFly = self.Template.canFly;

        var roomHasDeathPit = self.room.deathFallGraphic is not null;

        return roomHasDeathPit && belowRoom && !roomHasWater && (deadOrStunned || !canFly);
    }


    // Revive
    public static void TryRevivePlayer(this Player self, PlayerModule playerModule)
    {
        if (playerModule.ReviveCount <= 0) return;

        if (self.room == null) return;

        //if (self.room == null || self.graphicsModule == null) return;

        //if (self.killTag?.creatureTemplate is CreatureTemplate template
        //    && (template.type == CreatureTemplate.Type.DaddyLongLegs || template.type == CreatureTemplate.Type.BrotherLongLegs
        //    || template.type == CreatureTemplate.Type.BigEel || template.type == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs)) return;

        self.AllGraspsLetGoOfThisObject(true);

        self.room.DeflectEffect(self.firstChunk.pos);
        playerModule.ShieldTimer = 200;

        if (self.dead)
            self.RevivePlayer();

        else
            self.room.ReviveEffect(self.firstChunk.pos);

        playerModule.SetReviveCooldown(-1);
    }

    public static void RevivePlayer(this Player self)
    {
        self.Revive();

        self.abstractCreature.Room.world.game.cameras.First().hud.textPrompt.gameOverMode = false;
        self.playerState.permaDead = false;
        self.playerState.alive = true;

        self.exhausted = false;
        self.airInLungs = 1.0f;
        self.aerobicLevel = 0.0f;

        self.bodyMode = Player.BodyModeIndex.Default;
        self.animation = Player.AnimationIndex.None;

        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        playerModule.PickObjectAnimation(self);
    }

    public static void Revive(this Creature self)
    {
        //self.graphicsModule?.ReleaseAllInternallyContainedSprites();

        if (self.State is HealthState healthState)
        {
            healthState.health = 1.0f;
        }

        self.State.alive = true;

        self.dead = false;
        self.killTag = null;
        self.killTagCounter = 0;
        self.abstractCreature.abstractAI?.SetDestination(self.abstractCreature.pos);

        if (self is not Player)
        {
            self.Stun(100);
        }

        self.room.ReviveEffect(self.mainBodyChunk.pos);
    }
}
