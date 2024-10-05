﻿using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

namespace JojaExpress
{
    public class GUI
    {
        /*
        public static Rectangle[] boxes = new Rectangle[] {
            new(48, 96, 24, 24),
            new(72, 96, 24, 24),
            new(96, 96, 24, 24),
        };
        public static int tick = 0, index = 0;
        public static PerScreen<int> absTick = new();
        public static Texture2D birdTexture;
        public static PerScreen<bool> showAnimation = new(), droped = new();
        public static PerScreen<Vector2> target = new(), current = new();
        public static PerScreen<GameLocation> targetLocation = new();*/
        public static PerScreen<bool> needToCheckDialogueBox = new(), returnToHelpPage = new();
        public static List<ItemDeliver> delivers = new();

        public static void openMenu(string shopId, Dictionary<string, int> knownPurchased, Action<Dictionary<string, int>> actionOnClosed)
        {
            if (!DataLoader.Shops(Game1.content).TryGetValue(shopId, out var value)) return;

            ShopOwnerData[] source = ShopBuilder.GetCurrentOwners(value).ToArray();
            ShopOwnerData? ownerData = source.FirstOrDefault((ShopOwnerData p) => p.Type == ShopOwnerType.AnyOrNone) ?? source.FirstOrDefault((ShopOwnerData p) => p.Type == ShopOwnerType.AnyOrNone);

            CustomizedShop menu = new(shopId, value, knownPurchased, actionOnClosed);
            Game1.activeClickableMenu = menu;
        }

        [Obsolete]
        public static string getPostFixForItem(ISalable item)
        {
            if (ModEntry.tobeReceived.Last().TryGetValue(item.QualifiedItemId, out int amt))
                return ModEntry.postfix.Tokens(new Dictionary<string, int>() { { "count", amt } }).ToString();
            else return "";
        }

        [Obsolete]
        public static string getPostFixForLocalItem(ISalable item)
        {
            if (ModEntry.localReceived.TryGetValue(item.QualifiedItemId, out int amt))
                return ModEntry.postfix.Tokens(new Dictionary<string, int>() { { "count", amt } }).ToString();
            else return "";
        }

        public static string getPostFixForItemOnPhone(ISalable item)
        {
            if (ModEntry.tobeReceived.Last().TryGetValue(item.QualifiedItemId, out int amt))
                return "x" + amt;
            else return "x 0";
        }

        public static string getPostFixForLocalItemOnPhone(ISalable item)
        {
            if (ModEntry.localReceived.TryGetValue(item.QualifiedItemId, out int amt))
                return "x" + amt;
            else return "x 0";
        }

        /*[Obsolete]
        public static void dropPackage(SpriteBatch b)
        {
            tick++;
            absTick.Value++;
            if (tick % 10 == 0) { index++; index %= 3; }
            if (tick == 60)
            {
                StardewValley.Object obj = new("ofts.jojaExp.item.package.local", 1);
                foreach (KeyValuePair<string, int> p in ModEntry.localReceived)
                {
                    obj.modData.Add(p.Key, p.Value.ToString());
                }
                targetLocation.Value.debris.Add(Game1.createItemDebris(obj, target.Value, 0));
            }
            if (tick == 120) droped.Value = true;

            if (Game1.currentLocation == targetLocation.Value)
                b.Draw(birdTexture,
                new Vector2(current.Value.X - Game1.viewport.X, current.Value.Y - Game1.viewport.Y
                + (float)(Math.Sin(absTick.Value / 10) * 16)),
                boxes[index], Color.White, 0, Vector2.Zero, 4f, SpriteEffects.None, 2.8f);
        }*/

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

        public static void sendPackage(IDictionary<string, int> package) => delivers.Add(new(package));
    }
}
