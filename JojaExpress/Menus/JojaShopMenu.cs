using Force.DeepCloner;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData.Shops;
using StardewValley.Menus;

namespace JojaExpress
{
    [Obsolete("Too shitty", false)]
    public partial class JojaShopMenu : ShopMenu
    {
        Func<ISalable, string> getPostfix;
        ClickableTextureComponent searchTexture, previousButton, nextButton, cartButton;
        ClickableTextureComponent[] priceBG, pricePlus, priceMin;
        Texture2D tmp, price;
        public TextBox searchBox;
        public bool viewingCart = false;
        public string currentOption = "";
        public List<KeyValuePair<ISalable, int>> purchased;
        public Dictionary<string, int> knownPurchased;

        public JojaShopMenu(string shopId, ShopData shopData, ShopOwnerData ownerData, Dictionary<string, int> knownPurchased, Func<ISalable, string> getPostfix)
            : base(shopId, shopData, ownerData, null)
        {
            this.onPurchase = this.onPurchaseFunc;
            exitFunction = customExit;
            this.getPostfix = getPostfix;
            this.knownPurchased = knownPurchased;
            tmp = ModEntry._Helper.ModContent.Load<Texture2D>("assets/Search");
            price = ModEntry._Helper.ModContent.Load<Texture2D>("assets/price");
            cartButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen, yPositionOnScreen - 64, width, 64), 
                ModEntry._Helper.ModContent.Load<Texture2D>("assets/cart_button"), 
                new Rectangle(0, 0, 270, 16), 4f
            );
            searchTexture = new ClickableTextureComponent(
                new(xPositionOnScreen - 8, yPositionOnScreen + height - inventory.height + 14 * 4, 65 * 4, 48 * 4), 
                tmp, new(0, 0, 65, 48) ,4f);
            searchBox = new(null, null, Game1.smallFont, Color.Red)
            {
                X = xPositionOnScreen + 18,
                Y = yPositionOnScreen + height - inventory.height + 32 * 4,
                Width = 48 * 4,
                Height = 64,
                Selected = false
            };
            searchBox.OnEnterPressed += nextMatch;
            Game1.keyboardDispatcher.Subscriber = searchBox;
            previousButton = upArrow.ShallowClone();
            nextButton = downArrow.ShallowClone();
            previousButton.bounds = new(
                xPositionOnScreen + 4 + 44, yPositionOnScreen + height - inventory.height + 44 * 4,
                previousButton.bounds.Width, previousButton.bounds.Height);
            nextButton.bounds = new(
                xPositionOnScreen + 4 + 132, yPositionOnScreen + height - inventory.height + 44 * 4,
                nextButton.bounds.Width, nextButton.bounds.Height);

            MobilePhoneRender.setBG("shop");
            MobilePhoneRender.protrait.Clear();
            MobilePhoneRender.landscape.Clear();

            List<Func<ISalable>> funcs = new(4)
            {
                () => forSale[currentItemIndex],
                () => forSale[currentItemIndex + 1],
                () => forSale[currentItemIndex + 2],
                () => forSale[currentItemIndex + 3]
            };
            //Func<ISalable, string> postfixOnPhone = shopId == "ofts.JojaExp.jojaLocal" ? GUI.getPostFixForLocalItemOnPhone : GUI.getPostFixForItemOnPhone;
            for (int i = 0; i < 4; i++)
            {
                MobilePhoneRender.protrait.Add(new TexturedRenderPack(funcs[i], 35 - 16, 61 + 100 * i - 16, 0.5f));
                MobilePhoneRender.protrait.Add(new VolatileRenderPack(funcs[i], 35, 100 + 100 * i, 220, 55, Game1.dialogueFont));
                //MobilePhoneRender.protrait.Add(new VolatileRenderPack(postfixOnPhone, funcs[i], 200, 65 + 100 * i, 100, 50, null));
                MobilePhoneRender.landscape.Add(new TexturedRenderPack(funcs[i], 64 - 16, 38 + 59 * i - 16, 0.5f));
                MobilePhoneRender.landscape.Add(new VolatileRenderPack(funcs[i], 100, 38 + 59 * i, 290, 55, Game1.dialogueFont));
                //MobilePhoneRender.landscape.Add(new VolatileRenderPack(postfixOnPhone, funcs[i], 390, 38 + 59 * i, 100, 50, null));
            }
            currentOption = ModEntry._Helper.Translation.Get("view_cart");

