using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace JojaExpress
{
    public interface IRenderPack
    {
        void draw(SpriteBatch sb, int xOff, int yOff);
    }

    public class VolatileRenderPack : IRenderPack
    {
        public Func<string> displayStr;
        public int x, y, w, h;
        public SpriteFont font;
        public bool middle;

        public VolatileRenderPack(Func<string> displayStr, int x, int y, int width, int height, SpriteFont? font, bool middle = false) { 
            if (font == null) font = Game1.smallFont;
            this.displayStr = displayStr;
            this.x = x;
            this.y = y;
            this.w = width;
            this.h = height;
            this.font = font;
            this.middle = middle;
        }

        public void draw(SpriteBatch sb, int xOff, int yOff)
        {
            string str = displayStr.Invoke();
            MobilePhoneRender.drawStr(sb, str, new Rectangle(x + xOff, y + yOff, w, h), font, middle);
        }
    }

    public class RenderPack : IRenderPack
    {
        public string displayStr;
        public int x, y, w, h;
        public SpriteFont font;
        public bool middle;

        public RenderPack(string displayStr, int x, int y, int width, int height, SpriteFont? font, bool middle = false)
        {
            if (font == null) font = Game1.smallFont;
            this.displayStr = displayStr;
            this.x = x;
            this.y = y;
            this.w = width;
            this.h = height;
            this.font = font;
            this.middle = middle;
        }

        public void draw(SpriteBatch sb, int xOff, int yOff)
        {
            MobilePhoneRender.drawStr(sb, displayStr, new Rectangle(x + xOff, y + yOff, w, h), font, middle);
        }
    }
}
