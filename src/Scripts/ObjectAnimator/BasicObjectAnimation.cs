using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheSacrifice
{
    internal class BasicObjectAnimation : ObjectAnimation
    {
        public override void Update(Player self)
        {
            base.Update(self);

            if (!Hooks.PlayerData.TryGetValue(self, out var playerModule)) return;

            UpdateHaloEffects(self);

            for (int i = 0; i < playerModule.abstractInventory.Count; i++)
            {
                AbstractPhysicalObject abstractObject = playerModule.abstractInventory[i];

                if (i == playerModule.activeObjectIndex)
                    MoveToTargetPos(abstractObject, GetActiveObjectPos(self));
            }
        }
    }
}
