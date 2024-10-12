using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley.ItemTypeDefinitions;

namespace JojaExpress
{
    public class GUI
    {
        public static PerScreen<bool> needToCheckDialogueBox = new(), returnToHelpPage = new();
        public static List<ItemDeliver> delivers = new();
        public static PerScreen<List<string>> nameList = new();
        public static PerScreen<List<Texture2D>> itemList = new();
        public static PerScreen<List<Rectangle>> sourceRects = new();
        public static Texture2D? defaultTexture;
        public static Rectangle defaultRect = new(257, 184, 16, 16);
        public static PerScreen<int> loadAmount = new();

        public static void openMenu(string shopId, Dictionary<string, int> knownPurchased, Action<Dictionary<ISalable, ItemStockInformation>> actionOnClosed)
        {
            if (!DataLoader.Shops(Game1.content).TryGetValue(shopId, out var value)) return;
            ShopOwnerData[] source = ShopBuilder.GetCurrentOwners(value).ToArray();
            ShopOwnerData? ownerData = source.FirstOrDefault((ShopOwnerData p) => p.Type == ShopOwnerType.AnyOrNone) ?? source.FirstOrDefault((ShopOwnerData p) => p.Type == ShopOwnerType.AnyOrNone);

            CustomizedShop menu = new(shopId, value, knownPurchased, actionOnClosed);
            Game1.activeClickableMenu = menu;
        }

        public static void prepareConfigMenu()
        {
            nameList.Value = new();
            itemList.Value = new();
            sourceRects.Value = new();
            loadAmount.Value = ModEntry.config.WholeSaleIds.Length;
            if (defaultTexture is null) defaultTexture = Game1.content.Load<Texture2D>("LooseSprites\\temporary_sprites_1");

            foreach(string str in ModEntry.config.WholeSaleIds)
            {
                if (str[0] == 'I')
                {
                    ParsedItemData data = ItemRegistry.GetDataOrErrorItem(str[1..]);
                    nameList.Value.Add(data.DisplayName);
                    itemList.Value.Add(data.GetTexture());
                    sourceRects.Value.Add(data.GetSourceRect());
                }
                else
                {
                    nameList.Value.Add("Category: " + str[1..]);
                    itemList.Value.Add(defaultTexture);
                    sourceRects.Value.Add(defaultRect);
                }
            }
        }

        public static void saveConfigMenu()
        {

        }

        public static void resetConfigMenu()
        {
            ModEntry.config.WholeSaleIds = new[]
            {
                "I388", "I390", "C-74", "I176", "I174", "I180", "I182", "I178", "I442", "C-15", "C-19"
            };
            prepareConfigMenu();
        }

        public static void drawConfigMenu(SpriteBatch b, Vector2 position)
        {
            for(int i = 0; i < loadAmount.Value; i++)
            {
                b.Draw(itemList.Value[i], position + new Vector2(0, 20 + i * 80), sourceRects.Value[i], Color.White, 0f, new Vector2(8, 8), 4f, SpriteEffects.None, 0.88f);
                b.DrawString(Game1.dialogueFont, nameList.Value[i], position + new Vector2(100, i * 80), Color.Brown);
            }
        }

        public static void drawBird(object? sender, RenderedWorldEventArgs e)
        {
            for(int i = 0; i < delivers.Count; i++)
            {
                if (delivers[i].draw(e.SpriteBatch))
                {
                    delivers.RemoveAt(i);
                    i--;
                }
            }
        }

        public static void checkUI(object? sender, MenuChangedEventArgs e)
        {
            if (needToCheckDialogueBox.Value)
            {
                if (returnToHelpPage.Value) PlayerInteractionHandler.handleHelpDisplay();
                else PlayerInteractionHandler.exitMenu();
                needToCheckDialogueBox.Value = false;
                returnToHelpPage.Value = false;
            }
        }

        public static void sendPackage(IDictionary<ISalable, ItemStockInformation> package, string packageId) => delivers.Add(new(package, packageId));
    }
}
