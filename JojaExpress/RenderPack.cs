using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData.HomeRenovations;

namespace JojaExpress
{
    public interface IRenderPack
    {
        void draw(SpriteBatch sb, int xOff, int yOff);
    }

    public class TexturedRenderPack : IRenderPack
    {
        Func<ISalable?> texture;
        int x, y;
        float scale;
        bool drawShadow;

        public TexturedRenderPack(Func<ISalable?> texture, int x, int y, float scale = 1, bool drawShadow = false) 
        {
            this.texture = texture;
            this.x = x;
            this.y = y;
            this.scale = scale;
            this.drawShadow = drawShadow;
        }

        public void draw(SpriteBatch sb, int xOff, int yOff)
        {
            texture.Invoke()?.drawInMenu(sb, new Vector2(x + xOff, y + yOff), scale, 1f,0.9f, StackDrawType.Hide, Color.White, drawShadow);
        }
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

        public VolatileRenderPack(Func<ISalable?> displayStr, int x, int y, int width, int height, SpriteFont? font)
        {
            this.displayStr = () => displayStr.Invoke()?.DisplayName ?? "---------";
            if (font == null) font = Game1.smallFont;
            this.x = x;
            this.y = y;
            this.w = width;
            this.h = height;
            this.font = font;
            this.middle = false;
        }

        public VolatileRenderPack(Func<ItemStockInformation?> displayStr, int x, int y, int width, int height, SpriteFont? font)
        {
            this.displayStr = () => displayStr.Invoke()?.Price.ToString() ?? "-----";
            if (font == null) font = Game1.smallFont;
            this.x = x;
            this.y = y;
            this.w = width;
            this.h = height;
            this.font = font;
            this.middle = false;
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
