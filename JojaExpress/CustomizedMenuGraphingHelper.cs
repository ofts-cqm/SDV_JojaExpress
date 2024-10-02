using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;

namespace JojaExpress
{
    public partial class CustomizedShop : IClickableMenu
    {
        public ClickableComponent[] forSaleButtons = new ClickableComponent[4];
        public ClickableComponent searchTab, moneyTab;
        public ClickableTextureComponent upArrow, downArrow, scrollBar, cartButton, checkOutButton, backButton, search, unSearch, leftOption, middleOption, rightOption, notification;
        public ClickableTextureComponent[] priceBG = new ClickableTextureComponent[4],
            pricePlus = new ClickableTextureComponent[4],
            priceMin = new ClickableTextureComponent[4];
        public Texture2D MenuTexture = Game1.mouseCursors, PriceTexture, notificationTexture;

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
            b.DrawString(Game1.dialogueFont, totalStr, new(xPositionOnScreen + 20, yPositionOnScreen + 560), Color.Blue);
            //b.DrawString(Game1.dialogueFont, "G" + Game1.player.Money, new(xPositionOnScreen + width - 250, yPositionOnScreen), Color.Brown);
            for (int i = 0; i < 4; i++) drawSlot(i, b);

            if (currentList.Count == 0)
            {
                string str;
                if (!viewingCart) str = searchNone;
                else str = cartNone;

                SpriteText.drawString(b, str, xPositionOnScreen + width / 2 - SpriteText.getWidthOfString(str) / 2, yPositionOnScreen + height / 2 - 64);
            }

            if (!search.containsPoint(Game1.getMouseX(), Game1.getMouseY())) search.draw(b, Color.White * 0.3f, 0.86f);
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
                SpriteText.drawStringHorizontallyCenteredAt(b, cartStr, cartButton.bounds.X + 260, cartButton.bounds.Y + 25, color: Color.Blue);
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

        public void drawNotification(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            notification.draw(b);
            leftOption.draw(b);
            middleOption.draw(b);
            rightOption.draw(b);
            SpriteText.drawString(b, notificationString[0], notification.bounds.X + 50, notification.bounds.Y + 50, width: notification.bounds.Width - 100, color: Color.Blue);
            SpriteText.drawStringHorizontallyCenteredAt(b, notificationString[1], leftOption.bounds.X + 130, leftOption.bounds.Y + 30, width: leftOption.bounds.Width, color: Color.Black);
            SpriteText.drawStringHorizontallyCenteredAt(b, notificationString[2], middleOption.bounds.X + 130, middleOption.bounds.Y + 30, width: middleOption.bounds.Width, color: Color.Black);
            SpriteText.drawStringHorizontallyCenteredAt(b, notificationString[3], rightOption.bounds.X + 130, rightOption.bounds.Y + 30, width: rightOption.bounds.Width, color: Color.White);
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
            if (viewingCart) drawSalableAmount(i, b, salable, stockInfo);
            else drawSalablePrice(i, b, salable, stockInfo);
        }
    }
}
