using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using System.Xml.Serialization;
using Object = StardewValley.Object;

namespace JojaExpress
{
    [XmlType("Mods_jojaExp_packedItem")]
    public class PackedItem : Object
    {
        [XmlIgnore]
        public static Texture2D PackageTexture;

        [XmlElement("levelPacked")]
        public readonly NetInt levelPacked = new();

        [XmlElement("itemPacked")]
        public readonly NetObjectList<Item> itemPacked = new();

        [XmlIgnore]
        public static PackedItemDefinition definition = new();

        public override string TypeDefinitionId
        {
            get
            {
                return "(JOJAEXP.PI)";
            }
        }

        public int LevelPacked
        {
            get { return levelPacked.Value; }
            set { levelPacked.Value = value; }
        }

        public List<Item> ItemPacked
        {
            get { return itemPacked.ToList(); }
            set { itemPacked.CopyFrom(value); }
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            base.NetFields.AddField(levelPacked, "levelPacked").AddField(itemPacked, "itemPacked");
        }

        public PackedItem() { }

        public PackedItem(string itemId, IDictionary<ISalable, ItemStockInformation> items): base(itemId[0] == '_' ? itemId[1..] : itemId, 1)
        {
            if (itemId[0] != '_') itemId = "_" + itemId;
            LevelPacked = -1;
            Edibility = -300;
            this.itemId.Value = itemId;
            _qualifiedItemId = "(JOJAEXP.PI)" + itemId;
            foreach (var p in items)
            {
                if (p.Key is not Item item) continue;
                int stackSize = item.maximumStackSize();
                for (int i = p.Value.Stock; i > 0; i -= stackSize)
                {
                    Item newItem = item.getOne();
                    newItem.Stack = Math.Min(stackSize, i);
                    itemPacked.Add(newItem);
                }
            }
        }

        public PackedItem(string itemId, int initialStack, int levelPacked = -1, int quality = 0): base(itemId[0] == '_' ? itemId[1..] : itemId, initialStack, quality: quality) 
        {
            if (itemId[0] != '_') itemId = "_" + itemId;
            LevelPacked = levelPacked;
            AdjustItemData();
            this.itemId.Value = itemId;
            _qualifiedItemId = "(JOJAEXP.PI)" + itemId;
            if (levelPacked != -1) return;
            
            if (QualifiedItemId == "(JOJAEXP.PI)_ofts.jojaExp.item.package.global")
            {
                Dictionary<string, int> dic = ModEntry.tobeReceived[0];
                foreach(KeyValuePair<string, int> p in dic)
                {
                    Item? sampleItem;
                    if (p.Key.StartsWith("rcp"))
                    {
                        sampleItem = ItemRegistry.Create(p.Key[3..]);
                        sampleItem.isRecipe.Value = true;
                    } else sampleItem = ItemRegistry.Create(p.Key);
                    sampleItem.Stack = p.Value;
                    itemPacked.Add(sampleItem);
                }
                if (ModEntry.tobeReceived.Count > 1) ModEntry.tobeReceived.RemoveAt(0);
                if (!Context.IsMainPlayer) ModEntry._Helper.Multiplayer.SendMessage(1, "ofts.jojaExp.tobeReceivedPoped");
            }
            else
            {
                LevelPacked = 0;
                AdjustItemData();
                return;
            }
        }

        public override bool canStackWith(ISalable other)
        {
            if(!base.canStackWith(other)) return false;
            if(other is not PackedItem packed) return false;
            return levelPacked == packed.levelPacked && itemPacked.Equals(packed.itemPacked);
        }

        public void AdjustItemData()
        {
            ParsedItemData? data = new PackedItemDefinition().GetData(ItemId);
            name = data?.InternalName ?? ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).InternalName;
            edibility.Value = -300;
            type.Value = "basic";
            base.Category = -999;

            switch (LevelPacked)
            {
                case -1: break;
                case 0:
                    {
                        Price *= (int)(0.8 * 25);
                        break;
                    }
                case 1:
                    {
                        Price *= (int)(0.6 * 100);
                        break;
                    }
                case 2:
                    {
                        Price *= (int)(0.5 * 999);
                        break;
                    }
            }
            Edibility = -300;
        }

