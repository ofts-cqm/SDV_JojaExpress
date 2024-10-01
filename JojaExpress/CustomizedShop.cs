using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
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
        public int currentItemIndex = 0, hoverPrice = -1, safetyTimer = 250;
        public int totalMoney {get; set; } = 0;
        public bool scrolling = false, viewingCart = false, viewingNotification;
        public string shopId, hoverText = "", boldTitleText = "", searchString = "", searchNone, cartNone, checkOutStr, backStr, cartStr, totalStr, searchEmptyStr;
        public ISalable? hoveredItem;
        public MoneyDial totalMoneyDial = new(8);
        public Dictionary<string, int> knownPurchased;

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
            ITranslationHelper trans = ModEntry._Helper.Translation;
            searchNone = trans.Get("search.none", new { search = searchString });
            cartNone = trans.Get("cart.none");
            cartStr = trans.Get("view_cart");
            backStr = trans.Get("view_shop");
            checkOutStr = trans.Get("checkout");
            totalStr = trans.Get("totalMoney");
            searchEmptyStr = trans.Get("emptySearch");

            totalMoneyDial.currentValue = 0;
            exitFunction = PlayerInteractionHandler.exitMenu;
        }

        public void checkout_exit()
        {
            Game1.player.Money -= totalMoney;
            foreach (var p in purchased)
            {
                if (knownPurchased.ContainsKey(p.Key.QualifiedItemId))
                {
                    knownPurchased[p.Key.QualifiedItemId] += p.Value.Stock;
                }
                else knownPurchased.Add(p.Key.QualifiedItemId, p.Value.Stock);
            }
            PlayerInteractionHandler.exitMenu();
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
            Game1.player.team.synchronizedShopStock.OnItemPurchased(shopId, salable, forSale, amount);
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
            Game1.player.team.synchronizedShopStock.OnItemPurchased(shopId, salable, forSale, -amount);
            if (stock.Stock == 0) currentList.RemoveAt(index);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds) => _updatePosition();

        public override void performHoverAction(int x, int y)
        {
            if (viewingNotification) return;
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
                    ISalable salable = currentList[currentItemIndex + i];
                    hoverText = salable.getDescription();
                    boldTitleText = salable.DisplayName;
                    hoverPrice = currentList.getValue(currentItemIndex + i).Price;
                    hoveredItem = salable;
                    forSaleButtons[i].scale = Math.Min(forSaleButtons[i].scale + 0.03f, 1.1f);
                }
                else
                {
                    forSaleButtons[i].scale = Math.Max(1f, forSaleButtons[i].scale - 0.03f);
                }
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (viewingNotification) return;
            if (!viewingCart && cartButton.containsPoint(x, y))
            {
                viewingCart = true;
                currentItemIndex = 0;
                currentList = new View(purchased);
                searchString = "";
                return;
            }
            if (viewingCart && backButton.containsPoint(x, y))
            {
                viewingCart = false;
                currentItemIndex = 0;
                currentList = new View(forSale);
                searchString = "";
                return;
            }
            if (viewingCart && checkOutButton.containsPoint(x, y))
            {
                _tryCheckOut();
                return;
            }
            if(upperRightCloseButton.containsPoint(x, y))
            {
                _tryCloseMenu();
                return;
            }

            //base.receiveLeftClick(x, y, playSound);
            if (downArrow.containsPoint(x, y) && currentItemIndex < Math.Max(0, currentList.Count - 4))
            {
                _downArrowPressed();
                Game1.playSound("shwip");
            }
            else if (upArrow.containsPoint(x, y) && currentItemIndex > 0)
            {
                _upArrowPressed();
                Game1.playSound("shwip");
            }
            else if (scrollBar.containsPoint(x, y))
            {
                scrolling = true;
            }
            else if (!downArrow.containsPoint(x, y) && x > xPositionOnScreen + width && x < xPositionOnScreen + width + 128 && y > yPositionOnScreen && y < yPositionOnScreen + height)
            {
                scrolling = true;
                leftClickHeld(x, y);
                releaseLeftClick(x, y);
            }

            for (int j = 0; j < forSaleButtons.Length; j++)
            {
                if (currentItemIndex + j < currentList.Count && forSaleButtons[j].containsPoint(x, y))
                {
                    if (viewingCart)
                    {
                        if (pricePlus[j].containsPoint(x, y)) purchaseItem(currentItemIndex + j);
                        else if (priceMin[j].containsPoint(x, y)) sellItem(currentItemIndex + j);
                    }
                    else purchaseItem(currentItemIndex + j);
                    break;
                }
            }

            currentItemIndex = Math.Max(0, Math.Min(currentList.Count - 4, currentItemIndex));
            _setScrollBarToCurrentIndex();
        }

        public override void leftClickHeld(int x, int y)
        {
            if (viewingNotification) return;
            base.leftClickHeld(x, y);
            if (!downArrow.containsPoint(x, y) && x > xPositionOnScreen + width && x < xPositionOnScreen + width + 128 && y > yPositionOnScreen && y < yPositionOnScreen + height)
            {
                scrolling = true;
            }
            if (scrolling)
            {
                int y2 = scrollBar.bounds.Y;
                //scrollBar.bounds.Y = Math.Min(scrollBarRunner.Y, Math.Max(y - 20, scrollBarRunner.Bottom));//Math.Min(yPositionOnScreen + height - 64 - 12 - scrollBar.bounds.Height, Math.Max(y - 20, yPositionOnScreen + upArrow.bounds.Height + 20));
                float num = (y - scrollBarRunner.Y - 20) / (float)scrollBarRunner.Height;
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

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (viewingNotification) return;
            if (viewingCart) return;
            for (int j = 0; j < forSaleButtons.Length; j++)
            {
                if (currentItemIndex + j < currentList.Count && forSaleButtons[j].containsPoint(x, y))
                {
                    purchaseItem(currentItemIndex + j);
                    break;
                }
            }
            currentItemIndex = Math.Max(0, Math.Min(currentList.Count - 4, currentItemIndex));
            _setScrollBarToCurrentIndex();
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (viewingNotification) return;
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

        public override void receiveKeyPress(Keys key)
        {
            if (viewingNotification) return;
            if (key != 0)
            {
                if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose())
                {
                    _tryCloseMenu();
                }
                else if (Game1.options.snappyMenus && Game1.options.gamepadControls && !overrideSnappyMenuCursorMovementBan())
                {
                    applyMovementKey(key);
                }
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
            drawTextureBox(b, MenuTexture, WindowBorderSourceRect, xPositionOnScreen, yPositionOnScreen + 524, 560, 110, Color.White, 4f);
            //Game1.dayTimeMoneyBox.drawMoneyBox(b, xPositionOnScreen - 36, yPositionOnScreen + height - 240);
            Game1.dayTimeMoneyBox.moneyDial.draw(b, new(xPositionOnScreen + width - 220, yPositionOnScreen + 10), Game1.player.Money);
            totalMoneyDial.draw(b, new(xPositionOnScreen + 340, yPositionOnScreen + height - 65), totalMoney);
            b.DrawString(Game1.dialogueFont, totalStr, new(xPositionOnScreen + 20, yPositionOnScreen + 560), Color.Aqua);
            //b.DrawString(Game1.dialogueFont, "G" + Game1.player.Money, new(xPositionOnScreen + width - 250, yPositionOnScreen), Color.Brown);
            for (int i = 0; i < 4; i++) drawSlot(i, b);

            if (currentList.Count == 0)
            {
                string str;
                if (!viewingCart) str = searchNone;
                else str = cartNone;

                SpriteText.drawString(b, str, xPositionOnScreen + width / 2 - SpriteText.getWidthOfString(str) / 2, yPositionOnScreen + height / 2 - 64);
            }

            drawAnimation(b);
            poof?.draw(b);

            if(!search.containsPoint(Game1.getMouseX(), Game1.getMouseY())) search.draw(b, Color.White * 0.3f, 0.86f);
            else search.draw(b);
            if (!unSearch.containsPoint(Game1.getMouseX(), Game1.getMouseY())) unSearch.draw(b, Color.White * 0.3f, 0.86f);
            else unSearch.draw(b);

            if (viewingCart)
            {
                backButton.draw(b);
                SpriteText.drawStringHorizontallyCenteredAt(b, backStr, backButton.bounds.X + 130, cartButton.bounds.Y + 25, color: Color.Black);
                checkOutButton.draw(b);
                SpriteText.drawStringHorizontallyCenteredAt(b, checkOutStr, checkOutButton.bounds.X + 130, cartButton.bounds.Y + 25, color: Color.White);
            }
            else
            {
                cartButton.draw(b);
                SpriteText.drawStringHorizontallyCenteredAt(b, cartStr, cartButton.bounds.X + 260, cartButton.bounds.Y + 25, color: Color.LightBlue);
            }

            if (currentList.Count > 4)
            {
                drawTextureBox(b, MenuTexture, ScrollBarBackSourceRect, scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height + 40, Color.White, 4f);
                scrollBar.draw(b);
                upArrow.draw(b);
                downArrow.draw(b);
            }

            if (!hoverText.Equals(""))
            {
                if (hoveredItem?.IsRecipe ?? false) 
                    drawToolTip(b, " ", boldTitleText, hoveredItem as Item, craftingIngredients: new CraftingRecipe(hoveredItem.Name.Replace(" Recipe", "")));
                else drawToolTip(b, hoverText, boldTitleText, hoveredItem as Item);
            }

            base.draw(b);
            if (viewingNotification) drawNotification(b);

            drawMouse(b);
        }
    }
}
