using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace JojaExpress.PhoneScreen
{
    public abstract class PhoneScreenBase
    {
        public static IModContentHelper Loader => ModEntry._Helper.ModContent;
        public static readonly Color BackgroundColor = new(137, 182, 255);
        public PhoneScreenBase? father;

        public abstract void drawLandscape(SpriteBatch b, int xOff, int yOff);
        public abstract void drawProtrait(SpriteBatch b, int xOff, int yOff);
        public abstract PhoneScreenBase? click(int x, int y);

        public virtual void hover(int x, int y) { }
        public virtual void onClose() { }

        public static void drawLandscapeBackground(SpriteBatch b, int x, int y, bool drawLogo = false)
        {
            b.Draw(Game1.staminaRect, new Rectangle(x + 37, y + 13, 429, 260), BackgroundColor);
            if (drawLogo)
            {
                b.DrawString(Game1.smallFont, "Joja", new Vector2(x + 42, y + 246), Color.White);
            }
        }

        public static void drawProtraitBackground(SpriteBatch b, int x, int y, bool drawLogo = false)
        {
            b.Draw(Game1.staminaRect, new Rectangle(x + 13, y + 37, 260, 429), BackgroundColor);
            if (drawLogo)
            {
                b.DrawString(Game1.smallFont, "Joja", new Vector2(x + 15, y + 434), Color.White);
            }
        }
    }
}
