using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace JojaExpress
{
    public partial class CustomizedShop : IClickableMenu
    {
        public const string openMenuSound = "dwop", purchaseSound = "purchaseClick", purchaseRepeatSound = "purchaseRepeat";
        public readonly Rectangle WindowBorderSourceRect = new(384, 373, 18, 18), 
            ItemRowBackgroundSourceRect = new(384, 396, 15, 15), 
            ItemIconBackgroundSourceRect = new(296, 363, 18, 18),
            ScrollUpSourceRect = new(421, 459, 11, 12),
            ScrollDownSourceRect = new(421, 472, 11, 12),
            ScrollBarFrontSourceRect = new(435, 463, 6, 10),
            ScrollBarBackSourceRect = new(403, 383, 6, 6),
            SearchIconSourceRect = new(80, 0, 16, 16),
            CancleSearchSourceRect = new(268, 470, 16, 16);

        public TemporaryAnimatedSpriteList animations = new TemporaryAnimatedSpriteList();
        public TemporaryAnimatedSprite poof;

        public void _fillStockInfo(Dictionary<ISalable, ItemStockInformation> stock, Dictionary<string, int> knownPurchased)
        {
            foreach (KeyValuePair<ISalable, ItemStockInformation> item in stock)
            {
                ItemStockInformation stockInfo = item.Value;
                if (item.Key.IsRecipe)
                {
                    if (Game1.player.knowsRecipe(item.Key.Name)) return;
                    item.Key.Stack = 1;
                }

                if (knownPurchased.TryGetValue(item.Key.QualifiedItemId, out int amt))
                {
                    stockInfo.Stock -= amt;
                    purchased.Add(item.Key, new ItemStockInformation(stockInfo.Price, amt));
                }
                if (stockInfo.Stock != 0) forSale.Add(item.Key, item.Value);
            }
        }

        public void InitClickableComponents()
        {
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
            
            for (int i = 0; i < 4; i++)
            {
                forSaleButtons[i] = new ClickableComponent(Rectangle.Empty, i.ToString());
                priceBG[i] = new ClickableTextureComponent(Rectangle.Empty, PriceTexture, new Rectangle(0, 0, 96, 32), 2f);
                priceMin[i] = new ClickableTextureComponent(Rectangle.Empty, PriceTexture, new Rectangle(6, 6, 20, 20), 2f);
                pricePlus[i] = new ClickableTextureComponent(Rectangle.Empty, PriceTexture, new Rectangle(70, 6, 20, 20), 2f);
            }
        }

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

            cartButton.bounds = new(xPositionOnScreen + 560, yPositionOnScreen + height - 100, 520, 100);
            checkOutButton.bounds = new(xPositionOnScreen + 820, yPositionOnScreen + height - 100, 260, 100);
            backButton.bounds = new(xPositionOnScreen + 560, yPositionOnScreen + height - 100, 260, 100);
            searchTab.bounds = new(xPositionOnScreen, yPositionOnScreen, width, 64);
            moneyTab.bounds = new(xPositionOnScreen, yPositionOnScreen + height - 100, 560, 100);
            search.bounds = new(xPositionOnScreen + width - 260 - 128, yPositionOnScreen, 64, 64);
            unSearch.bounds = new(xPositionOnScreen + width - 260 - 64, yPositionOnScreen, 64, 64);

            for (int i = 0; i < 4; i++)
            {
                forSaleButtons[i].bounds = new(xPositionOnScreen + 16, yPositionOnScreen + 83 + i * ((height - 206) / 4), width - 32, (height - 206) / 4 + 4);
                priceBG[i].bounds = new(forSaleButtons[i].bounds.X + 830, forSaleButtons[i].bounds.Y + 24, 96 * 2, 32 * 2);
                priceMin[i].bounds = new(forSaleButtons[i].bounds.X + 842, forSaleButtons[i].bounds.Y + 24 + 12, 40, 40);
                pricePlus[i].bounds = new(forSaleButtons[i].bounds.X + 970, forSaleButtons[i].bounds.Y + 24 + 12, 40, 40);
            }
        }

        public int _getPurchaseAmount(int itemIndex)
        {
            if (!Game1.oldKBState.IsKeyDown(Keys.LeftShift)) return 1;
            int currentVal;
            if (Game1.oldKBState.IsKeyDown(Keys.LeftControl))
                currentVal = Game1.oldKBState.IsKeyDown(Keys.D1) ? 999 : 25;
            else currentVal = 5;

            currentVal = Math.Min(currentVal, Math.Max(1, forSale[currentList[itemIndex]].Stock));
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

        public override bool readyToClose() => animations.Count == 0;

        public void _tryCloseMenu()
        {
            if (!viewingCart)
            {
                viewingCart = true;
                currentItemIndex = 0;
                currentList = new View(purchased);
                searchString = "";
                return;
            }
            viewingNotification = true;
            // Todo complete this fuck
        }

        public void _tryCheckOut()
        {
            viewingNotification = true;
            // Todo complete this fuck
        }

        public void drawNotification(SpriteBatch b)
        {
            // Todo complete this fuck
        }

        public void drawAnimation(SpriteBatch b)
        {
            for (int i = animations.Count - 1; i >= 0; i--)
            {
                if (animations[i].update(Game1.currentGameTime))
                {
                    animations.RemoveAt(i);
                }
                else
                {
                    animations[i].draw(b, localPosition: true);
                }
            }
        }

        public void drawSalableIcon(int i, SpriteBatch b, ISalable salable, ItemStockInformation stockInfo)
        {
            b.Draw(MenuTexture, new Vector2(forSaleButtons[i].bounds.X + 32 - 12, forSaleButtons[i].bounds.Y + 24 - 4), ItemIconBackgroundSourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            Vector2 vector = new Vector2(forSaleButtons[i].bounds.X + 32 - 8, forSaleButtons[i].bounds.Y + 24);
            Color color = Color.White;
            int num = stockInfo.Stock;

            salable.drawInMenu(b, vector, 1f, 1f, 0.9f, StackDrawType.HideButShowQuality, color, drawShadow: true);
            if (num != int.MaxValue)
            {
                Utility.drawTinyDigits(num, b, vector + new Vector2(64 - Utility.getWidthOfTinyDigitString(num, 3f) + 3, 47f), 3f, 1f, color);
            }
        }

        public void drawSalableName(int i, SpriteBatch b, ISalable salable, ItemStockInformation stockInfo)
        {
            string text = salable.DisplayName;
            if (salable.Stack > 1) text = text + " x" + salable.Stack;
            bool flag2 = stockInfo.Price > 0;
            if (SpriteText.getWidthOfString(text) > width - (flag2 ? (150 + SpriteText.getWidthOfString(stockInfo.Price + " ")) : 100) && text.Length > (flag2 ? 27 : 37))
            {
                text = text.Substring(0, flag2 ? 27 : 37);
                text += "...";
            }
            SpriteText.drawString(b, text, forSaleButtons[i].bounds.X + 96 + 8, forSaleButtons[i].bounds.Y + 28);
        }

        public void drawSalablePrice(int i, SpriteBatch b, ISalable salable, ItemStockInformation stockInfo)
        {
            int num2 = forSaleButtons[i].bounds.Right;
            if (stockInfo.Price > 0)
            {
                SpriteText.drawString(b, stockInfo.Price + " ", num2 - SpriteText.getWidthOfString(stockInfo.Price + " ") - 60, forSaleButtons[i].bounds.Y + 28, alpha: Game1.player.Money >= stockInfo.Price ? 1f : 0.5f);
                Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(forSaleButtons[i].bounds.Right - 52, forSaleButtons[i].bounds.Y + 40 - 4), new Rectangle(193, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, layerDepth: 1);
            }
        }

        public void drawSalableAmount(int i, SpriteBatch b, ISalable salable, ItemStockInformation stockInfo)
        {
            priceBG[i].draw(b);
            pricePlus[i].draw(b);
            priceMin[i].draw(b);
            int num = stockInfo.Stock;
            int numLength = SpriteText.getWidthOfString(num + "");
            SpriteText.drawString(
                b, num + "", forSaleButtons[i].bounds.X + 890 + (70 - numLength) / 2, forSaleButtons[i].bounds.Y + 32, color: Color.Gray
            );
        }

        public void drawSlot(int i, SpriteBatch b)
        {
            if (currentItemIndex + i >= currentList.Count) return;

            drawTextureBox(
                b, MenuTexture, ItemRowBackgroundSourceRect,
                forSaleButtons[i].bounds.X, forSaleButtons[i].bounds.Y,
                forSaleButtons[i].bounds.Width, forSaleButtons[i].bounds.Height,
                (forSaleButtons[i].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) && !scrolling) ?
                Color.Wheat : Color.White, 4f, drawShadow: false);
            ISalable salable = currentList[currentItemIndex + i];
            ItemStockInformation stockInfo = currentList.getValue(currentItemIndex + i);

            if (salable.ShouldDrawIcon()) drawSalableIcon(i, b, salable, stockInfo);
            drawSalableName(i, b, salable, stockInfo);
            if (viewingCart) drawSalableAmount(i, b, salable,stockInfo);
            else drawSalablePrice(i, b, salable, stockInfo);
        }
    }
}