            purchased = new();
            foreach (var p in knownPurchased)
            {
                purchased.Add(new KeyValuePair<ISalable, int>(ItemRegistry.Create(p.Key), p.Value));
            }

            priceBG = new ClickableTextureComponent[4];
            pricePlus = new ClickableTextureComponent[4];
            priceMin = new ClickableTextureComponent[4];

            for(int i = 0; i < 4; i++)
            {
                priceBG[i] = new ClickableTextureComponent(new Rectangle(forSaleButtons[i].bounds.X + 800, forSaleButtons[i].bounds.Y + 24, 96 * 2, 32 * 2), price, new Rectangle(0, 0, 96, 32), 2f);
                priceMin[i] = new ClickableTextureComponent(new Rectangle(forSaleButtons[i].bounds.X + 800 + 12, forSaleButtons[i].bounds.Y + 24 + 12, 40, 40), price, new Rectangle(6, 6, 20, 20), 2f);
                pricePlus[i] = new ClickableTextureComponent(new Rectangle(forSaleButtons[i].bounds.X + 800 + 140, forSaleButtons[i].bounds.Y + 24 + 12, 40, 40), price, new Rectangle(70, 6, 20, 20), 2f);
            }
        }

        public void customExit()
        {
            foreach (var p in purchased)
            {
                if (knownPurchased.ContainsKey(p.Key.QualifiedItemId))
                {
                    knownPurchased[p.Key.QualifiedItemId] += p.Value;
                }
                else knownPurchased.Add(p.Key.QualifiedItemId, p.Value);
            }
            PlayerInteractionHandler.exitMenu();
        }

        public bool onPurchaseFunc(ISalable item, Farmer farmer, int amt, ItemStockInformation _)
        {
            bool found = false;
            for (int i = 0; i < purchased.Count; i++)
            {
                if (purchased[i].Key.canStackWith(item))
                {
                    purchased[i] = new KeyValuePair<ISalable, int>(item, purchased[i].Value + amt);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                purchased.Add(new KeyValuePair<ISalable, int>(item, amt));
            }
            heldItem = null;
            return false;
        }

        public override void receiveKeyPress(Keys key)
        {
            if (!searchBox.Selected && !Game1.options.doesInputListContain(Game1.options.menuButton, key))
            {
                base.receiveKeyPress(key);
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (viewingCart)
            {
                if (cartButton.containsPoint(x, y))
                {
                    viewingCart = false;
                    currentItemIndex = 0;
                    currentOption = ModEntry._Helper.Translation.Get("view_cart");
                    return;
                }
                alternateLeftClick(x, y);
                return;
            }
            base.receiveLeftClick(x, y, playSound);
            searchBox.Update();
            if(previousButton.containsPoint(x, y))
            {
                previousMatch();
            }
            else if(nextButton.containsPoint(x, y))
            {
                nextMatch(null);
            }
            else if (cartButton.containsPoint(x, y))
            {
                viewingCart = true;
                currentItemIndex = 0;
                currentOption = ModEntry._Helper.Translation.Get("view_shop");
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (!viewingCart) base.receiveRightClick(x, y, playSound);
        }

        public void drawSearchBox(SpriteBatch b)
        {
            if (viewingCart) return;
            searchTexture.draw(b);
            b.DrawString(Game1.dialogueFont, ModEntry._Helper.Translation.Get("search"), new(
                xPositionOnScreen + 4, yPositionOnScreen + height - inventory.height + 18 * 4
                ), Color.Brown);
            string textt = searchBox.Text;
            Vector2 vectort = Game1.smallFont.MeasureString(searchBox.Text);
            while (vectort.X > (float)searchBox.Width)
            {
                textt = textt.Substring(1);
                vectort = Game1.smallFont.MeasureString(textt);
            }

            bool flagt = Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 >= 500.0;
            if (flagt && searchBox.Selected)
            {
                b.Draw(Game1.staminaRect, new Rectangle(searchBox.X + 10 + (int)vectort.X + 2, searchBox.Y + 2, 4, 32), Color.Red);
            }
            b.DrawString(Game1.smallFont, searchBox.Text, new Vector2(searchBox.X + 10, searchBox.Y + 2), Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
            previousButton.draw(b);
            nextButton.draw(b);
        }

        public void drawCartBox(SpriteBatch b, int i, ShopCachedTheme visualTheme, bool scrolling)
        {
            if (currentItemIndex + i >= purchased.Count) return;

            drawTextureBox(
                b, visualTheme.ItemRowBackgroundTexture, visualTheme.ItemRowBackgroundSourceRect, 
                forSaleButtons[i].bounds.X, forSaleButtons[i].bounds.Y, 
                forSaleButtons[i].bounds.Width, forSaleButtons[i].bounds.Height, 
                (forSaleButtons[i].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) 
                && !scrolling) ? visualTheme.ItemRowBackgroundHoverColor : Color.White, 
                4f, drawShadow: false
            );
            b.Draw(
                visualTheme.ItemIconBackgroundTexture, 
                new Vector2(forSaleButtons[i].bounds.X + 32 - 12, forSaleButtons[i].bounds.Y + 24 - 4), 
                visualTheme.ItemIconBackgroundSourceRect, Color.White, 0f, Vector2.Zero, 4f, 
                SpriteEffects.None, 1f
            );
            Vector2 vector = new Vector2(forSaleButtons[i].bounds.X + 32 - 8, forSaleButtons[i].bounds.Y + 24);
            Color color = Color.White;
            ISalable salable = purchased[currentItemIndex + i].Key;
            int num = purchased[currentItemIndex + i].Value;
            salable.drawInMenu(b, vector, 1f, 1f, 0.9f, StackDrawType.HideButShowQuality, color, drawShadow: true);
            SpriteText.drawString(
                b, salable.DisplayName, forSaleButtons[i].bounds.X + 96 + 8, 
                forSaleButtons[i].bounds.Y + 28, color: visualTheme.ItemRowTextColor
            );
            priceBG[i].draw(b);
            pricePlus[i].draw(b);
            priceMin[i].draw(b);
            int numLength = SpriteText.getWidthOfString(num + "");
            SpriteText.drawString(
                b, num + "", forSaleButtons[i].bounds.X + 860 + (70 - numLength)/2, forSaleButtons[i].bounds.Y + 32, color: Color.Gray
            );
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            searchTexture = new ClickableTextureComponent(
                new(xPositionOnScreen - 8, yPositionOnScreen + height - inventory.height - 12 + 17 * 4, 65 * 4, 48 * 4),
                tmp, new(0, 0, 65, 48), 4f);
            searchBox.X = xPositionOnScreen + 18;
            searchBox.Y = yPositionOnScreen + height - inventory.height + 32 * 4;
            searchBox.Width = 48 * 4;
            searchBox.Height = 64;

            previousButton.bounds = new(
                xPositionOnScreen + 4 + 44, yPositionOnScreen + height - inventory.height + 44 * 4,
                previousButton.bounds.Width, previousButton.bounds.Height);
            nextButton.bounds = new(
                xPositionOnScreen + 4 + 132, yPositionOnScreen + height - inventory.height + 44 * 4,
                nextButton.bounds.Width, nextButton.bounds.Height);
            cartButton.bounds = new Rectangle(xPositionOnScreen, yPositionOnScreen - 64, width, 64);
            for (int i = 0; i < 4; i++)
            {
                priceBG[i].bounds = new Rectangle(forSaleButtons[i].bounds.X + 800, forSaleButtons[i].bounds.Y + 24, 96 * 2, 32 * 2);
                priceMin[i].bounds = new Rectangle(forSaleButtons[i].bounds.X + 800 + 12, forSaleButtons[i].bounds.Y + 24 + 12, 40, 40);
                pricePlus[i].bounds = new Rectangle(forSaleButtons[i].bounds.X + 800 + 140, forSaleButtons[i].bounds.Y + 24 + 12, 40, 40);
            }
        }

        public override void performHoverAction(int x, int y)
        {
            if (viewingCart)
            {
                upperRightCloseButton?.tryHover(x, y, 0.5f);
                hoverText = "";
                hoveredItem = null;
                hoverPrice = -1;
                boldTitleText = "";
                upArrow.tryHover(x, y);
                downArrow.tryHover(x, y);
                scrollBar.tryHover(x, y);
                if (getValue<bool>("scrolling"))
                {
                    return;
                }
                for (int i = 0; i < forSaleButtons.Count; i++)
                {
                    if (currentItemIndex + i < purchased.Count && forSaleButtons[i].containsPoint(x, y))
                    {
                        ISalable salable = purchased[currentItemIndex + i].Key;
                        hoverText = salable.getDescription();
                        boldTitleText = salable.DisplayName;
                        //hoverPrice = ((itemPriceAndStock != null && itemPriceAndStock.TryGetValue(salable, out var value)) ? value.Price : salable.salePrice());
                        hoveredItem = salable;
                        forSaleButtons[i].scale = Math.Min(forSaleButtons[i].scale + 0.03f, 1.1f);
                        if (pricePlus[i].containsPoint(x, y)) pricePlus[i].scale = Math.Min(pricePlus[i].scale + 0.06f, 2.2f);
                        else pricePlus[i].scale = Math.Max(2f, pricePlus[i].scale - 0.06f);
                        if (priceMin[i].containsPoint(x, y)) priceMin[i].scale = Math.Min(priceMin[i].scale + 0.06f, 2.2f);
                        else priceMin[i].scale = Math.Max(2f, pricePlus[i].scale - 0.06f);
                    }
                    else 
                    {
                        forSaleButtons[i].scale = Math.Max(1f, forSaleButtons[i].scale - 0.03f);
                        pricePlus[i].scale = Math.Max(2f, pricePlus[i].scale - 0.06f);
                        priceMin[i].scale = Math.Max(2f, pricePlus[i].scale - 0.06f);
                    }
                }
                return;
            }
            base.performHoverAction(x, y);
        }

        public override void draw(SpriteBatch b)
        {
            Rectangle scrollBarRunner = getValue<Rectangle>("scrollBarRunner");
            bool scrolling = getValue<bool>("scrolling");
            TemporaryAnimatedSpriteList animations = getValue<TemporaryAnimatedSpriteList>("animations");
            TemporaryAnimatedSprite poof = getValue<TemporaryAnimatedSprite>("poof");

            if (!Game1.options.showMenuBackground && !Game1.options.showClearBackgrounds && MobilePhoneRender.Api == null)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            }

            ShopCachedTheme visualTheme = VisualTheme;
            //Rectangle rec = new Rectangle(xPositionOnScreen + 8, yPositionOnScreen - 76, width - 16, 60);
            cartButton.draw(b);
            SpriteText.drawString(b, currentOption, xPositionOnScreen + 16, yPositionOnScreen - 48, width: width - 32, height: 64, color: Color.Brown, scroll_text_alignment: SpriteText.ScrollTextAlignment.Center);
            drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), xPositionOnScreen + width - inventory.width - 32 - 24, yPositionOnScreen + height - 256 + 40, inventory.width + 56, height - 448 + 20, Color.White, 4f);
            drawTextureBox(b, visualTheme.WindowBorderTexture, visualTheme.WindowBorderSourceRect, xPositionOnScreen, yPositionOnScreen, width, height - 256 + 32 + 4, Color.White, 4f);
            
            drawCurrency(b);
            drawSearchBox(b);

            for (int i = 0; i < forSaleButtons.Count; i++)
            {
                if (currentItemIndex + i >= forSale.Count) continue;
                if (!viewingCart) drawItemBox(b, i, visualTheme, scrolling);
                else drawCartBox(b, i, visualTheme, scrolling);
            }

            if (purchased.Count == 0 && viewingCart)
            {
                string str = ModEntry._Helper.Translation.Get("cart.none");
                SpriteText.drawString(b, str, xPositionOnScreen + width / 2 - SpriteText.getWidthOfString(str) / 2, yPositionOnScreen + height / 2 - 128);
            }

            inventory.draw(b);
            for (int num5 = animations.Count - 1; num5 >= 0; num5--)
            {
                if (animations[num5].update(Game1.currentGameTime))
                {
                    animations.RemoveAt(num5);
                }
                else
                {
                    animations[num5].draw(b, localPosition: true);
                }
            }

            poof?.draw(b);
            upArrow.draw(b);
            downArrow.draw(b);
            for (int j = 0; j < tabButtons.Count; j++)
            {
                tabButtons[j].draw(b);
            }

            if (forSale.Count > 4)
            {
                drawTextureBox(b, visualTheme.ScrollBarBackTexture, visualTheme.ScrollBarBackSourceRect, scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f);
                scrollBar.draw(b);
            }

            drawToolTip(b);
            upperRightCloseButton.draw(b);
            drawMouse(b);
        }
    }
}
