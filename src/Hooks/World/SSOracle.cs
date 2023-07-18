using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using static Conversation;
using static SSOracleBehavior;
using Random = UnityEngine.Random;
using Action = SSOracleBehavior.Action;

namespace Pearlcat;

// the base was referenced from Dronemaster: https://github.com/HarvieSorroway/TheDroneMaster
public static partial class Hooks
{
    public static void ApplySSOracleHooks()
    {
        On.SSOracleBehavior.SeePlayer += SSOracleBehavior_SeePlayer;
        On.SSOracleBehavior.NewAction += SSOracleBehavior_NewAction;

        On.SSOracleBehavior.PebblesConversation.AddEvents += PebblesConversation_AddEvents;

        On.SSOracleBehavior.UpdateStoryPearlCollection += SSOracleBehavior_UpdateStoryPearlCollection;

        try
        {
            IL.SSOracleBehavior.Update += SSOracleBehavior_Update;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Oracle Hooks Error:\n" + e);
        }
    }

    private static void SSOracleBehavior_Update(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.After,
            x => x.MatchLdstr("Yes, help yourself. They are not edible."));

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<string, SSOracleBehavior, string>>((origText, self) =>
        {
            if (self.oracle.room.game.IsPearlcatStory())
                return self.Translate("...oh? Take them, the data they contain is worthless to me. I suppose they'd be far more useful to you...");

            return origText;
        });

