using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace JojaExpress
{
    public class JOLNMenu: IClickableMenu
    {
        public readonly Rectangle WindowBorderSourceRect = new(18, 0, 18, 18);

        public int tick = 0;
        public TemporaryAnimatedSprite screen;
        public Dictionary<ISalable, ItemStockInformation> books = new();
        public bool proceed = false, learningCrafting = false;
        public Texture2D menuTexture;
        public string notification = "";

        public JOLNMenu(Dictionary<ISalable, ItemStockInformation> purchased)
        {
            foreach (KeyValuePair<ISalable, ItemStockInformation> item in purchased)
            {
                if (item.Key is Item obj)
                {
                    if (obj.Category == -102 || obj.Category == -103) books.Add(obj, item.Value);
                    else
                    {
                        string key = obj.Name.Substring(0, obj.Name.IndexOf("Recipe") - 1);
                        if (obj.Category == -7)
                        {
                            Game1.player.cookingRecipes.Add(key, 0);
                            proceed = true;
                        }
                        else
                        {
                            Game1.player.craftingRecipes.Add(key, 0);
                            learningCrafting = true;
                        }
                    }
                }
            }

            if(!learningCrafting && !proceed)
            {
                exitThisMenu();
                GUI.sendPackage(books, "_ofts.jojaExp.item.package.joln");
                return;
            }

            width = 42 * 8;
            height = 28 * 8;
            xPositionOnScreen = (Game1.uiViewport.Width - width) / 2;
            yPositionOnScreen = (Game1.uiViewport.Height - height) / 2;
            tick = 0;
            menuTexture = ModEntry._Helper.ModContent.Load<Texture2D>("assets/menu");

            if (learningCrafting) displayCraftingLecture();
            else displayCookingLecture();
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            xPositionOnScreen = (Game1.uiViewport.Width - width) / 2;
            yPositionOnScreen = (Game1.uiViewport.Height - height) / 2;
            screen.position = new Vector2(xPositionOnScreen, yPositionOnScreen);
        }

        public void displayCraftingLecture() 
        {
            //screen = new(new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height), )
            screen = new TemporaryAnimatedSprite(
                "LooseSprites\\Cursors", new Rectangle(517, 361, 42, 28), 150f, 2, 999999,
                new Vector2(xPositionOnScreen, yPositionOnScreen), false, false, 0.88f, 0f,
                Color.White, 8f, 0f, 0f, 0f);
            notification = ModEntry._Helper.Translation.Get("learningCrafting");
        }

        public void displayCookingLecture() 
        {
            screen = new TemporaryAnimatedSprite(
                "LooseSprites\\Cursors", new Rectangle(602, 361, 42, 28), 150f, 2, 999999, 
                new Vector2(xPositionOnScreen, yPositionOnScreen), false, false, 0.88f, 0f, 
                Color.White, 8f, 0f, 0f, 0f);
            notification = ModEntry._Helper.Translation.Get("learningCooking");
            proceed = false;
            learningCrafting = false;
        }

        public override bool readyToClose() => false;

        public override void draw(SpriteBatch b)
        {
            tick++;
            if(tick == 180)
            {
                if (proceed)
                {
                    tick = 0;
                    Game1.addHUDMessage(new HUDMessage(ModEntry._Helper.Translation.Get("newCrafting"), HUDMessage.achievement_type));
                    displayCookingLecture();
                }
                else
                {
                    if (learningCrafting) Game1.addHUDMessage(new HUDMessage(ModEntry._Helper.Translation.Get("newCrafting"), HUDMessage.achievement_type));
                    else Game1.addHUDMessage(new HUDMessage(ModEntry._Helper.Translation.Get("newCooking"), HUDMessage.achievement_type));
                    exitThisMenu();
                    if (books.Count != 0) GUI.sendPackage(books, "_ofts.jojaExp.item.package.joln");
                    return;
                }
            }

            drawTextureBox(b, menuTexture, WindowBorderSourceRect, xPositionOnScreen - 16, yPositionOnScreen - 66, width + 32, height + 82, Color.White, 4f);
            b.DrawString(Game1.dialogueFont, notification, new Vector2(xPositionOnScreen, yPositionOnScreen - 50), Color.Blue);
            screen.update(Game1.currentGameTime);
            screen.draw(b, true);
        }
    }
}
