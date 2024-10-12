using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace JojaExpress
{
    public class ItemDeliver
    {
        public static Texture2D birdTexture;
        public static readonly Rectangle[] boxes = new Rectangle[] {
            new(48, 96, 24, 24),
            new(72, 96, 24, 24),
            new(96, 96, 24, 24),
        };

        public IDictionary<ISalable, ItemStockInformation> toBeDelivered;
        public GameLocation targetLocation;
        public Vector2 target, current;
        public int tick = 0, absTick = 0, index = 0;
        public bool droped = false;
        public string packageId;

        public ItemDeliver(IDictionary<ISalable, ItemStockInformation> toBeDelivered, string packageId)
        {
            this.toBeDelivered = toBeDelivered;
            targetLocation = Game1.currentLocation;
            target = new(Game1.player.Position.X - Game1.viewport.X, Game1.player.Position.Y - Game1.tileSize - Game1.viewport.Y);
            current = new Vector2(Game1.viewport.Width + 300, target.Y);
            birdTexture ??= ModEntry._Helper.GameContent.Load<Texture2D>("LooseSprites\\parrots");
            this.packageId = packageId;
        }

        public bool draw(SpriteBatch b)
        {
            if (current.X < target.X && !droped)
            {
                dropPackage(b);
                return false;
            }

            tick++;
            absTick++;
            if (tick >= 10) { tick = 0; index++; index %= 3; }

            if (Game1.currentLocation == targetLocation)
                b.Draw(birdTexture,
                    new Vector2(current.X, current.Y
                + (float)(Math.Sin(absTick / 10) * 16)),
                    boxes[index], Color.White, 0, Vector2.Zero, 4f, SpriteEffects.None, 2.8f);

            current.X -= 6.4f;

            if (current.X < -24)
            {
                if (droped) return true;
                targetLocation.debris.Add(Game1.createItemDebris(new PackedItem(packageId, toBeDelivered), new Vector2(target.X + Game1.viewport.X, target.Y + Game1.viewport.Y), 0));
                droped = true;
            }
            return false;
        }

        public void dropPackage(SpriteBatch b)
        {
            tick++;
            absTick++;
            if (tick % 10 == 0) { index++; index %= 3; }
            if (tick == 60) 
                targetLocation.debris.Add(Game1.createItemDebris(new PackedItem(packageId, toBeDelivered), new Vector2(target.X + Game1.viewport.X, target.Y + Game1.viewport.Y), 0));
            
            if (tick == 120) droped = true;

            if (Game1.currentLocation == targetLocation)
                b.Draw(birdTexture,
                new Vector2(current.X, current.Y
                + (float)(Math.Sin(absTick / 10) * 16)),
                boxes[index], Color.White, 0, Vector2.Zero, 4f, SpriteEffects.None, 2.8f);
        }
    }
}