        public Item getItem(int stack)
        {
            HashSet<string> baseContextTags = ItemContextTagManager.GetBaseContextTags(ItemId);
            if (baseContextTags.Contains("torch_item"))
            {
                return new Torch(stack, ItemId[1..]);
            }
            
            if (ItemId == "_812")
            {
                return new ColoredObject(ItemId[1..], stack, Color.Orange);
            }

            return ItemRegistry.Create(ItemId[1..].Replace("JOJAEXP.PI", "O"), stack, this.Quality);
        }

        protected override Item GetOneNew()
        {
            return new PackedItem(ItemId, 1);
        }

        protected override void GetOneCopyFrom(Item source)
        {
            base.GetOneCopyFrom(source);
            PackedItem? packed = source as PackedItem;
            if(packed != null)
            {
                LevelPacked = packed.LevelPacked;
                ItemPacked = packed.ItemPacked;
            }
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            Inventory inv = Game1.player.Items;
            if (LevelPacked == -1)
            {
                base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
                return;
            }

            if (drawShadow && !bigCraftable.Value && base.QualifiedItemId != "(O)590" && base.QualifiedItemId != "(O)SeedSpot")
            {
                spriteBatch.Draw(Game1.shadowTexture, location + new Vector2(32f, 48f), Game1.shadowTexture.Bounds, color * 0.5f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0004f);
            }
            spriteBatch.Draw(PackageTexture, location + new Vector2(32f, 32f), new Rectangle(LevelPacked * 16, 0, 16, 16), color * transparency, 0f, new Vector2(8, 8), 4f * scaleSize, SpriteEffects.None, layerDepth - 0.0003f);
            base.drawInMenu(spriteBatch, location + new Vector2(4f, 4f), scaleSize / 2, transparency, layerDepth - 0.0002f, StackDrawType.Hide, color, false);
            spriteBatch.Draw(PackageTexture, location + new Vector2(16, 32f), new Rectangle(0, LevelPacked * 8 + 16, 32, 8), color * transparency, 0f, new Vector2(4, 16), 2f * scaleSize, SpriteEffects.None, layerDepth);
            DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
        }

        public override int maximumStackSize()
        {
            return LevelPacked == -1 ? 1 : 999;
        }

        public override bool performUseAction(GameLocation location)
        {
            bool flag = Game1.eventUp || Game1.isFestival() || Game1.fadeToBlack || Game1.player.swimming.Value || Game1.player.bathingClothes.Value || Game1.player.onBridge.Value;
            if (flag) return false;
            switch (levelPacked.Value)
            {
                case -1:
                    {
                        bool cookingLearned = false, craftingLearned = false;
                        foreach(Item item in ItemPacked)
                        {
                            string shopId = $"ofts.JojaExp.joja{(ItemId.EndsWith("local") ? "Local" : "Global")}";
                            bool discard = item.actionWhenPurchased(shopId);
                            if (item.IsRecipe)
                            {
                                string key = item.Name.Substring(0, item.Name.IndexOf("Recipe") - 1);
                                if (item is Item obj && obj.Category == -7)
                                {
                                    Game1.player.cookingRecipes.Add(key, 0);
                                    cookingLearned = true;
                                }
                                else
                                {
                                    Game1.player.craftingRecipes.Add(key, 0);
                                    craftingLearned = true;
                                }
                                Game1.playSound("newRecipe");
                                continue;
                            }

                            if (discard) continue;

                            Game1.currentLocation.debris.Add(Game1.createItemDebris(item, Game1.player.Position, 0));
                        }
                        if (cookingLearned) Game1.addHUDMessage(new HUDMessage(ModEntry._Helper.Translation.Get("newCooking"), HUDMessage.achievement_type));
                        if (craftingLearned) Game1.addHUDMessage(new HUDMessage(ModEntry._Helper.Translation.Get("newCrafting"), HUDMessage.achievement_type));

                        break;
                    }
                case 0:
                    {
                        Game1.currentLocation.debris.Add(Game1.createItemDebris(getItem(25), Game1.player.Position, 0));
                        break;
                    }
                case 1:
                    {
                        Game1.currentLocation.debris.Add(Game1.createItemDebris(getItem(100), Game1.player.Position, 0));
                        break;
                    }
                case 2:
                    {
                        Game1.currentLocation.debris.Add(Game1.createItemDebris(getItem(999), Game1.player.Position, 0));
                        break;
                    }
            }
            return true;
        }
    }
}
