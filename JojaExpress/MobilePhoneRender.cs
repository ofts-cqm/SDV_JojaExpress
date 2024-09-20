using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace JojaExpress
{
    internal class MobilePhoneRender
    {
        public static Texture2D background;
        public static string bgID = "";
        public static IMobilePhoneApi Api;
        public static IModContentHelper helper;
        public static bool old_rotated = false;
        public static List<RenderPack> protrait = new(), landscape = new();
        public static bool splitBySpace = true;

        public static void init(IMobilePhoneApi Api)
        {
            MobilePhoneRender.Api = Api;
            helper = ModEntry._Helper.ModContent;
            background = helper.Load<Texture2D>("assets/background_dialogue_protrait");
        }

        public static void setBG(string id)
        {
            bgID = id;
            background = helper.Load<Texture2D>($"assets/background_{id}_{(Api.GetPhoneRotated() ? "landscape" : "protrait")}");
            protrait.Clear();
            landscape.Clear();
        }

        public static void render(object? sender, RenderingActiveMenuEventArgs e)
        {
            if (Api == null || !PlayerInteractionHandler.isAppRunning.Value) return;

            if(Api.GetPhoneRotated() != old_rotated){
                old_rotated = Api.GetPhoneRotated();
                background = helper.Load<Texture2D>($"assets/background_{bgID}_{(Api.GetPhoneRotated() ? "landscape" : "protrait")}");
            }

            Rectangle rec = Api.GetPhoneRectangle();
            SpriteBatch sb = e.SpriteBatch;
            sb.Draw(background, rec, Color.White);
            List<RenderPack> packs = Api.GetPhoneRotated() ? landscape : protrait;

            int xOff = rec.X, yOff = rec.Y;
            foreach (RenderPack pack in packs)
            {
                drawStr(sb, pack.displayStr, pack.rec, pack.font, pack.middle, xOff, yOff);
            }
        }

        public static string[] charToStr(string charArr)
        {
            string[] strs = new string[charArr.Length];
            for(int i = 0; i < charArr.Length; i++)
            {
                strs[i] = charArr[i] + ""; 
            }
            return strs;
        }

        public static void drawStr(SpriteBatch b, string str, Rectangle rec, SpriteFont font, bool middle, int xOff, int yOff)
        {
            // 画string的一个helper function。可以换行。返回最后一个字符的坐标
            int ypos = rec.Y + yOff;
            float lastWidth = 0f;
            string[] newStr = splitBySpace ? str.Split(" ") : charToStr(str);
            string currentStr = "";
            // 遍历
            for (int i = 0; i < newStr.Length; i++)
            {
                // 获取字符宽度
                string appended = currentStr + newStr[i];
                if (splitBySpace) appended += " ";
                Vector2 measured = font.MeasureString(appended);
                // 如果Y超了，罢工不画了
                if (measured.Y + ypos > rec.Y + rec.Height + yOff) return;
                // 如果X超了，换行
                if (measured.X > rec.Width)
                {
                    Vector2 position = new Vector2(rec.X + xOff + (middle ? (rec.Width - lastWidth) / 2 : 0), ypos);
                    b.DrawString(font, currentStr, position, Color.White);
                    ypos += (int)measured.Y;
                    currentStr = "";
                }
                currentStr += newStr[i];
                if (splitBySpace) currentStr += " ";
                lastWidth = measured.X;
            }
            // 最后画一下还没画好的
            Vector2 position2 = new Vector2(rec.X + xOff + (middle ? (rec.Width - lastWidth) / 2 : 0), ypos);
            b.DrawString(font, currentStr, position2, Color.White);
        }
    }

    public class RenderPack
    {
        public string displayStr;
        public Rectangle rec;
        public bool middle;
        public SpriteFont font;

        public RenderPack(string displayStr, int x, int y, int width, int height, SpriteFont? font, bool middle = false)
        {
            if (font == null) font = Game1.smallFont;
            this.displayStr = displayStr;
            this.rec = new Rectangle(x, y, width, height);
            this.middle = middle;
            this.font = font;
        }
    }
}
