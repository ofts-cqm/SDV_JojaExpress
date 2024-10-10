using StardewModdingAPI.Events;
using StardewValley.GameData.Shops;
using StardewValley.GameData;
using StardewValley;
using System.Globalization;
using StardewValley.GameData.Objects;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.Tools;
using GenericModConfigMenu;
using StardewModdingAPI;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;

namespace JojaExpress
{
    public class LoadingManager
    {
        public static void loadModConfigMenu(IGenericModConfigMenuApi configMenu, ModConfig config, IManifest ModManifest, IModHelper Helper)
        {
            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => config = new ModConfig(),
                save: () => Helper.WriteConfig(config)
            );

            configMenu.AddKeybindList(
                mod: ModManifest,
                getValue: () => config.Open,
                setValue: value => config.Open = value,
                name: () => Helper.Translation.Get("open_name"),
                tooltip: () => Helper.Translation.Get("open_tip")
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                getValue: () => config.CarriageFee,
                setValue: value => config.CarriageFee = value,
                name: () => Helper.Translation.Get("fee_name"),
                tooltip: () => Helper.Translation.Get("fee_tip"),
                min: 0,
                formatValue: (value) => (value - 1).ToString("P1")
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                getValue: () => config.CarriageFee_NoJoja,
                setValue: value => config.CarriageFee_NoJoja = value,
                name: () => Helper.Translation.Get("fee_NoJoja_name"),
                tooltip: () => Helper.Translation.Get("fee_NoJoja_tip"),
                min: 0,
                formatValue: (value) => (value - 1).ToString("P1")
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                getValue: () => config.CarriageFee_Member,
                setValue: value => config.CarriageFee_Member = value,
                name: () => Helper.Translation.Get("fee_Member_name"),
                tooltip: () => Helper.Translation.Get("fee_Member_tip"),
                min: 0,
                formatValue: (value) => (value - 1).ToString("P1")
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.OpenByKey,
                setValue: value => config.OpenByKey = value,
                name: () => Helper.Translation.Get("openByKey"),
                tooltip: () => Helper.Translation.Get("openByKey_tooltip")
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.OpenByPad,
                setValue: value => config.OpenByPad = value,
                name: () => Helper.Translation.Get("openByPad"),
                tooltip: () => Helper.Translation.Get("openByPad_tooltip")
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.OpenByPhone,
                setValue: value => config.OpenByPhone = value,
                name: () => Helper.Translation.Get("openByPhone"),
                tooltip: () => Helper.Translation.Get("openByPhone_tooltip")
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.CloseWhenCCComplete,
                setValue: value => config.CloseWhenCCComplete = value,
                name: () => Helper.Translation.Get("CloseWhenCCComplete"),
                tooltip: () => Helper.Translation.Get("CloseWhenCCComplete")
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.EnableCommunity,
                setValue: value => config.EnableCommunity = value,
                name: () => Helper.Translation.Get("enablec"),
                tooltip: () => Helper.Translation.Get("enablec")
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.EnableGlobal,
                setValue: value => config.EnableGlobal = value,
                name: () => Helper.Translation.Get("enableg"),
                tooltip: () => Helper.Translation.Get("enableg")
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.EnableQi,
                setValue: value => config.EnableQi = value,
                name: () => Helper.Translation.Get("enableq"),
                tooltip: () => Helper.Translation.Get("enableq")
            );
        }

        public static IEnumerable<ItemQueryResult> loadByCategory(int category, HashSet<string> avoidItems)
        {
            List<ItemQueryResult> result = new();
            foreach (KeyValuePair<string, ObjectData> pair in Game1.objectData)
                if (pair.Value.Category == category) result.AddRange(loadById(pair.Key, avoidItems));
            return result;
        }

        public static IEnumerable<ItemQueryResult> loadById(string id, HashSet<string> avoidItems)
        {
            return avoidItems.Contains(id) ? new ItemQueryResult[0] : new ItemQueryResult[]
            {
                new(new PackedItem("_" + id, 1, 0)),
                new(new PackedItem("_" + id, 1, 1)),
                new(new PackedItem("_" + id, 1, 2))
            };
        }

        public static IEnumerable<ItemQueryResult> handleItemQuery(string key, string arguments, ItemQueryContext _, bool __, HashSet<string> avoidItemIds, Action<string, string> logError)
        {
            List<ItemQueryResult> empty = new();
            if (arguments[0] == 'C')
            {
                if (int.TryParse(arguments[1..], out var category))
                    return loadByCategory(category, avoidItemIds ?? new());
                else
                {
                    logError.Invoke(key, "Failed to find category");
                    return empty;
                }
            }
            else if (arguments[0] == 'I')
            {
                if (Game1.objectData.ContainsKey(arguments[1..]))
                    return loadById(arguments[1..], avoidItemIds ?? new());
                else
                {
                    logError.Invoke(key, "ID doesn't exist or is not an object");
                    return empty;
                }
            }
            return empty;
        }

