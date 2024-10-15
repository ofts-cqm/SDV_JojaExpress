using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace JojaExpress
{
    public class GUI
    {
        public static PerScreen<bool> needToCheckDialogueBox = new(), returnToHelpPage = new();
        public static List<ItemDeliver> delivers = new();
        public static PerScreen<ConfigMenu> menus = new();

        public static void prepareConfigMenu() => menus.Value = new();

        public static void saveConfigMenu() => ModEntry.config.WholeSaleIds = menus.Value.rawIDs.ToArray();

        public static void closeConfigMenu() => menus.Value = null;

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
            menus.Value.xPositionOnScreen = (int)position.X;
            menus.Value.yPositionOnScreen = (int)position.Y;
            menus.Value.height = menus.Value.loadAmount * 80 + 100;
            menus.Value.draw(b);
            menus.Value.performHoverAction(Game1.getMouseX(), Game1.getMouseY());
        }

        public static int loadAmount() => menus.Value.loadAmount;

        public static void openMenu(string shopId, Dictionary<string, int> knownPurchased, Action<Dictionary<ISalable, ItemStockInformation>> actionOnClosed)
        {
            if (!DataLoader.Shops(Game1.content).TryGetValue(shopId, out var value)) return;
            ShopOwnerData[] source = ShopBuilder.GetCurrentOwners(value).ToArray();
            ShopOwnerData? ownerData = source.FirstOrDefault((ShopOwnerData p) => p.Type == ShopOwnerType.AnyOrNone) ?? source.FirstOrDefault((ShopOwnerData p) => p.Type == ShopOwnerType.AnyOrNone);

            CustomizedShop menu = new(shopId, value, knownPurchased, actionOnClosed);
            Game1.activeClickableMenu = menu;
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