        Plugin.Logger.LogWarning(c.Context);
    }


    private static void SSOracleBehavior_UpdateStoryPearlCollection(On.SSOracleBehavior.orig_UpdateStoryPearlCollection orig, SSOracleBehavior self)
    {
        if (self.oracle.room.game.IsPearlcatStory()) return;

        orig(self); // HACK
    }

    public static ConditionalWeakTable<SSOracleBehavior, SSOracleModule> SSOracleData { get; } = new();
    public static SSOracleModule GetModule(this SSOracleBehavior oracle) => SSOracleData.GetValue(oracle, x => new SSOracleModule());

    private static void PebblesConversation_AddEvents(On.SSOracleBehavior.PebblesConversation.orig_AddEvents orig, PebblesConversation self)
    {
        var module = self.owner.GetModule();
        var l = self.owner.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese ? 1 : 0;

        var id = self.id;
        var e = self.events;


        if (self.id == ID.Moon_Pearl_Misc || self.id == ID.Moon_Pearl_Misc2)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(38, Enums.SSOracle.PearlcatPebbles, true, module.Rand);
        }

        else if (id == Enums.SSOracle.Pearlcat_SSConvoFirstMeet)
        {
            e.Add(new WaitEvent(self, 160));

            e.Add(new TextEvent(self, 0,
                self.Translate(".  .  ."), 0));

            e.Add(new TextEvent(self, 0,
                self.Translate("And just who might you be?"), l * 80));
            
            e.Add(new WaitEvent(self, 80));
            


            e.Add(new TextEvent(self, 0,
                self.Translate("You can communicate with us, you can manipulate our storage medium in unforseen ways..."), l * 80));

            e.Add(new WaitEvent(self, 40));



            e.Add(new TextEvent(self, 0,
                self.Translate("Clearly an artifical creation. But from where? Sent by who?"), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("And why... to me?"), l * 80));


            e.Add(new WaitEvent(self, 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("I do not know what to make of this."), l * 80));
        }

        else if (id == Enums.SSOracle.Pearlcat_SSConvoRMPearl)
        {
            e.Add(new WaitEvent(self, 120));

            e.Add(new TextEvent(self, 0,
                self.Translate("...that pearl you carry, the purple one, where did you find it?"), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate(".  .  ."), 0));

            e.Add(new WaitEvent(self, 80));



            e.Add(new TextEvent(self, 0,
                self.Translate("It appears to contain a hymn that once meant a lot to the inhabitants of my city..."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("...one which still means a lot to me."), l * 80));


            e.Add(new TextEvent(self, 0,
                self.Translate("I have tried to reconstruct it from memory many times, fruitlessly."), l * 80));

            e.Add(new WaitEvent(self, 80));



            e.Add(new TextEvent(self, 0,
                self.Translate("And now here you are, with a perfect copy."), l * 80));

            e.Add(new WaitEvent(self, 80));


            e.Add(new TextEvent(self, 0,
                self.Translate("I suppose the only thing you value in these pearls is how pure their lattice is? I will substitute you with one much more refined than this."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("I assure you it will be a more than suitable replacement for your primitive needs."), l * 80));

            e.Add(new WaitEvent(self, 80));
        }

        else if (id == Enums.SSOracle.Pearlcat_SSConvoFirstLeave)
        {
            e.Add(new WaitEvent(self, 120));

            e.Add(new TextEvent(self, 0,
                self.Translate(".  .  ."), 0));

            e.Add(new TextEvent(self, 0,
                self.Translate("You have a knack for finding these pearls, yes?"), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("I suppose, seeing as you cannot read the information stored on them, I can read them for you, if you wish."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("Think of this mutually, I can 'see' a little further outside this can..."), l * 80));


            e.Add(new TextEvent(self, 0,
                self.Translate("...and you can gather more of that data you so clearly desire."), l * 80));


            e.Add(new WaitEvent(self, 120));

            e.Add(new TextEvent(self, 0,
                self.Translate("I will be waiting here, as always."), l * 80));

            e.Add(new WaitEvent(self, 160));
        }
        
        else
        {
            orig(self); // HACK
        }
    }

    private static void PebblesPearlIntro(this PebblesConversation self)
    {
        var module = self.owner.GetModule();
        var save = self.owner.oracle.room.game.GetMiscWorld();
        

        if (module.WasPearlAlreadyRead)
        {
            self.events.Add(new TextEvent(self, 0, self.owner.Translate("Already read this one, dummy."), 10));
        }
        else
        {
            switch (save.UniquePearlsBroughtToPebbles)
            {
                case 0:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("Something new to read... it has been too long."), 10));
                    break;

                case 1:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("And I shall read this one to you as well."), 10));
                    break;

                default:
                    switch (Random.Range(0, 4))
                    {
                        case 0:
                            self.events.Add(new TextEvent(self, 0, self.owner.Translate("I will admit, you seem to have some talent for finding these. Let's see..."), 10));
                            break;

                        case 1:
                            self.events.Add(new TextEvent(self, 0, self.owner.Translate("Ah, yet another one? Where do you find all of these?"), 10));
                            break;

                        case 2:
                            self.events.Add(new TextEvent(self, 0, self.owner.Translate("Let us see what you have found this time, wet mouse."), 10));
                            break;

                        default:
                            self.events.Add(new TextEvent(self, 0, self.owner.Translate("Something else new? Allow me to see..."), 10));
                            break;
                    }
                    break;
            }
        }
    }

    private static void SSOracleBehavior_SeePlayer(On.SSOracleBehavior.orig_SeePlayer orig, SSOracleBehavior self)
    {
        if (self.oracle.room.game.IsPearlcatStory() && self.action != Enums.SSOracle.Pearlcat_SSActionGeneral)
        {
            if (self.timeSinceSeenPlayer < 0)
                self.timeSinceSeenPlayer = 0;
            
            self.SlugcatEnterRoomReaction();
            self.NewAction(Enums.SSOracle.Pearlcat_SSActionGeneral);
            return;
        }

        orig(self); // HACK
    }

    private static void SSOracleBehavior_NewAction(On.SSOracleBehavior.orig_NewAction orig, SSOracleBehavior self, Action nextAction)
    {
        if (self.oracle.room.game.IsPearlcatStory() && self.action != Enums.SSOracle.Pearlcat_SSActionGeneral)
        {
            if (self.currSubBehavior.ID == Enums.SSOracle.Pearlcat_SSSubBehavGeneral) return;

            self.inActionCounter = 0;
            self.action = nextAction;

            var subBehavior = self.allSubBehaviors.FirstOrDefault(x => x.ID == Enums.SSOracle.Pearlcat_SSSubBehavGeneral);
            
            if (subBehavior == null)
                self.allSubBehaviors.Add(subBehavior = new SSOracleMeetPearlcat(self));

            self.currSubBehavior.Deactivate();

            subBehavior.Activate(self.action, nextAction);
            self.currSubBehavior = subBehavior;
            return;
        }
        
        orig(self, nextAction); // HACK
    }


    public class SSOracleMeetPearlcat : ConversationBehavior
    {
        public int ConvoCount { get; set; }

        public SSOracleMeetPearlcat(SSOracleBehavior owner) : base(owner, Enums.SSOracle.Pearlcat_SSSubBehavGeneral, Enums.SSOracle.Pearlcat_SSConvoFirstMeet)
        {
            var save = oracle.room.game.GetMiscWorld();
         
            if (save.PebblesMeetCount == 1)
            {
                dialogBox.NewMessage(
                    Translate("Ah. So you've returned."), 0);

                dialogBox.NewMessage(
                    Translate("Brought me something to read? Or just wasting my time, as per usual?"), 0);
            }
            else if (save.PebblesMeetCount == 2)
            {
                dialogBox.NewMessage(
                    Translate("Back again?"), 0);
            }

            save.PebblesMeetCount++;
        }

        public override void Update()
        {
            if (player == null) return;

            var module = owner.GetModule();
            var save = oracle.room.game.GetMiscWorld();

            var meetCount = save.PebblesMeetCount;

            owner.movementBehavior = MovementBehavior.KeepDistance;

            ReadPearlUpdate(module);


            if (owner.conversation != null && !owner.conversation.slatedForDeletion) return;

            module.PearlBeingRead = null;
            
            if (meetCount == 1)
            {
                if (ConvoCount == 0)
                {
                    owner.LockShortcuts();

                    owner.InitateConversation(Enums.SSOracle.Pearlcat_SSConvoFirstMeet, this);
                    ConvoCount++;
                }
                else if (ConvoCount == 1)
                {
                    if (HasRMPearl(oracle))
                        owner.InitateConversation(Enums.SSOracle.Pearlcat_SSConvoRMPearl, this);

                    ConvoCount++;
                }
                else if (ConvoCount == 2)
                {
                    owner.InitateConversation(Enums.SSOracle.Pearlcat_SSConvoFirstLeave, this);
                    ConvoCount++;
                }
                else
                {
                    owner.UnlockShortcuts();
                    owner.getToWorking = 1.0f;
                    owner.movementBehavior = MovementBehavior.Meditate;
                }
            }
            else
            {
                owner.UnlockShortcuts();
                owner.getToWorking = 1.0f;
                owner.movementBehavior = MovementBehavior.Meditate;
            }
        }

        private void ReadPearlUpdate(SSOracleModule module)
        {
            var oraclePos = oracle.firstChunk.pos;

            if (module.PearlBeingRead != null)
            {
                module.PearlBeingRead.AllGraspsLetGoOfThisObject(true);

                module.PearlBeingRead.firstChunk.HardSetPosition(oraclePos);
                owner.lookPoint = module.PearlBeingRead.firstChunk.pos;
            }

            if (owner.getToWorking != 1.0f) return;

            if (module.PearlToRead != null)
            {
                owner.LockShortcuts();
                owner.getToWorking = 0.0f;
                owner.movementBehavior = MovementBehavior.ShowMedia;

                module.PearlToRead.AllGraspsLetGoOfThisObject(true);

                var oraclePearlDir = Custom.DirVec(module.PearlToRead.firstChunk.pos, oraclePos);
                var oraclePearlDist = Custom.Dist(oraclePos, module.PearlToRead.firstChunk.pos);

                module.PearlToRead.firstChunk.vel = oraclePearlDir * Custom.LerpMap(oraclePearlDist, 200.0f, 10.0f, 10.0f, 1.0f);


                if (oraclePearlDist < 10.0f)
                {
                    StartItemConversation(module.PearlToRead, module);

                    module.PearlBeingRead = module.PearlToRead;
                    module.PearlToRead = null;
                }
            }
            else // Look for pearl to read
            {
                var roomObjects = oracle.room.physicalObjects;

                for (int i = 0; i < roomObjects.Length; i++)
                {
                    for (int j = 0; j < roomObjects[i].Count; j++)
                    {
                        var physicalObject = roomObjects[i][j];

                        if (physicalObject.grabbedBy.Count > 0) continue;

                        if (!module.PearlsHeldByPlayer.Contains(physicalObject)) continue;


                        if (physicalObject is not DataPearl dataPearl) continue;

                        if (dataPearl is PebblesPearl) continue;

                        module.PearlToRead = dataPearl;
                    }
                }
            }

            module.PearlsHeldByPlayer.Clear();

            if (player != null)
                foreach (Creature.Grasp? grasp in player.grasps)
                    if (grasp?.grabbed != null && grasp.grabbed is DataPearl pearl)
                        module.PearlsHeldByPlayer.Add(pearl);
        }

        public void StartItemConversation(DataPearl pearl, SSOracleModule module)
        {
            var save = pearl.room.game.GetMiscWorld();
            var pearlID = pearl.abstractPhysicalObject.ID.number;

            module.WasPearlAlreadyRead = save.PearlIDsBroughtToPebbles.ContainsKey(pearlID);

            var rand = Random.Range(0, 100000);

            if (!module.WasPearlAlreadyRead)
                save.PearlIDsBroughtToPebbles.Add(pearlID, rand);

            else
                rand = save.PearlIDsBroughtToPebbles[pearlID];

            module.Rand = rand;


            if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.Misc || pearl.AbstractPearl.dataPearlType.Index == -1)
                owner.InitateConversation(Conversation.ID.Moon_Pearl_Misc, this);

            else if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.Misc2)
                owner.InitateConversation(Conversation.ID.Moon_Pearl_Misc2, this);

            else if (ModManager.MSC && pearl.AbstractPearl.dataPearlType == MoreSlugcats.MoreSlugcatsEnums.DataPearlType.BroadcastMisc)
                owner.InitateConversation(MoreSlugcats.MoreSlugcatsEnums.ConversationID.Moon_Pearl_BroadcastMisc, this);

            else if (pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.PebblesPearl)
                owner.InitateConversation(Conversation.ID.Moon_Pebbles_Pearl, this);

            else
                owner.InitateConversation(DataPearlToConversation(pearl.AbstractPearl.dataPearlType), this);
        }

        public bool HasRMPearl(Oracle oracle)
        {
            foreach (var roomObject in oracle.room.physicalObjects)
            {
                foreach (var physicalObject in roomObject)
                {
                    if (physicalObject is not DataPearl dataPearl) continue;

                    var type = dataPearl.AbstractPearl.dataPearlType;

                    if (type == Enums.Pearls.RM_Pearlcat || type == MoreSlugcats.MoreSlugcatsEnums.DataPearlType.RM)
                        return true;
                }
            }

            return false;
        }

        public bool HasPearl()
        {
            return true;
        }
    }
}
