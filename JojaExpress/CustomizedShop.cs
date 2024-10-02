using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.Menus;

namespace JojaExpress
{
    public partial class CustomizedShop : IClickableMenu
    {
        //public List<KeyValuePair<ISalable, ItemStockInformation>> forSale, purchased, currentList;
        Dictionary<ISalable, ItemStockInformation> forSale, purchased;
        View currentList;
        public ShopData shopData;
        public Action<List<KeyValuePair<ISalable, ItemStockInformation>>> actionOnClosed;
        public Rectangle scrollBarRunner;
        public int currentItemIndex = 0, hoverPrice = -1, safetyTimer = 250;
        public TextBox searchBox;
        public int totalMoney { get; set; } = 0;
        public bool scrolling = false, viewingCart = false, viewingNotification;
        public string shopId, hoverText = "", boldTitleText = "", searchNone, cartNone, checkOutStr, backStr, cartStr, totalStr, searchEmptyStr;
        public ISalable? hoveredItem;
        public MoneyDial totalMoneyDial = new(8);
        public Dictionary<string, int> knownPurchased;

        public CustomizedShop(string shopId, ShopData data, Dictionary<string, int> knownPurchased, 
            Action<List<KeyValuePair<ISalable, ItemStockInformation>>> actionOnClosed)
        {
            this.shopData = data;
            this.shopId = shopId;
            this.actionOnClosed = actionOnClosed;
            this.knownPurchased = knownPurchased;
            PriceTexture = ModEntry._Helper.ModContent.Load<Texture2D>("assets/price");
            notificationTexture = ModEntry._Helper.ModContent.Load<Texture2D>("assets/notification");

            forSale = new();
            purchased = new();
            _fillStockInfo(ShopBuilder.GetShopStock(shopId, shopData));
            currentList = new(forSale);
            InitClickableComponents();
            _updatePosition();
            Game1.player.forceCanMove();
            Game1.playSound(openMenuSound);
            if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                populateClickableComponentList();
                snapToDefaultClickableComponent();
            }
            _setScrollBarToCurrentIndex();

            searchBox = new(ModEntry._Helper.ModContent.Load<Texture2D>("assets/searchBoxTexture"), null, Game1.dialogueFont, Color.Aqua)
            {
                X = searchTab.bounds.X + 20,
                Y = searchTab.bounds.Y,
                Width = searchTab.bounds.Width - 150,
                Height = 44,
                Selected = false
            };
            searchBox.OnEnterPressed += (box) => updateSearchBox();

            ITranslationHelper trans = ModEntry._Helper.Translation;
            searchNone = trans.Get("search.none", new { search = searchBox.Text });
            cartNone = trans.Get("cart.none");
            cartStr = trans.Get("view_cart");
            backStr = trans.Get("view_shop");
            checkOutStr = trans.Get("checkout");
            totalStr = trans.Get("totalMoney");
            searchEmptyStr = trans.Get("emptySearch");

            totalMoneyDial.currentValue = 0;
            exitFunction = PlayerInteractionHandler.exitMenu;
        }

        public void updateSearchBox()
        {
            ITranslationHelper trans = ModEntry._Helper.Translation;
            searchNone = trans.Get("search.none", new { search = searchBox.Text });
            currentList.filter(searchBox.Text);
            searchBox.Selected = false;
        }

        public void checkout_exit()
        {
            Game1.player.Money -= totalMoney;
            foreach (var p in purchased)
            {
                if (knownPurchased.ContainsKey(p.Key.QualifiedItemId))
                {
                    knownPurchased[p.Key.QualifiedItemId] = p.Value.Stock;
                }
                else knownPurchased.Add(p.Key.QualifiedItemId, p.Value.Stock);
                Game1.player.team.synchronizedShopStock.OnItemPurchased(shopId, p.Key, forSale, p.Value.Stock);
            }
            actionOnClosed.Invoke(purchased.ToList());
            exitThisMenu();
        }

        public void purchaseItem(int index)
        {
            if (!forSale.TryGetValue(currentList[index], out var stock)) return;

            int amount = _getPurchaseAmount(index);
            ISalable salable = currentList[index];
            totalMoney += amount * stock.Price;
            if (stock.Stock != int.MaxValue) 
            {
                stock.Stock -= amount;
                forSale[salable] = stock;
            }

            if (purchased.ContainsKey(salable))
            {
                ItemStockInformation stock2 = purchased[salable];
                stock2.Stock += amount;
                purchased[salable] = stock2;
            }
            else purchased.Add(salable, new ItemStockInformation(stock.Price, amount));
            
            if (stock.Stock == 0) currentList.RemoveAt(index);
        }

        public void sellItem(int index)
        {
            if (!purchased.TryGetValue(currentList[index], out var stock)) return;

            int amount = _getPurchaseAmount(index);
            ISalable salable = currentList[index];
            totalMoney -= amount * stock.Price;
            stock.Stock -= amount;
            purchased[salable] = stock;

            if (forSale.ContainsKey(salable))
            {
                ItemStockInformation stock2 = forSale[salable];

                if (stock2.Stock != int.MaxValue) 
                {
                    stock2.Stock += amount;
                    forSale[salable] = stock2; 
                }
            }
            else forSale.Add(salable, new ItemStockInformation(stock.Price, amount));

            if (stock.Stock == 0) currentList.RemoveAt(index);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds) => _updatePosition();
    }
}
