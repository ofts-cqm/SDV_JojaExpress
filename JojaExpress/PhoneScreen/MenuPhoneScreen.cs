using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace JojaExpress.PhoneScreen
{
    internal class MenuPhoneScreen : PhoneScreenBase
    {
        public string[] shops, texts;
        public ClickableTextureComponent? close;
        public ClickableTextureComponent[] buttons = new ClickableTextureComponent[6];
        public string title;

        public static Dictionary<string, Action<Dictionary<ISalable, ItemStockInformation>>> actions = new()
        {
            { "jojaLocal", (purchased) => GUI.sendPackage(purchased, "_ofts.jojaExp.item.package.local") },
            { "jojaGlobal", (purchased) => ModEntry.needMail = true },
            { "JojaWhole", (purchased) => GUI.sendPackage(purchased, "_ofts.jojaExp.item.package.whole") },
            { "joja.joln", (purchased) => Game1.activeClickableMenu = new JOLNMenu(purchased) }
        };

        public MenuPhoneScreen(string title, string[] texts, string[] shops, PhoneScreenBase? father, bool drawClose) 
        {
            this.shops = shops;
            this.texts = texts;
            this.title = title;
            this.father = father;
            if (drawClose) close = new(new(0, 0, 32, 32), Loader.Load<Texture2D>("assets\\phone"), new(0, 0, 16, 16), 2f);
            for(int i = 0; i < 6; i++)
                buttons[i] = new(new(0, 0, 100, 64), Loader.Load<Texture2D>("assets\\phone"), new(16, 0, 25, 16), 4f);
        }

        public override PhoneScreenBase? click(int x, int y)
        {
            if (close?.containsPoint(x, y) ?? false) return father;

            string? action = null;            
            for(int i = 0; i < 6; i++)
            {
                if (buttons[i].containsPoint(x, y))
                {
                    action = shops[i];
                    break;
                }
            }
            if (action == null) return this;

            if (action == "#back") return father;
            ITranslationHelper trans = ModEntry._Helper.Translation;
            if (action == "#help")
            {
                string[] shops = new string[] { "?local", "?global", "?qi", "?whole", "?joln", "#back"};
                string[] texts = new[] { trans.Get("local"), trans.Get("global"), trans.Get("qi"), trans.Get("whole"), trans.Get("joln"), Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel") };
                return new MenuPhoneScreen(trans.Get("app.help"), texts, shops, this, false);
            }
            if (action == "$qi")
            {
                if (Utility.TryOpenShopMenu("QiGemShop", "AnyOrNone"))
                {
                    Game1.activeClickableMenu.exitFunction = PlayerInteractionHandler.exitMenu;
                }
                return new ShopPhoneScreen();
            }
            if (action.StartsWith('$'))
            {
                GUI.openMenu("ofts.JojaExp." + action[1..], new(), actions[action[1..]]);
                return new ShopPhoneScreen();
            }
            return new DialoguePhoneScreen(trans.Get(action[1..] + "_help"), this);
        }

        public override void hover(int x, int y)
        {
            close?.tryHover(x, y);
            foreach (ClickableTextureComponent button in buttons) button.tryHover(x, y);
        }

        public override void drawLandscape(SpriteBatch b, int xOff, int yOff)
        {
            drawLandscapeBackground(b, xOff, yOff, true);
            if (close is not null)
            {
                close.bounds.X = xOff + 430;
                close.bounds.Y = yOff + 230;
                close.draw(b);
            }
            MobilePhoneRender.drawStr(b, title, new(55 + xOff, 35 + yOff, 150, 90), Game1.dialogueFont, false);
            for (int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 2; j++)
                {
                    ClickableTextureComponent button = buttons[i * 2 + j];
                    button.bounds.X = 230 + xOff + 110 * j;
                    button.bounds.Y = 30 + yOff + 70 * i;
                    button.draw(b);
                    //MobilePhoneRender.drawMiddleStr(b, texts[i * 2 + j], new(235 + xOff + 110 * j, 35 + yOff + 70 * i, 90, 60), Game1.smallFont);
                }
            }
        }

        public override void drawProtrait(SpriteBatch b, int xOff, int yOff)
        {
            drawProtraitBackground(b, xOff, yOff, true);
            if (close is not null)
            {
                close.bounds.X = xOff + 230;
                close.bounds.Y = yOff + 430;
                close.draw(b);
            }
            MobilePhoneRender.drawStr(b, title, new(30 + xOff, 60 + yOff, 250, 120), Game1.dialogueFont, false);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    ClickableTextureComponent button = buttons[i * 2 + j];
                    button.bounds.X = 32 + xOff + 110 * j;
                    button.bounds.Y = 214 + yOff + 70 * i;
                    button.draw(b);
                    //MobilePhoneRender.drawMiddleStr(b, texts[i * 2 + j], new(30 + xOff + 110 * j, 210 + yOff + 70 * i, 104, 72), Game1.smallFont);
                }
            }
        }
    }
}
