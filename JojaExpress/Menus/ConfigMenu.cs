using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace JojaExpress
{
    public class ConfigMenu : IClickableMenu
    {
        public static string addTranslation = "", categoryTranslation = "", addCategoryTranslation = "", addItemTranslation = "";
        public int loadAmount = 0, hoveredIndex = -1, selectedIndex = -1;
        public List<ClickableTextureComponent> itemIcons = new(), deleteIcons = new();
        public List<string> displayNames = new();
        public List<string> rawIDs = new();
        public Texture2D defaultTexture, menuTexture;
        public string noteString = "";
        public TextBox textbox;
        public ClickableTextureComponent option, modify, cancel;
        public bool addItem = true;
        public static readonly Rectangle defaultSource = new(257, 184, 16, 16),
            itemBackgroundSource = new(384, 396, 15, 15),
            rowBackgroundSource = new(384, 373, 18, 18);
        public static Dictionary<string, int> categoryDictionary = new();

        public ConfigMenu() 
        {
            menuTexture = ModEntry._Helper.ModContent.Load<Texture2D>("assets/price");
            width = 400;
            string[] ids = ModEntry.config.WholeSaleIds;
            loadAmount = ids.Length;
            defaultTexture = Game1.content.Load<Texture2D>("LooseSprites\\temporary_sprites_1");

            for (int i = 0; i < loadAmount; i++)
            {
                string id = ids[i], itemId = id[1..];
                rawIDs.Add(itemId);
                if (id[0] == 'C')
                {
                    displayNames.Add(categoryTranslation + Object.GetCategoryDisplayName(int.Parse(itemId)));
                    itemIcons.Add(new(new(8, i * 80 + 8, 64, 64), defaultTexture, defaultSource, 4f));
                }
                else
                {
                    ParsedItemData data = ItemRegistry.GetDataOrErrorItem("(O)" + itemId);
                    displayNames.Add(data.DisplayName);
                    itemIcons.Add(new(new(8, i * 80 + 8, 64, 64), data.GetTexture(), data.GetSourceRect(), 4f));
                }
                deleteIcons.Add(new(new(0, 0, 64, 64), Game1.mouseCursors, new(268, 470, 16, 16), 4f));
            }
            textbox = new(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Color.Brown) { Selected = false };
            option = new(new(0, 0, 256, 56), menuTexture, new(96, 0, 130, 26), 2f);
            modify = new(new(0, 0, 64, 64), Game1.mouseCursors, new(128, 256, 64, 64), 0.75f);
            cancel = new(new(0, 0, 64, 64), Game1.mouseCursors, new(192, 256, 64, 64), 0.75f);

            textbox.OnEnterPressed += checkInputText;
            textbox.OnTabPressed += (_) => addItem = !addItem;

            if (categoryDictionary.Count == 0) for (int i = 0; i > -104; i--) categoryDictionary.Add(Object.GetCategoryDisplayName(i), i);
        }

        public override bool readyToClose() => false;

        public void checkInputText(TextBox? _ = null)
        {
            textbox.Selected = false;
            string id = textbox.Text;
            if (addItem)
            {
                ParsedItemData data;
                if (Game1.objectData.ContainsKey(id))
                {
                    data = ItemRegistry.GetData("(O)" + id);
                }
                else
                {
                    IItemDataDefinition itemType = ItemRegistry.GetObjectTypeDefinition();
                    foreach (string allId in itemType.GetAllIds())
                    {
                        ParsedItemData tempData = itemType.GetData(allId);
                        if (tempData.InternalName == id || tempData.DisplayName == id)
                        {
                            data = tempData;
                            goto modifyItem;
                        }
                    }
                    noteString = ModEntry._Helper.Translation.Get("configMenu.error", new { type = "Object", source = id});
                    return;
                }

                modifyItem:
                if (selectedIndex == -1)
                {
                    rawIDs.Add('I' + data.ItemId);
                    displayNames.Add(data.DisplayName);
                    itemIcons.Add(new(new(8, loadAmount * 80 + 8, 64, 64), data.GetTexture(), data.GetSourceRect(), 4f));
                    deleteIcons.Add(new(new(0, 0, 64, 64), Game1.mouseCursors, new(268, 470, 16, 16), 4f));
                    loadAmount++;
                }
                else
                {
                    rawIDs[selectedIndex] = 'I' + data.ItemId;
                    displayNames[selectedIndex] = data.DisplayName;
                    itemIcons[selectedIndex].texture = data.GetTexture();
                    itemIcons[selectedIndex].sourceRect = data.GetSourceRect();
                }

                noteString = ModEntry._Helper.Translation.Get("configMenu.success", new { type = "item", source = data.DisplayName });
            }
            else
            {
                int categoryId;
                string categoryName;
                if (int.TryParse(id, out categoryId))
                {
                    categoryName = Object.GetCategoryDisplayName(categoryId);
                    if(categoryName == "")
                    {
                        noteString = ModEntry._Helper.Translation.Get("configMenu.error", new { type = "Category", source = id });
                        return;
                    }
                }
                else
                {
                    categoryName = id;
                    if (!categoryDictionary.TryGetValue(id, out categoryId))
                    {
                        noteString = ModEntry._Helper.Translation.Get("configMenu.error", new { type = "Category", source = id });
                        return;
                    }
                }

                if (selectedIndex == -1)
                {
                    rawIDs.Add("C" + categoryId);
                    displayNames.Add(categoryTranslation + categoryName);
                    itemIcons.Add(new(new(8, loadAmount * 80 + 8, 64, 64), defaultTexture, defaultSource, 4f));
                    deleteIcons.Add(new(new(0, 0, 64, 64), Game1.mouseCursors, new(268, 470, 16, 16), 4f));
                    loadAmount++;
                }
                else
                {
                    rawIDs[selectedIndex] = "C" + categoryId;
                    displayNames[selectedIndex] = categoryName;
                    itemIcons[selectedIndex].texture = defaultTexture;
                    itemIcons[selectedIndex].sourceRect = defaultSource;
                }
                noteString = ModEntry._Helper.Translation.Get("configMenu.success", new { type = "Category", source = categoryName });
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            for (int i = 0; i < loadAmount; i++)
            {
                if (deleteIcons[i].containsPoint(x, y))
                {
                    loadAmount--;
                    deleteIcons.RemoveAt(i);
                    itemIcons.RemoveAt(i);
                    displayNames.RemoveAt(i);
                    rawIDs.RemoveAt(i);
                    if(selectedIndex == i)
                    {
                        selectedIndex = -1;
                        textbox.Text = "";
                        textbox.Selected = false;
                    }
                    return;
                }
                if (new Rectangle(xPositionOnScreen, yPositionOnScreen + i * 80, 416, 80).Contains(x, y) && selectedIndex != i)
                {
                    selectedIndex = i;
                    textbox.Text = rawIDs[i];
                    return;
                }
            }
            if (textbox.X < x && textbox.Y < y && textbox.X + width > x && textbox.Y + height > y)
            {
                if (!textbox.Selected)
                {
                    textbox.SelectMe();
                    noteString = ModEntry._Helper.Translation.Get("configMenu.note" + (addItem ? "Object" : "Category"));
                }
                return;
            }
            else
            {
                textbox.Selected = false;
                noteString = "";
            }

            if (cancel.containsPoint(x, y)) textbox.Text = "";
            if (modify.containsPoint(x, y)) checkInputText();
            if (option.containsPoint(x, y)) addItem = !addItem;
        }

        public override void performHoverAction(int x, int y)
        {
            hoveredIndex = -1;
            for (int i = 0; i < loadAmount; i++)
            {
                if (new Rectangle(xPositionOnScreen, yPositionOnScreen + i * 80, 416, 80).Contains(x, y)) hoveredIndex = i;
                deleteIcons[i].tryHover(x, y);
            }
            option.tryHover(x, y);
            modify.tryHover(x, y);
            cancel.tryHover(x, y);
        }

        public override void draw(SpriteBatch b)
        {
            drawTextureBox(b, Game1.mouseCursors, rowBackgroundSource, xPositionOnScreen - 16, yPositionOnScreen - 16, 448, loadAmount * 80 + 32, Color.White, 4f, false);
            for (int i = 0; i < loadAmount; i++)
            {
                drawTextureBox(b, Game1.mouseCursors, itemBackgroundSource, xPositionOnScreen, yPositionOnScreen + i * 80, 416, 80, hoveredIndex == i ? Color.Wheat : Color.White, 2f, false);
                itemIcons[i].bounds.X = xPositionOnScreen + 8;
                itemIcons[i].bounds.Y = yPositionOnScreen + i * 80 + 8;
                itemIcons[i].draw(b);
                deleteIcons[i].bounds.X = xPositionOnScreen + 340;
                deleteIcons[i].bounds.Y = yPositionOnScreen + i * 80 + 10;
                deleteIcons[i].draw(b);
                b.DrawString(Game1.dialogueFont, displayNames[i], new(xPositionOnScreen + 80, yPositionOnScreen + i * 80 + 16), Color.Brown);
            }
            textbox.X = xPositionOnScreen - 20;
            textbox.Y = yPositionOnScreen + loadAmount * 80 + 100;
            textbox.Draw(b);
            if (textbox.Text == "" && !textbox.Selected) b.DrawString(Game1.smallFont, addTranslation, new(xPositionOnScreen - 4, yPositionOnScreen + loadAmount * 80 + 110), Color.Gray);
            option.bounds.X = xPositionOnScreen - 16;
            option.bounds.Y = yPositionOnScreen + loadAmount * 80 + 40;
            option.draw(b);
            b.DrawString(Game1.smallFont, addItem ? addItemTranslation : addCategoryTranslation, new(xPositionOnScreen - 8, yPositionOnScreen + loadAmount * 80 + 50), Color.Brown);
            modify.bounds.X = xPositionOnScreen + 180;
            modify.bounds.Y = yPositionOnScreen + loadAmount * 80 + 100;
            modify.draw(b);
            cancel.bounds.X = xPositionOnScreen + 240;
            cancel.bounds.Y = yPositionOnScreen + loadAmount * 80 + 100;
            cancel.draw(b);
        }
    }
}
