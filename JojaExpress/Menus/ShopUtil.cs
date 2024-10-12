using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Logging;
using StardewValley.Menus;
using StardewValley.Triggers;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace JojaExpress
{
    public partial class JojaShopMenu
    {
        public void drawToolTip(SpriteBatch b)
        {
            if (hoverText.Equals("")) return;
            
            if (hoveredItem?.IsRecipe ?? false)
            {
                drawToolTip(
                    b, " ", boldTitleText, hoveredItem as Item, heldItem != null, -1, currency,
                    involkeMethod<string>("getHoveredItemExtraItemIndex"),
                    involkeMethod<int>("getHoveredItemExtraItemAmount"),
                    new CraftingRecipe(hoveredItem.Name.Replace(" Recipe", "")),
                    (hoverPrice > 0) ? hoverPrice : (-1)
                );
            }
            else
            {
                drawToolTip(
                    b, hoverText, boldTitleText, hoveredItem as Item,
                    heldItem != null, -1, currency,
                    involkeMethod<string>("getHoveredItemExtraItemIndex"),
                    involkeMethod<int>("getHoveredItemExtraItemAmount"),
                    null, (hoverPrice > 0) ? hoverPrice : (-1)
                );
            }
        }

        private void setScrollBarToCurrentIndex()
        {
            if (forSale.Count > 0)
            {
                float num = (float)getValue<Rectangle>("scrollBarRunner").Height / (float)Math.Max(1, forSale.Count - 4 + 1);
                scrollBar.bounds.Y = (int)(num * (float)currentItemIndex + (float)upArrow.bounds.Bottom + 4f);
                if (currentItemIndex == forSale.Count - 4)
                {
                    scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - 4;
                }
            }
        }

        private void downArrowPressed()
        {
            downArrow.scale = downArrow.baseScale;
            currentItemIndex++;
            setScrollBarToCurrentIndex();
            updateSaleButtonNeighbors();
        }

        private void upArrowPressed()
        {
            upArrow.scale = upArrow.baseScale;
            currentItemIndex--;
            setScrollBarToCurrentIndex();
            updateSaleButtonNeighbors();
        }

        public void alternateLeftClick(int x, int y)
        {
            if (upperRightCloseButton != null && readyToClose() && upperRightCloseButton.containsPoint(x, y))
            {
                Game1.playSound(closeSound);
                exitThisMenu();
            }

            _ = inventory.snapToClickableComponent(x, y);
            if (downArrow.containsPoint(x, y) && currentItemIndex < Math.Max(0, forSale.Count - 4))
            {
                downArrowPressed();
                Game1.playSound("shwip");
            }
            else if (upArrow.containsPoint(x, y) && currentItemIndex > 0)
            {
                upArrowPressed();
                Game1.playSound("shwip");
            }
            else if (scrollBar.containsPoint(x, y))
            {
                setValue("scrolling", true);
            }
            else if (!downArrow.containsPoint(x, y) && x > xPositionOnScreen + width && x < xPositionOnScreen + width + 128 && y > yPositionOnScreen && y < yPositionOnScreen + height)
            {
                setValue("scrolling", true);
                leftClickHeld(x, y);
                releaseLeftClick(x, y);
            }

            for (int i = 0; i < tabButtons.Count; i++)
            {
                if (tabButtons[i].containsPoint(x, y))
                {
                    switchTab(i);
                }
            }

            for(int i = 0; i < 4; i++)
            {
                int index = currentItemIndex + i;
                if (pricePlus[i].containsPoint(x, y) && purchased.Count > index && forSale.Contains(purchased[index].Key)) 
                {
                    purchase(purchased[index].Key, true, index);
                    return;
                }
                if (priceMin[i].containsPoint(x, y) && purchased.Count > index)
                {
                    purchase(purchased[index].Key, false, index);
                    return;
                }
            }
        }

        public void drawItemBox(SpriteBatch b, int i, ShopCachedTheme visualTheme, bool scrolling)
        {
            bool flag = canPurchaseCheck != null && !canPurchaseCheck(currentItemIndex + i);
            drawTextureBox(b, visualTheme.ItemRowBackgroundTexture, visualTheme.ItemRowBackgroundSourceRect, forSaleButtons[i].bounds.X, forSaleButtons[i].bounds.Y, forSaleButtons[i].bounds.Width, forSaleButtons[i].bounds.Height, (forSaleButtons[i].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) && !scrolling) ? visualTheme.ItemRowBackgroundHoverColor : Color.White, 4f, drawShadow: false);
            ISalable salable = forSale[currentItemIndex + i];
            ItemStockInformation stockInfo = itemPriceAndStock[salable];
            StackDrawType stackDrawType = GetStackDrawType(stockInfo, salable);
            string text = salable.DisplayName;
            if (salable.Stack > 1)
            {
                text = text + " x" + salable.Stack;
            }
            text += getPostfix.Invoke(salable);
            b.Draw(visualTheme.ItemIconBackgroundTexture, new Vector2(forSaleButtons[i].bounds.X + 32 - 12, forSaleButtons[i].bounds.Y + 24 - 4), visualTheme.ItemIconBackgroundSourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            Vector2 vector = new Vector2(forSaleButtons[i].bounds.X + 32 - 8, forSaleButtons[i].bounds.Y + 24);
            Color color = Color.White * ((!flag) ? 1f : 0.25f);
            int num = 1;
            if (itemPriceAndStock.TryGetValue(salable, out var value))
            {
                num = value.Stock;
            }

            salable.drawInMenu(b, vector, 1f, 1f, 0.9f, StackDrawType.HideButShowQuality, color, drawShadow: true);
            if (num != int.MaxValue && ShopId != "ClintUpgrade" && ((stackDrawType == StackDrawType.Draw && num > 1) || stackDrawType == StackDrawType.Draw_OneInclusive))
            {
                Utility.drawTinyDigits(num, b, vector + new Vector2(64 - Utility.getWidthOfTinyDigitString(num, 3f) + 3, 47f), 3f, 1f, color);
            }

            string text2 = text;
            bool flag2 = itemPriceAndStock[forSale[currentItemIndex + i]].Price > 0;
            if (SpriteText.getWidthOfString(text2) > width - (flag2 ? (150 + SpriteText.getWidthOfString(itemPriceAndStock[forSale[currentItemIndex + i]].Price + " ")) : 100) && text2.Length > (flag2 ? 27 : 37))
            {
                text2 = text2.Substring(0, flag2 ? 27 : 37);
                text2 += "...";
            }

            SpriteText.drawString(b, text2, forSaleButtons[i].bounds.X + 96 + 8, forSaleButtons[i].bounds.Y + 28, 999999, -1, 999999, flag ? 0.5f : 1f, 0.88f, junimoText: false, -1, "", visualTheme.ItemRowTextColor);

            int num2 = forSaleButtons[i].bounds.Right;
            int num3 = forSaleButtons[i].bounds.Y + 28 - 4;
            int y = forSaleButtons[i].bounds.Y + 44;
            if (itemPriceAndStock[forSale[currentItemIndex + i]].Price > 0)
            {
                SpriteText.drawString(b, itemPriceAndStock[forSale[currentItemIndex + i]].Price + " ", num2 - SpriteText.getWidthOfString(itemPriceAndStock[forSale[currentItemIndex + i]].Price + " ") - 60, forSaleButtons[i].bounds.Y + 28, 999999, -1, 999999, (getPlayerCurrencyAmount(Game1.player, currency) >= itemPriceAndStock[forSale[currentItemIndex + i]].Price && !flag) ? 1f : 0.5f, 0.88f, junimoText: false, -1, "", visualTheme.ItemRowTextColor);
                Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(forSaleButtons[i].bounds.Right - 52, forSaleButtons[i].bounds.Y + 40 - 4), new Rectangle(193 + currency * 9, 373, 9, 10), Color.White * ((!flag) ? 1f : 0.25f), 0f, Vector2.Zero, 4f, flipped: false, 1f, -1, -1, (!flag) ? 0.35f : 0f);
                num2 -= SpriteText.getWidthOfString(itemPriceAndStock[forSale[currentItemIndex + i]].Price + " ") + 96;
                num3 = forSaleButtons[i].bounds.Y + 20;
                y = forSaleButtons[i].bounds.Y + 28;
            }

            if (itemPriceAndStock[forSale[currentItemIndex + i]].TradeItem != null)
            {
                int count = 5;
                string tradeItem = itemPriceAndStock[forSale[currentItemIndex + i]].TradeItem;
                if (tradeItem != null && itemPriceAndStock[forSale[currentItemIndex + i]].TradeItemCount.HasValue)
                {
                    count = itemPriceAndStock[forSale[currentItemIndex + i]].TradeItemCount.Value;
                }

                bool flag3 = HasTradeItem(tradeItem, count);
                if (canPurchaseCheck != null && !canPurchaseCheck(currentItemIndex + i))
                {
                    flag3 = false;
                }

                float num4 = SpriteText.getWidthOfString("x" + count);
                ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(tradeItem);
                Texture2D texture = dataOrErrorItem.GetTexture();
                Rectangle sourceRect = dataOrErrorItem.GetSourceRect();
                Utility.drawWithShadow(b, texture, new Vector2((float)(num2 - 88) - num4, num3), sourceRect, Color.White * (flag3 ? 1f : 0.25f), 0f, Vector2.Zero, -1f, flipped: false, -1f, -1, -1, flag3 ? 0.35f : 0f);
                SpriteText.drawString(b, "x" + count, num2 - (int)num4 - 16, y, 999999, -1, 999999, flag3 ? 1f : 0.5f, 0.88f, junimoText: false, -1, "", visualTheme.ItemRowTextColor);
            }
        }

        public T getValue<T>(string name)
        {
            return (T)typeof(ShopMenu).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
        }

        public void setValue<T>(string name, T value)
        {
            typeof(ShopMenu).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, value);
        }

        public T involkeMethod<T>(string name)
        {
            return (T)typeof(ShopMenu).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, null);
        }

        public void nextMatch(TextBox sender)
        {
            int i = currentItemIndex;
            while (true)
            {
                i++;
                i %= forSale.Count;
                if (i == currentItemIndex) return;

                if (forSale[i].DisplayName.Contains(searchBox.Text, StringComparison.CurrentCultureIgnoreCase))
                {
                    currentItemIndex = i;
                    ModEntry._Helper.Reflection.GetMethod(this, "setScrollBarToCurrentIndex").Invoke();
                    updateSaleButtonNeighbors();
                    return;
                }
            }
        }

        public void purchase(ISalable item, bool purchase, int index)
        {
            int val = getPurchaseAmount(item);
            if (!purchase) val = -val;

            if (val != 0)
            {
                if (__purchase(item, val))
                    //itemPriceAndStock.Remove(item);
                    forSale.Remove(item);
                if (itemPriceAndStock[item].Stock != 0 && !forSale.Contains(item))
                    forSale.Add(item);
                if (purchased[index].Value == 0)
                {
                    purchased.RemoveAt(index);
                }
            }
            else
            {
                if (itemPriceAndStock[item].Price > 0) Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
                Game1.playSound("cancel");
            }
                
            currentItemIndex = Math.Max(0, Math.Min(forSale.Count - 4, currentItemIndex));
            updateSaleButtonNeighbors();
            setScrollBarToCurrentIndex();
        }

        public int getPurchaseAmount(ISalable item)
        {
            if (!Game1.oldKBState.IsKeyDown(Keys.LeftShift)) return 1;
            int currentVal, price = Math.Max(1, itemPriceAndStock[item].Price);

            if (Game1.oldKBState.IsKeyDown(Keys.LeftControl)) 
                currentVal = Game1.oldKBState.IsKeyDown(Keys.D1) ? 999 : 25;
            else currentVal = 5;

            int maxAmount = getPlayerCurrencyAmount(Game1.player, currency) / price;
            if (currentVal > maxAmount)
                currentVal = maxAmount;

            currentVal = Math.Min(currentVal, Math.Max(1, itemPriceAndStock[item].Stock));
            currentVal = Math.Min(currentVal, item.maximumStackSize());
            if (currentVal == -1) currentVal = 1;

            return currentVal;
        }

        public bool __purchase(ISalable item, int stockToBuy)
        {
            chargePlayer(Game1.player, currency, itemPriceAndStock[item].Price * stockToBuy);
            if (!_isStorageShop && item.actionWhenPurchased(ShopId))
            {
                if (item.IsRecipe)
                {
                    string key = heldItem.Name.Substring(0, heldItem.Name.IndexOf("Recipe") - 1);
                    if (item is Item obj && obj.Category == -7) Game1.player.cookingRecipes.Add(key, 0);
                    else Game1.player.craftingRecipes.Add(key, 0);
                    Game1.playSound("newRecipe");
                }
                heldItem = null;
            }
            else
            {
                if ((heldItem as Item)?.QualifiedItemId == "(O)858")
                {
                    Game1.player.team.addQiGemsToTeam.Fire(heldItem.Stack);
                    heldItem = null;
                }

                if (Game1.mouseClickPolling > 300)
                    if (purchaseRepeatSound != null) Game1.playSound(purchaseRepeatSound);
                else if (purchaseSound != null) Game1.playSound(purchaseSound);
            }

            if (itemPriceAndStock[item].Stock != int.MaxValue && !item.IsInfiniteStock())
            {
                HandleSynchedItemPurchase(item, Game1.player, stockToBuy);
                ItemStockInformation itemStockInformation = itemPriceAndStock[item];
                item.Stack = Math.Min(item.Stack, itemStockInformation.Stock);
                if (itemStockInformation.ItemToSyncStack != null)
                {
                    itemStockInformation.ItemToSyncStack.Stack = itemStockInformation.Stock;
                }
            }

            List<string> actionsOnPurchase = itemPriceAndStock[item].ActionsOnPurchase;
            if (actionsOnPurchase != null && actionsOnPurchase.Count > 0)
            {
                foreach (string item2 in itemPriceAndStock[item].ActionsOnPurchase)
                {
                    if (!TriggerActionManager.TryRunAction(item2, out var error, out var exception))
                    {
                        DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 4);
                        defaultInterpolatedStringHandler.AppendLiteral("Shop ");
                        defaultInterpolatedStringHandler.AppendFormatted(ShopId);
                        defaultInterpolatedStringHandler.AppendLiteral(" ignored invalid action '");
                        defaultInterpolatedStringHandler.AppendFormatted(item2);
                        defaultInterpolatedStringHandler.AppendLiteral("' on purchase of item '");
                        defaultInterpolatedStringHandler.AppendFormatted(item.QualifiedItemId);
                        defaultInterpolatedStringHandler.AppendLiteral("': ");
                        defaultInterpolatedStringHandler.AppendFormatted(error);
                        ModEntry._Monitor.Log(defaultInterpolatedStringHandler.ToStringAndClear(), LogLevel.Error);
                    }
                }
            }

            onPurchase(item, Game1.player, stockToBuy);

            if (itemPriceAndStock[item].Stock <= 0)
            {
                hoveredItem = null;
                return true;
            }
            return false;
        }

        public void previousMatch()
        {
            int i = currentItemIndex;
            while (true)
            {
                i--;
                if (i < 0) i = forSale.Count - 1;
                if (i == currentItemIndex) return;

                if (forSale[i].DisplayName.Contains(searchBox.Text, StringComparison.CurrentCultureIgnoreCase))
                {
                    currentItemIndex = i;
                    ModEntry._Helper.Reflection.GetMethod(this, "setScrollBarToCurrentIndex").Invoke();
                    updateSaleButtonNeighbors();
                    return;
                }
            }
        }
    }
}
