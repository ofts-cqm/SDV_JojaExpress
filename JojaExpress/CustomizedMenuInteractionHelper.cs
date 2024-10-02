using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace JojaExpress
{
    public partial class CustomizedShop: IClickableMenu
    {
        public override void performHoverAction(int x, int y)
        {
            if (viewingNotification) { _notificationHover(x, y); return; }
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
            if (viewingNotification) { _notificationClick(x, y); return; }
            if (searchBox.Selected) searchBox.Selected = false;
            if (!viewingCart && cartButton.containsPoint(x, y))
            {
                viewingCart = true;
                currentItemIndex = 0;
                currentList = new View(purchased);
                searchBox.Text = "";
                return;
            }
            if (viewingCart && backButton.containsPoint(x, y))
            {
                viewingCart = false;
                currentItemIndex = 0;
                currentList = new View(forSale);
                searchBox.Text = "";
                return;
            }
            if (viewingCart && checkOutButton.containsPoint(x, y))
            {
                _tryCheckOut();
                return;
            }
            if (upperRightCloseButton.containsPoint(x, y))
            {
                _tryCloseMenu();
                return;
            }

            if (search.containsPoint(x, y)) updateSearchBox();
            else if(unSearch.containsPoint(x, y))
            {
                searchBox.Selected = false;
                searchBox.Text = "";
                currentList.filter("");
            }
            else if (searchTab.containsPoint(x, y)) searchBox.SelectMe();

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
            if (searchBox.Selected) searchBox.Selected = false;
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
            if (viewingNotification || searchBox.Selected) return;
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
    }
}
