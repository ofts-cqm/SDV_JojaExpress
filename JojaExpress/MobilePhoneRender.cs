using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace JojaExpress
{
    internal class MobilePhoneRender
    {
        public static Texture2D background, frame;
        public static string bgID = "";
        public static IMobilePhoneApi Api;
        public static IModContentHelper helper;
        public static bool old_rotated = false;
        public static List<IRenderPack> protrait = new(), landscape = new();
        public static bool splitBySpace = true;

        public static void init(IMobilePhoneApi Api)
        {
            MobilePhoneRender.Api = Api;
            helper = ModEntry._Helper.ModContent;
            background = helper.Load<Texture2D>("assets/background_dialogue_protrait");
            frame = helper.Load<Texture2D>("assets/JPad_frame");
        }

        public static void setBG(string id)
        {
            if (bgID == id) return;
            bgID = id;
            protrait.Clear();
            landscape.Clear();

            if (PlayerInteractionHandler.isJPadRunning.Value)
            {
                background = helper.Load<Texture2D>($"assets/background_{id}_landscape");
            }
            else if (Api != null)
            {
                background = helper.Load<Texture2D>($"assets/background_{id}_{(Api.GetPhoneRotated() ? "landscape" : "protrait")}");
            }
        }

        public static void JPadRender(SpriteBatch sb)
        {
            Rectangle rec = new Rectangle(43, 47, 503, 286);
            sb.Draw(frame, rec, Color.White);
            sb.Draw(background, rec, Color.White);

            foreach (IRenderPack pack in landscape)
            {
                pack.draw(sb, 43, 47);
            }
        }

        public static void render(object? sender, RenderingActiveMenuEventArgs e)
        {
            if (PlayerInteractionHandler.isJPadRunning.Value) 
            {
                JPadRender(e.SpriteBatch);
                return; 
            }

            if (Api == null || !PlayerInteractionHandler.isAppRunning.Value) return;

            if(Api.GetPhoneRotated() != old_rotated){
                old_rotated = Api.GetPhoneRotated();
                background = helper.Load<Texture2D>($"assets/background_{bgID}_{(Api.GetPhoneRotated() ? "landscape" : "protrait")}");
            }

            Rectangle rec = Api.GetPhoneRectangle();
            SpriteBatch sb = e.SpriteBatch;
            sb.Draw(background, rec, Color.White);
            List<IRenderPack> packs = Api.GetPhoneRotated() ? landscape : protrait;

            int xOff = rec.X, yOff = rec.Y;
            foreach (IRenderPack pack in packs)
            {
                pack.draw(sb, xOff, yOff);
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

        public static void drawStr(SpriteBatch b, string str, Rectangle rec, SpriteFont font, bool middle, uint color = uint.MaxValue)
        {
            // 画string的一个helper function。可以换行。返回最后一个字符的坐标
            int ypos = rec.Y;
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
                Vector2 position = new Vector2(rec.X + (middle ? (rec.Width - lastWidth) / 2 : 0), ypos);
                // 如果Y超了，罢工不画了
                if (measured.Y + ypos > rec.Y + rec.Height)
                {
                    //b.DrawString(font, currentStr, position, Color.White);
                    return;
                }
                // 如果X超了，换行
                if (measured.X > rec.Width)
                {
                    b.DrawString(font, currentStr, position, new(color));
                    ypos += (int)measured.Y;
                    currentStr = "";
                }
                currentStr += newStr[i];
                if (splitBySpace) currentStr += " ";
                lastWidth = measured.X;
            }
            // 最后画一下还没画好的
            Vector2 measured2 = font.MeasureString(currentStr);
            Vector2 position2 = new Vector2(rec.X + (middle ? (rec.Width - lastWidth) / 2 : 0), ypos);
            // 如果Y超了，罢工不画了
            if (measured2.Y + ypos <= rec.Y + rec.Height && measured2.X <= rec.Width)
            {
                b.DrawString(font, currentStr, position2, new(color));
            }
        }
    }
}
