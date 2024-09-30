using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewValley.Menus;

namespace JojaExpress
{
    public partial class CustomizedShop: IClickableMenu
    {
        //public List<KeyValuePair<ISalable, ItemStockInformation>> forSale, purchased, currentList;
        Dictionary<ISalable, ItemStockInformation> forSale, purchased;
        View currentList;
        public ShopData shopData;
        public Action<List<KeyValuePair<ISalable, ItemStockInformation>>> actionOnClosed;
        public ClickableComponent[] forSaleButtons = new ClickableComponent[4];
        public ClickableComponent searchTab, moneyTab;
        public ClickableTextureComponent upArrow, downArrow, scrollBar, cartButton, checkOutButton, backButton, search, unSearch;
        public ClickableTextureComponent[] priceBG = new ClickableTextureComponent[4], 
            pricePlus = new ClickableTextureComponent[4], 
            priceMin = new ClickableTextureComponent[4];
        public Texture2D MenuTexture = Game1.mouseCursors, PriceTexture;
        public Rectangle scrollBarRunner;
        public int currentItemIndex = 0, totalMoney = 0, hoverPrice = -1, safetyTimer = 250;
        public bool scrolling = false, viewingCart = false;
        public string shopId, hoverText = "", boldTitleText = "";
        public ISalable? hoveredItem;

        public CustomizedShop(string shopId, ShopData data, Dictionary<string, int> knownPurchased, 
            Action<List<KeyValuePair<ISalable, ItemStockInformation>>> actionOnClosed)
        {
            this.shopData = data;
            this.shopId = shopId;
            this.actionOnClosed = actionOnClosed;
            PriceTexture = ModEntry._Helper.ModContent.Load<Texture2D>("assets/price");

            forSale = new();
            purchased = new();
            _fillStockInfo(ShopBuilder.GetShopStock(shopId, shopData), knownPurchased);
            currentList = new(forSale.ToList());
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
        }

        public void purchaseItem(int index)
        {
            if (!forSale.TryGetValue(currentList[index].Key, out var stock)) return;

            int amount = _getPurchaseAmount(index);
            ISalable salable = currentList[index].Key;
            totalMoney += amount * stock.Price;
            if (stock.Stock != int.MaxValue) stock.Stock -= amount;

            if (purchased.ContainsKey(salable))
            {
                ItemStockInformation stock2 = purchased[salable];
                stock2.Stock += amount;
            }
            else purchased.Add(salable, new ItemStockInformation(stock.Price, amount));
            Game1.player.team.synchronizedShopStock.OnItemPurchased(shopId, salable, forSale, amount);
            if (stock.Stock == 0) currentList.RemoveAt(index);
        }

        public void sellItem(int index)
        {
            if (!purchased.TryGetValue(currentList[index].Key, out var stock)) return;

            int amount = _getPurchaseAmount(index);
            ISalable salable = currentList[index].Key;
            totalMoney -= amount * stock.Price;
            stock.Stock -= amount;

            if (forSale.ContainsKey(salable))
            {
                ItemStockInformation stock2 = purchased[salable];
                if (stock2.Stock == 0) stock2.Stock += amount;
            }
            else forSale.Add(salable, new ItemStockInformation(stock.Price, amount));
            Game1.player.team.synchronizedShopStock.OnItemPurchased(shopId, salable, forSale, -amount);
            if (stock.Stock == 0) currentList.RemoveAt(index);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds) => _updatePosition();

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            hoverText = "";
            hoveredItem = null;
            hoverPrice = -1;
            boldTitleText = "";
            upArrow.tryHover(x, y);
            downArrow.tryHover(x, y);
            scrollBar.tryHover(x, y);
            cartButton.tryHover(x, y);
            checkOutButton.tryHover(x, y);
            backButton.tryHover(x, y);
            search.tryHover(x, y);
            unSearch.tryHover(x, y);
            
            if (scrolling) return;

