using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace JojaExpress
{
    public partial class CustomizedShop : IClickableMenu
    {
        public const string openMenuSound = "dwop", purchaseSound = "purchaseClick", purchaseRepeatSound = "purchaseRepeat";
        public readonly Rectangle WindowBorderSourceRect = new(18, 0, 18, 18), 
            ItemRowBackgroundSourceRect = new(36, 0, 15, 15), 
            ItemIconBackgroundSourceRect = new(0, 0, 18, 18),
            ScrollUpSourceRect = new(51, 0, 11, 12),
            ScrollDownSourceRect = new(51, 12, 11, 12),
            ScrollBarFrontSourceRect = new(62, 0, 6, 10),
            ScrollBarBackSourceRect = new(0, 18, 6, 6),
            SearchIconSourceRect = new(80, 0, 16, 16),
            CancleSearchSourceRect = new(268, 470, 16, 16);
        public Action<int> afterNotification;
        public string[] notificationString = { "", "", "", ""};

        public void _fillStockInfo(Dictionary<ISalable, ItemStockInformation> stock, bool addRecipe, bool addPacked)
        {
            foreach (KeyValuePair<ISalable, ItemStockInformation> item in stock)
            {
                ItemStockInformation stockInfo = item.Value;
                if (item.Key.IsRecipe)
                {
                    if (Game1.player.knowsRecipe(item.Key.Name)) continue;
                    item.Key.Stack = 1;
                }

                if (item.Key.IsRecipe && !addRecipe) continue;
                if (item.Key is PackedItem && !addPacked) continue;

                if (stockInfo.Stock != 0) forSale.Add(item.Key, item.Value);
            }
        }

        public void InitClickableComponents()
        {
            MenuTexture = ModEntry._Helper.ModContent.Load<Texture2D>("assets/menu");

            upperRightCloseButton = new(Rectangle.Empty, Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
            upArrow = new(Rectangle.Empty, MenuTexture, ScrollUpSourceRect, 4f);
            downArrow = new(Rectangle.Empty, MenuTexture, ScrollDownSourceRect, 4f);
            scrollBar = new(Rectangle.Empty, MenuTexture, ScrollBarFrontSourceRect, 4f);

            cartButton = new(Rectangle.Empty, PriceTexture, new Rectangle(96, 0, 130, 25), 4f);
            checkOutButton = new(Rectangle.Empty, PriceTexture, new Rectangle(291, 0, 65, 25), 4f);
            backButton = new(Rectangle.Empty, PriceTexture, new Rectangle(226, 0, 65, 25), 4f);
            searchTab = new(Rectangle.Empty, "searchTab");
            moneyTab = new(Rectangle.Empty, "moneyTab");
            search = new(Rectangle.Empty, Game1.mouseCursors, SearchIconSourceRect, 4f);
            unSearch = new(Rectangle.Empty, Game1.mouseCursors, CancleSearchSourceRect, 3f);

            leftOption = new(Rectangle.Empty, PriceTexture, new Rectangle(226, 0, 65, 25), 4f);
            middleOption = new(Rectangle.Empty, PriceTexture, new Rectangle(226, 0, 65, 25), 4f);
            rightOption = new(Rectangle.Empty, PriceTexture, new Rectangle(291, 0, 65, 25), 4f);
            notification = new(Rectangle.Empty, notificationTexture, new Rectangle(0, 0, 64, 48), 14f);

            for (int i = 0; i < 4; i++)
            {
                forSaleButtons[i] = new ClickableComponent(Rectangle.Empty, i.ToString());
                priceBG[i] = new ClickableTextureComponent(Rectangle.Empty, PriceTexture, new Rectangle(0, 0, 96, 32), 2f);
                priceMin[i] = new ClickableTextureComponent(Rectangle.Empty, PriceTexture, new Rectangle(6, 6, 20, 20), 2f);
                pricePlus[i] = new ClickableTextureComponent(Rectangle.Empty, PriceTexture, new Rectangle(70, 6, 20, 20), 2f);
            }

            MobilePhoneRender.setBG("shop");
            MobilePhoneRender.protrait.Clear();
            MobilePhoneRender.landscape.Clear();


            Func<ISalable?>[] funcs = new[] {func1, func2, func3, func4 };
            Func<ItemStockInformation?>[] funcs2 = new[] { func5, func6, func7, func8 };
            for (int i = 0; i < 4; i++)
            {
                MobilePhoneRender.protrait.Add(new TexturedRenderPack(funcs[i], 35 - 16, 61 + 100 * i - 16, 0.5f));
                MobilePhoneRender.protrait.Add(new VolatileRenderPack(funcs[i], 35, 100 + 100 * i, 220, 55, Game1.dialogueFont));
                MobilePhoneRender.protrait.Add(new VolatileRenderPack(funcs2[i], 200, 65 + 100 * i, 100, 50, null));
                MobilePhoneRender.landscape.Add(new TexturedRenderPack(funcs[i], 64 - 16, 38 + 59 * i - 16, 0.5f));
                MobilePhoneRender.landscape.Add(new VolatileRenderPack(funcs[i], 100, 38 + 59 * i, 290, 55, Game1.dialogueFont));
                MobilePhoneRender.landscape.Add(new VolatileRenderPack(funcs2[i], 390, 38 + 59 * i, 100, 50, null));
            }
        }

        public ISalable? func1() => currentList.Count > currentItemIndex ? currentList[currentItemIndex] : null;
        public ISalable? func2() => currentList.Count > currentItemIndex + 1 ? currentList[currentItemIndex + 1] : null;
        public ISalable? func3() => currentList.Count > currentItemIndex + 2 ? currentList[currentItemIndex + 2] : null;
        public ISalable? func4() => currentList.Count > currentItemIndex + 3 ? currentList[currentItemIndex + 3] : null;
        public ItemStockInformation? func5() => currentList.Count > currentItemIndex ? currentList.getValue(currentItemIndex) : null;
        public ItemStockInformation? func6() => currentList.Count > currentItemIndex + 1 ? currentList.getValue(currentItemIndex + 1) : null;
        public ItemStockInformation? func7() => currentList.Count > currentItemIndex + 2 ? currentList.getValue(currentItemIndex + 2) : null;
        public ItemStockInformation? func8() => currentList.Count > currentItemIndex + 3 ? currentList.getValue(currentItemIndex + 3) : null;

        public void _updatePosition()
        {
            width = 1000 + borderWidth * 2;
            height = 550 + borderWidth * 2;
            xPositionOnScreen = Game1.uiViewport.Width / 2 - 580 + borderWidth;
            yPositionOnScreen = Game1.uiViewport.Height / 2 - 350 + borderWidth;

            upperRightCloseButton.bounds = new(xPositionOnScreen + width - 4, yPositionOnScreen, 48, 48);
            upArrow.bounds = new(xPositionOnScreen + width, yPositionOnScreen + 60, 44, 48);
            downArrow.bounds = new(xPositionOnScreen + width, yPositionOnScreen + height - 64, 44, 48);
            scrollBar.bounds = new(upArrow.bounds.X + 12, upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40);
            scrollBarRunner = new(scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + 4, scrollBar.bounds.Width, height - 220);

            cartButton.bounds = new(xPositionOnScreen + 560, yPositionOnScreen + height - 106, 520, 100);
            checkOutButton.bounds = new(xPositionOnScreen + 820, yPositionOnScreen + height - 106, 260, 100);
            backButton.bounds = new(xPositionOnScreen + 560, yPositionOnScreen + height - 106, 260, 100);
            searchTab.bounds = new(xPositionOnScreen, yPositionOnScreen, width, 64);
            moneyTab.bounds = new(xPositionOnScreen, yPositionOnScreen + height - 100, 560, 100);
            search.bounds = new(xPositionOnScreen + width - 260 - 128, yPositionOnScreen, 64, 64);
            unSearch.bounds = new(xPositionOnScreen + width - 260 - 64, yPositionOnScreen, 64, 64);

            notification.bounds = new Rectangle(xPositionOnScreen + width / 2 - 448, yPositionOnScreen + height / 2 - 336, 896, 672);
            leftOption.bounds = new(notification.bounds.X + 58, notification.bounds.Bottom - 130, 260, 100);
            middleOption.bounds = new(notification.bounds.X + 318, notification.bounds.Bottom - 130, 260, 100);
            rightOption.bounds = new(notification.bounds.X + 578, notification.bounds.Bottom - 130, 260, 100);

            for (int i = 0; i < 4; i++)
            {
                forSaleButtons[i].bounds = new(xPositionOnScreen + 16, yPositionOnScreen + 83 + i * ((height - 206) / 4), width - 32, (height - 206) / 4 + 4);
                priceBG[i].bounds = new(forSaleButtons[i].bounds.X + 830, forSaleButtons[i].bounds.Y + 24, 96 * 2, 32 * 2);
                priceMin[i].bounds = new(forSaleButtons[i].bounds.X + 842, forSaleButtons[i].bounds.Y + 24 + 12, 40, 40);
                pricePlus[i].bounds = new(forSaleButtons[i].bounds.X + 970, forSaleButtons[i].bounds.Y + 24 + 12, 40, 40);
            }
        }

        public int _getPurchaseAmount(int itemIndex, Dictionary<ISalable, ItemStockInformation> shop)
        {
            if (!Game1.oldKBState.IsKeyDown(Keys.LeftShift)) return 1;
            int currentVal;
            if (Game1.oldKBState.IsKeyDown(Keys.LeftControl))
                currentVal = Game1.oldKBState.IsKeyDown(Keys.D1) ? 999 : 25;
            else currentVal = 5;

            currentVal = Math.Min(currentVal, Math.Max(1, shop[currentList[itemIndex]].Stock));
            currentVal = Math.Min(currentVal, currentList[itemIndex].maximumStackSize());
            if (currentVal == -1) currentVal = 1;

            return currentVal;
        }

        public void _setScrollBarToCurrentIndex()
        {
            if (currentList.Count > 0)
            {
                float num = scrollBarRunner.Height / (float)Math.Max(1, currentList.Count - 4 + 1);
                scrollBar.bounds.Y = (int)(num * currentItemIndex + upArrow.bounds.Bottom + 4);
                if (currentItemIndex == currentList.Count - 4)
                {
                    scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - 4;
                }
            }
        }

        public void _downArrowPressed()
        {
            downArrow.scale = downArrow.baseScale;
            currentItemIndex++;
            _setScrollBarToCurrentIndex();
        }

        public void _upArrowPressed()
        {
            upArrow.scale = upArrow.baseScale;
            currentItemIndex--;
            _setScrollBarToCurrentIndex();
        }

        public void _tryCloseMenu()
        {
            if (!viewingCart)
            {
                viewingCart = true;
                currentItemIndex = 0;
                currentList = new View(purchased);
                searchBox.Text = "";
                return;
            }
            if (purchased.Count == 0)
            {
                exitThisMenu();
                return;
            }
            getNotificationStr("unexpClose");
            afterNotification = (option) =>
            {
                switch (option)
                {
                    case 0:
                        {
                            viewingNotification = false;
                            break;
                        }
                    case 1:
                        {
                            exitThisMenu();
                            break;
                        }
                    case 2:
                        {
                            _tryCheckOut();
                            break;
                        }
                }
            };
        }

        public void _tryCheckOut()
        {
            if (Game1.player.Money >= totalMoney)
            {
                checkout_exit();
                return;
            }
            getNotificationStr("noMoney");
            afterNotification = (option) =>
            {
                switch (option)
                {
                    case 0:
                        {
                            viewingNotification = false;
                            break;
                        }
                    case 1:
                        {
                            viewingCart = false;
                            currentItemIndex = 0;
                            currentList = new View(forSale);
                            _setScrollBarToCurrentIndex();
                            searchBox.Text = "";
                            viewingNotification = false;
                            break;
                        }
                    case 2:
                        {
                            exitThisMenu();
                            break;
                        }
                }
            };
        }

        public void getNotificationStr(string key)
        {
            viewingNotification = true;
            ITranslationHelper helper = ModEntry._Helper.Translation;
            for(int i = 0; i < 4; i++) 
            {
                notificationString[i] = helper.Get(key + "." + i);
            }
        }

        public void _notificationClick(int x, int y)
        {
            if (leftOption.containsPoint(x, y)) { viewingNotification = false; afterNotification.Invoke(0);  return; }
            if (middleOption.containsPoint(x, y)) { viewingNotification = false; afterNotification.Invoke(1);  return; }
            if (rightOption.containsPoint(x, y)) { viewingNotification = false; afterNotification.Invoke(2);  return; }
        }

        public void _notificationHover(int x, int y)
        {
            leftOption.tryHover(x, y);
            middleOption.tryHover(x, y);
            rightOption.tryHover(x, y);
        }
    }
}
