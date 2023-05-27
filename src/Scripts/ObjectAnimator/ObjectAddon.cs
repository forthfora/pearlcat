using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Pearlcat
{
    public class ObjectAddon : UpdatableAndDeletable, IDrawable
    {
        public static ConditionalWeakTable<PhysicalObject, ObjectAddon> ObjectsWithAddon = new();

        public readonly WeakReference<AbstractPhysicalObject> AbstractObject;

        public ObjectAddon(AbstractPhysicalObject abstractObject)
        {
            AbstractObject = new WeakReference<AbstractPhysicalObject>(abstractObject);

            ObjectsWithAddon.Add(abstractObject.realizedObject, this);
            abstractObject.realizedObject.room.AddObject(this);
        }



        public override void Update(bool eu)
        {
            base.Update(eu);

            if (slatedForDeletetion)
                RemoveFromRoom();

            if (!AbstractObject.TryGetTarget(out var abstractObject) || abstractObject.slatedForDeletion
                || abstractObject.realizedObject == null || abstractObject.realizedObject.slatedForDeletetion)
            {
                Destroy();
            }
        }



        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            foreach (FSprite sprite in sLeaser.sprites)
            {
                sprite.RemoveFromContainer();
                rCam.ReturnFContainer("Midground").AddChild(sprite);
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }



        PhysicalObject? parent;
        FSprite? parentSprite;

        public void ParentGraphics_DrawSprites(PhysicalObject self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            parent = self;
            parentSprite = sLeaser.sprites.FirstOrDefault();
        }



        public bool drawHalo = false;
        public int haloSprite;

        public float haloScale = 0.75f;
        public float haloAlpha = 0.5f;
        public Color haloColor = Color.white;


        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (!AbstractObject.TryGetTarget(out var abstractObject)) return;

            if (parent == null || parentSprite == null) return;

            if (slatedForDeletetion || rCam.room != room || parent.room != room)
            {
                sLeaser.CleanSpritesAndRemove();
                return;
            }



            // Halo
            FSprite halo = sLeaser.sprites[haloSprite];
            halo.isVisible = drawHalo;

            halo.isVisible = true;
            halo.SetPosition(parentSprite.GetPosition());

            halo.scale = haloScale;
            halo.alpha = haloAlpha;
            halo.color = haloColor;
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            int spriteIndex = 0;

            // Assign Sprite Indexes
            haloSprite = spriteIndex++;


            // Create Sprites
            sLeaser.sprites[haloSprite] = new FSprite("LizardBubble6", true);


            AddToContainer(sLeaser, rCam, null!);
        }
    }
}
