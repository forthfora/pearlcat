using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using System;
using static Conversation;
using static SSOracleBehavior;
using Random = UnityEngine.Random;

namespace Pearlcat;

public static class SSOracleConversation_Helpers
{
    public static void PebblesPearlIntro(this PebblesConversation self)
    {
        var module = self.owner.GetModule();
        var save = self.owner.oracle.room.game.GetMiscWorld();

        if (save == null) return;


        if (module.WasPearlAlreadyRead)
        {
            switch (Random.Range(0, 4))
            {
                case 0:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("I already read this one. I can read it again, I suppose."), 10));
                    break;

                case 1:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("This one again? I have better things to do... but..."), 10));
                    break;

                case 2:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("Let us see what- oh, again?"), 10));
                    break;

                default:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("I remember this one... don't you? Well..."), 10));
                    break;
            }
        }
        else
        {
            switch (save.UniquePearlsBroughtToPebbles)
            {
                case 1:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("Something new to read... it has been too long..."), 10));
                    break;

                case 2:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("Another? And I shall read this one to you as well..."), 10));
                    break;

                case 3:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("And a third? So it is..."), 10));
                    break;

                case 4:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("Four! Well, if you insist..."), 10));
                    break;

                case 5:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("So curious..."), 10));
                    break;

                default:
                    switch (Random.Range(0, 4))
                    {
                        case 0:
                            self.events.Add(new TextEvent(self, 0, self.owner.Translate("Another one? I shouldn't be surprised. Let's see..."), 10));
                            break;

                        case 1:
                            self.events.Add(new TextEvent(self, 0, self.owner.Translate("Ah, yet another one? You are even better at finding these than I imagined..."), 10));
                            break;

                        case 2:
                            self.events.Add(new TextEvent(self, 0, self.owner.Translate("Read this one too? Yes, yes, here it is..."), 10));
                            break;

                        default:
                            self.events.Add(new TextEvent(self, 0, self.owner.Translate("Something else new? Allow me to see..."), 10));
                            break;
                    }
                    break;
            }
        }
    }
}