        public static void updateTranslation(ITranslationHelper helper)
        {
            PackedItemDefinition.packedDescription = helper.Get("item.package.descrip.packed");
            PackedItemDefinition.packedName = helper.Get("item.package.packed");
        }
        public static void loadAsset(object? sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo("JojaExp\\string"))
            {
                e.LoadFrom(() =>
                {
                    return new Dictionary<string, string>()
                    {
                        { "display", ModEntry._Helper.Translation.Get("item.package.display") },
                        { "descrip_global", ModEntry._Helper.Translation.Get("item.package.descrip.global") },
                        { "descrip_local", ModEntry._Helper.Translation.Get("item.package.descrip.local") },
                        { "descrip_whole", ModEntry._Helper.Translation.Get("item.package.descrip.whole") },
                        { "descrip_jpad", ModEntry._Helper.Translation.Get("item.jpad.descrip") },
                        { "jpad", ModEntry._Helper.Translation.Get("item.jpad") }
                    };
                }, AssetLoadPriority.Medium);
            }
            else if (e.Name.IsEquivalentTo("Data/Shops"))
            {
                e.Edit(asset =>
                {
                    IDictionary<string, ShopData> data = asset.AsDictionary<string, ShopData>().Data;

                    //joja local
                    data.Add("ofts.JojaExp.jojaLocal", initShop());

                    // joja global
                    var shop = initShop();
                    QuantityModifier mdfr = new()
                    {
                        Modification = QuantityModifier.ModificationType.Multiply,
                        Id = "ofts.jojaExp.mdfr.0",
                        Amount = ModEntry.getPriceModifier()
                    };
                    ShopItemData sid0 = new()
                    {
                        ItemId = "ALL_ITEMS @requirePrice",
                        Id = "ofts.jojaExp.sid.0",
                        IgnoreShopPriceModifiers = true,
                        PriceModifiers = new() { }
                    };
                    sid0.PriceModifiers.Add(mdfr);
                    sid0.PriceModifierMode = QuantityModifier.QuantityModifierMode.Maximum;
                    shop.Items.Add(sid0);
                    data.Add("ofts.JojaExp.jojaGlobal", shop);

                    // joja wholesale
                    ShopData wholesale = initShop();
                    foreach(string id in ModEntry.config.WholeSaleIds)
                    {
                        wholesale.Items.Add(new() {ItemId = "jojaExp.getItem " + id, Id = "ofts.jojaExp.sid." + id });
                    }
                    data.Add("ofts.JojaExp.jojaWhole", wholesale);

                    // joja mart
                    data["Joja"].Items.Add(new ShopItemData() { 
                        ItemId = "(T)ofts.jojaExp.item.jpad",
                        Id = "ofts.jojaExp.paditem",
                        IgnoreShopPriceModifiers = false,
                        PriceModifiers = new() { }
                    });
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/mail"))
            {
                ModEntry._Monitor.Log("Updating Mails");
                e.Edit(asset =>
                {
                    IDictionary<string, string> mails = asset.AsDictionary<string, string>().Data;
                    string mail = ModEntry._Helper.Translation.Get("mail_format");
                    mails.Add("ofts.jojaExp.mail", mail);
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                ModEntry._Monitor.Log("Adding objects");
                e.Edit(asset =>
                {
                    IDictionary<string, ObjectData> objects = asset.AsDictionary<string, ObjectData>().Data;
                    ObjectData data = new()
                    {
                        Name = "jojaExp.globalPackage",
                        DisplayName = "[LocalizedText JojaExp\\string:display]",
                        Description = "[LocalizedText JojaExp\\string:descrip_global]",
                        Type = "Basic",
                        Category = -999,
                        Price = 0,
                        Texture = "LooseSprites/Giftbox",
                        SpriteIndex = 33,
                        IsDrink = false,
                        ExcludeFromFishingCollection = true,
                        ExcludeFromRandomSale = true,
                        ExcludeFromShippingCollection = true
                    };
                    objects.Add("ofts.jojaExp.item.package.global", data);
                    ObjectData data2 = new()
                    {
                        Name = "jojaExp.localPackage",
                        DisplayName = "[LocalizedText JojaExp\\string:display]",
                        Description = "[LocalizedText JojaExp\\string:descrip_local]",
                        Type = "Basic",
                        Category = -999,
                        Price = 0,
                        Texture = "LooseSprites/Giftbox",
                        SpriteIndex = 11,
                        IsDrink = false,
                        ExcludeFromFishingCollection = true,
                        ExcludeFromRandomSale = true,
                        ExcludeFromShippingCollection = true
                    };
                    objects.Add("ofts.jojaExp.item.package.local", data2);
                    ObjectData data3 = new()
                    {
                        Name = "jojaExp.wholePackage",
                        DisplayName = "[LocalizedText JojaExp\\string:display]",
                        Description = "[LocalizedText JojaExp\\string:descrip_whole]",
                        Type = "Basic",
                        Category = -999,
                        Price = 0,
                        Texture = "LooseSprites/Giftbox",
                        SpriteIndex = 99,
                        IsDrink = false,
                        ExcludeFromFishingCollection = true,
                        ExcludeFromRandomSale = true,
                        ExcludeFromShippingCollection = true
                    };
                    objects.Add("ofts.jojaExp.item.package.whole", data3);
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("JojaExp/assets/JPad"))
            {
                e.LoadFromModFile<Texture2D>("assets/Jpad", AssetLoadPriority.Low);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Tools"))
            {
                e.Edit(asset => {
                    IDictionary<string, ToolData> data = asset.AsDictionary<string, ToolData>().Data;
                    ToolData data3 = new()
                    {
                        ClassName = "GenericTool",
                        Name = "jojaExp.jpad",
                        DisplayName = "[LocalizedText JojaExp\\string:jpad]",
                        Description = "[LocalizedText JojaExp\\string:descrip_jpad]",
                        Texture = "JojaExp/assets/JPad",
                        SpriteIndex = 1,
                        SalePrice = 10000,
                        MenuSpriteIndex = 0,
                        UpgradeLevel = -1,
                        ApplyUpgradeLevelToDisplayName = false,
                        CanBeLostOnDeath = true,
                        AttachmentSlots = -1,
                        ConventionalUpgradeFrom = "",
                        UpgradeFrom = new(),
                        SetProperties = new(),
                        ModData = new(),
                        CustomFields = new()
                    };
                    data.Add("ofts.jojaExp.item.jpad", data3);
                });
            }
        }

        private static ShopData initShop()
        {
            ShopData shop = new()
            {
                Currency = 0,
                StackSizeVisibility = StackSizeVisibility.Show,
                OpenSound = "dwop",
                PurchaseSound = "purchaseClick",
                PurchaseRepeatSound = "purchaseRepeat",
                Owners = new() { initOwner() },
                SalableItemTags = new(),
                Items = new()
            };
            return shop;
        }

        private static ShopOwnerData initOwner()
        {
            ShopOwnerData owner = new();
            owner.Condition = "";
            owner.Portrait = "assets/joja";
            owner.Dialogues = new();
            owner.Id = "ofts.jojaExp.0";
            owner.Name = "AnyOrNone";
            return owner;
        }

        public static void fillShops(object? sender, AssetReadyEventArgs e)
        {
            if (e.Name.IsEquivalentTo("Data/Shops"))
            {
                ShopData? jojaLocal = null;
                List<ShopItemData> items = new();
                HashSet<string> ids = new HashSet<string>();
                foreach (KeyValuePair<string, ShopData> shop in DataLoader.Shops(Game1.content))
                {
                    if (shop.Value.Currency != 0) continue;
                    if (shop.Key == "ofts.JojaExp.jojaLocal")
                    {
                        jojaLocal = shop.Value;
                        continue;
                    }
                    if (shop.Key == "ofts.JojaExp.jojaGlobal") continue;

                    List<QuantityModifier>? modifiers = shop.Value.PriceModifiers;
                    foreach (ShopItemData item in shop.Value.Items)
                    {
                        if (item.ItemId == null || !ItemRegistry.IsQualifiedItemId(item.ItemId) || item.TradeItemId != null) continue;
                        if (ids.Contains(item.ItemId)) continue;
                        if (!item.IgnoreShopPriceModifiers && modifiers != null && modifiers.Count != 0)
                        {
                            if (item.PriceModifiers != null) item.PriceModifiers.AddRange(modifiers);
                            else item.PriceModifiers = modifiers;
                        }
                        try
                        {
                            var a = ItemRegistry.GetMetadata(item.ItemId).GetParsedData().DisplayName;
                            items.Add(item);
                            ids.Add(item.ItemId);
                        }catch (Exception ex)
                        {
                            ModEntry._Monitor.Log($"Failed to add {item.ItemId} to Joja Community. " +
                                $"Technical details: {ex.Message}. " +
                                $"Joja Express already handled this exception. Your game will still run normally");
                        }
                    }
                    if (modifiers != null && modifiers.Count != 0)
                    {
                        shop.Value.PriceModifiers = null;
                    }
                }
                if (jojaLocal == null) throw new Exception("failed to find jojalocal market");
                
                items.Sort((a, b) =>
                {
                    return string.Compare(ItemRegistry.GetMetadata(a.ItemId).GetParsedData().DisplayName,
                        ItemRegistry.GetMetadata(b.ItemId).GetParsedData().DisplayName,
                        new CultureInfo(ModEntry._Helper.Translation.Locale), CompareOptions.IgnoreSymbols);
                });
                jojaLocal.Items.AddRange(items);

            }
        }
    }
}