            for (int i = 0; i < forSaleButtons.Length; i++)
            {
                pricePlus[i].tryHover(x, y);
                priceMin[i].tryHover(x, y);
                if (currentItemIndex + i < currentList.Count && forSaleButtons[i].containsPoint(x, y))
                {
                    ISalable salable = currentList[currentItemIndex + i].Key;
                    hoverText = salable.getDescription();
                    boldTitleText = salable.DisplayName;
                    hoverPrice = currentList[currentItemIndex + i].Value.Price;
                    hoveredItem = salable;
                    forSaleButtons[i].scale = Math.Min(forSaleButtons[i].scale + 0.03f, 1.1f);
                }
                else
                {
                    forSaleButtons[i].scale = Math.Max(1f, forSaleButtons[i].scale - 0.03f);
                }
            }
        }

        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            if (scrolling)
            {
                int y2 = scrollBar.bounds.Y;
                scrollBar.bounds.Y = Math.Min(yPositionOnScreen + height - 64 - 12 - scrollBar.bounds.Height, Math.Max(y, yPositionOnScreen + upArrow.bounds.Height + 20));
                float num = (y - scrollBarRunner.Y) / (float)scrollBarRunner.Height;
                currentItemIndex = Math.Min(Math.Max(0, currentList.Count - 4), Math.Max(0, (int)(currentList.Count * num)));
                _setScrollBarToCurrentIndex();
                if (y2 != scrollBar.bounds.Y) Game1.playSound("shiny4");
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            scrolling = false;
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && currentItemIndex > 0)
            {
                _upArrowPressed();
                Game1.playSound("shiny4");
            }
            else if (direction < 0 && currentItemIndex < Math.Max(0, currentList.Count - 4))
            {
                _downArrowPressed();
                Game1.playSound("shiny4");
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.options.showMenuBackground && 
                !Game1.options.showClearBackgrounds && 
                !PlayerInteractionHandler.isAppRunning.Value && 
                !PlayerInteractionHandler.isJPadRunning.Value)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            }

            drawTextureBox(b, MenuTexture, WindowBorderSourceRect, xPositionOnScreen, yPositionOnScreen + 64, width, 460, Color.White, 4f);
            drawTextureBox(b, MenuTexture, WindowBorderSourceRect, xPositionOnScreen, yPositionOnScreen - 16, width - 250, 80, Color.White, 4f);
            drawTextureBox(b, MenuTexture, WindowBorderSourceRect, xPositionOnScreen + width - 250, yPositionOnScreen - 16, 250, 80, Color.White, 4f);
            drawTextureBox(b, MenuTexture, WindowBorderSourceRect, xPositionOnScreen, yPositionOnScreen + 524, 560, 100, Color.White, 4f);
            //Game1.dayTimeMoneyBox.drawMoneyBox(b, xPositionOnScreen - 36, yPositionOnScreen + height - 240);
            Game1.dayTimeMoneyBox.moneyDial.draw(b, new(xPositionOnScreen + width - 220, yPositionOnScreen + 10), Game1.player.Money);
            //b.DrawString(Game1.dialogueFont, "G" + Game1.player.Money, new(xPositionOnScreen + width - 250, yPositionOnScreen), Color.Brown);
            for (int i = 0; i < 4; i++) drawSlot(i, b);

            if (currentList.Count == 0)
            {
                string str;
                if (viewingCart) str = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11583");
                else str = ModEntry._Helper.Translation.Get("cart.none");

                SpriteText.drawString(b, str, xPositionOnScreen + width / 2 - SpriteText.getWidthOfString(str) / 2, yPositionOnScreen + height / 2 - 128);
            }

            drawAnimation(b);

            poof?.draw(b);
            upArrow.draw(b);
            downArrow.draw(b);

            if(!search.containsPoint(Game1.getMouseX(), Game1.getMouseY())) search.draw(b, Color.White * 0.3f, 0.86f);
            else search.draw(b);
            if (!unSearch.containsPoint(Game1.getMouseX(), Game1.getMouseY())) unSearch.draw(b, Color.White * 0.3f, 0.86f);
            else unSearch.draw(b);

            if (currentList.Count > 4)
            {
                drawTextureBox(b, MenuTexture, ScrollBarBackSourceRect, scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f);
                scrollBar.draw(b);
            }

            if (!hoverText.Equals(""))
            {
                if (hoveredItem?.IsRecipe ?? false) 
                    drawToolTip(b, " ", boldTitleText, hoveredItem as Item, craftingIngredients: new CraftingRecipe(hoveredItem.Name.Replace(" Recipe", "")));
                else drawToolTip(b, hoverText, boldTitleText, hoveredItem as Item);
            }

            base.draw(b);
            drawMouse(b);
        }
    }
}
