using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace JojaExpress
{
    public class PlayerInteractionHandler
    {
        public static IMobilePhoneApi Api;
        public static PerScreen<bool> isAppRunning = new(), isJPadRunning = new();

        public static void checkInv(object? sender, InventoryChangedEventArgs args)
        {
            foreach (Item item in args.Added)
            {
                if (item is StardewValley.Object obj && obj.QualifiedItemId == "(O)ofts.jojaExp.item.package.global" && obj.modData.Count() == 0 && ModEntry.tobeReceived.Count > 1)
                {
                    foreach (KeyValuePair<string, int> p in ModEntry.tobeReceived[0])
                    {
                        obj.modData.Add(p.Key, p.Value.ToString());
                    }
                    ModEntry.tobeReceived.RemoveAt(0);
                    if (!Context.IsMainPlayer) ModEntry._Helper.Multiplayer.SendMessage(1, "ofts.jojaExp.tobeReceivedPoped");
                }
            }
        }

        public static void handlePhone()
        {
            if (Api == null || Api.IsCallingNPC() || Api.GetAppRunning()) return;
            Api.SetAppRunning(true);
            Api.SetRunningApp("Joja Express");
            MobilePhoneRender.setBG("question");
            MobilePhoneRender.protrait.Add(new RenderPack(ModEntry._Helper.Translation.Get("app.welcome"), 30, 60, 250, 120, Game1.dialogueFont));
            MobilePhoneRender.protrait.Add(new RenderPack(ModEntry._Helper.Translation.Get("local"), 35, 203, 210, 60, null, true));
            MobilePhoneRender.protrait.Add(new RenderPack(ModEntry._Helper.Translation.Get("global"), 35, 255, 210, 60, null, true));
            MobilePhoneRender.protrait.Add(new RenderPack(ModEntry._Helper.Translation.Get("qi"), 35, 306, 210, 60, null, true));
            MobilePhoneRender.protrait.Add(new RenderPack(ModEntry._Helper.Translation.Get("help"), 35, 363, 210, 60, null, true));
            MobilePhoneRender.landscape.Add(new RenderPack(ModEntry._Helper.Translation.Get("app.welcome"), 55, 35, 150, 190, Game1.dialogueFont));
            MobilePhoneRender.landscape.Add(new RenderPack(ModEntry._Helper.Translation.Get("local"), 230, 50, 210, 60, null, true));
            MobilePhoneRender.landscape.Add(new RenderPack(ModEntry._Helper.Translation.Get("global"), 230, 101, 210, 60, null, true));
            MobilePhoneRender.landscape.Add(new RenderPack(ModEntry._Helper.Translation.Get("qi"), 230, 152, 210, 60, null, true));
            MobilePhoneRender.landscape.Add(new RenderPack(ModEntry._Helper.Translation.Get("help"), 230, 203, 210, 60, null, true));
            openMenu();
            isAppRunning.Value = true;
        }

        public static void sendMail(object? sender, DayEndingEventArgs e)
        {
            if (ModEntry.needMail)
                Game1.player.mailForTomorrow.Add("ofts.jojaExp.mail");
            ModEntry.needMail = false;
        }

        public static bool sentPurchasedItem(ISalable item, Farmer farmer, int amt)
        {
            if (ModEntry.localReceived.ContainsKey(item.QualifiedItemId)) ModEntry.localReceived[item.QualifiedItemId] += amt;
            else ModEntry.localReceived.Add(item.QualifiedItemId, amt);

            if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ShopMenu menu) menu.heldItem = null;
            return false;
        }

        public static bool mailPurchasedItem(ISalable item, Farmer farmer, int amt)
        {
            if (ModEntry.tobeReceived.Last().ContainsKey(item.QualifiedItemId)) ModEntry.tobeReceived.Last()[item.QualifiedItemId] += amt;
            else ModEntry.tobeReceived.Last().Add(item.QualifiedItemId, amt);

            if (!Context.IsMainPlayer) ModEntry._Helper.Multiplayer.SendMessage(new KeyValuePair<string, int>(item.QualifiedItemId, amt), "ofts.jojaExp.tobeReceivedAdded");

            if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ShopMenu menu) menu.heldItem = null;
            ModEntry.needMail = true;
            return false;
        }

        public static void openMenu()
        {
            List<KeyValuePair<string, string>> responses = new();
            if (!(ModEntry.fee_state == 1 && ModEntry.config.CloseWhenCCComplete) && ModEntry.config.EnableCommunity)
                responses.Add(new KeyValuePair<string, string>("local", ModEntry._Helper.Translation.Get("local")));
            if (ModEntry.config.EnableGlobal)
                responses.Add(new KeyValuePair<string, string>("global", ModEntry._Helper.Translation.Get("global")));
            if (ModEntry.config.EnableQi)
                responses.Add(new KeyValuePair<string, string>("qi", ModEntry._Helper.Translation.Get("qi")));
            if (responses.Count == 0)
            {
                Game1.multipleDialogues(ModEntry._Helper.Translation.Get("sorry").ToString().Split('$'));
                Game1.delayedActions.Add(
                    new DelayedAction(20, () => { GUI.needToCheckDialogueBox.Value = true; })
                    );
                handleNoteDisplay();
                return;
            }
            responses.Add(new KeyValuePair<string, string>("help", ModEntry._Helper.Translation.Get("help")));
            responses.Add(new KeyValuePair<string, string>("__cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")));
            Game1.currentLocation.ShowPagedResponses(ModEntry._Helper.Translation.Get("prompt"), responses, delegate (string callId)
            {
                switch (callId)
                {
                    case "local":
                        {
                            ModEntry.localReceived = new();
                            GUI.openMenu("ofts.JojaExp.jojaLocal", ModEntry.localReceived, GUI.getPostFixForLocalItem);
                            break;
                        }
                    case "global":
                        {
                            GUI.openMenu("ofts.JojaExp.jojaGlobal", ModEntry.tobeReceived.Last(), GUI.getPostFixForItem);
                            break;
                        }
                    case "qi":
                        {
                            if(Utility.TryOpenShopMenu("QiGemShop", "AnyOrNone"))
                            {
                                Game1.activeClickableMenu.exitFunction = exitMenu;
                            }
                            break;
                        }
                    case "help":
                        {
                            /*Game1.multipleDialogues(
                                ModEntry._Helper.Translation.Get(
                                    "info", new { percent = (ModEntry.getPriceModifier() - 1).ToString("P1") }
                                ).ToString().Split('$')
                            );
                            Game1.delayedActions.Add(
                                new DelayedAction(20, () => { GUI.needToCheckDialogueBox.Value = true; })
                                );
                            handleNoteDisplay();*/
                            handleHelpDisplay();
                            break;
                        }
                    case "__cancel":
                        {
                            exitMenu();
                            break;
                        }
                }
            }, false, false);
        }

        public static void handleHelpDisplay()
        {
            if((Api != null && isAppRunning.Value) || isJPadRunning.Value)
            {
                MobilePhoneRender.protrait.Clear();
                MobilePhoneRender.landscape.Clear();
                MobilePhoneRender.setBG("question");
                MobilePhoneRender.protrait.Add(new RenderPack(ModEntry._Helper.Translation.Get("app.help"), 30, 60, 250, 120, Game1.dialogueFont));
                MobilePhoneRender.protrait.Add(new RenderPack(ModEntry._Helper.Translation.Get("local"), 35, 203, 210, 60, null, true));
                MobilePhoneRender.protrait.Add(new RenderPack(ModEntry._Helper.Translation.Get("global"), 35, 255, 210, 60, null, true));
                MobilePhoneRender.protrait.Add(new RenderPack(ModEntry._Helper.Translation.Get("qi"), 35, 306, 210, 60, null, true));
                MobilePhoneRender.protrait.Add(new RenderPack(Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel"), 35, 363, 210, 60, null, true));
                MobilePhoneRender.landscape.Add(new RenderPack(ModEntry._Helper.Translation.Get("app.help"), 55, 35, 150, 190, Game1.dialogueFont));
                MobilePhoneRender.landscape.Add(new RenderPack(ModEntry._Helper.Translation.Get("local"), 230, 50, 210, 60, null, true));
                MobilePhoneRender.landscape.Add(new RenderPack(ModEntry._Helper.Translation.Get("global"), 230, 101, 210, 60, null, true));
                MobilePhoneRender.landscape.Add(new RenderPack(ModEntry._Helper.Translation.Get("qi"), 230, 152, 210, 60, null, true));
                MobilePhoneRender.landscape.Add(new RenderPack(Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel"), 230, 203, 210, 60, null, true));
            }

            List<KeyValuePair<string, string>> responses = new()
            {
                new KeyValuePair<string, string>("local", ModEntry._Helper.Translation.Get("local")),
                new KeyValuePair<string, string>("global", ModEntry._Helper.Translation.Get("global")),
                new KeyValuePair<string, string>("qi", ModEntry._Helper.Translation.Get("qi")),
                new KeyValuePair<string, string>("__cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel"))
            };
            Game1.currentLocation.ShowPagedResponses(ModEntry._Helper.Translation.Get("app.help"), responses, delegate (string callId)
            {
                if(callId == "__cancel")
                {
                    exitMenu();
                    return;
                }

                Game1.multipleDialogues(
                    ModEntry._Helper.Translation.Get(
                        callId + "_help", new { percent = (ModEntry.getPriceModifier() - 1).ToString("P1") }
                    ).ToString().Split('$')
                );

                Game1.delayedActions.Add(
                    new DelayedAction(20, () => { 
                        GUI.needToCheckDialogueBox.Value = true;
                        GUI.returnToHelpPage.Value = true;
                    })
                );

                handleNoteDisplay();
            }, false, false);
        }

        public static void handleNoteDisplay()
        {
            if ((isJPadRunning.Value || isAppRunning.Value) && Game1.activeClickableMenu is DialogueBox dialogue)
            {
                MobilePhoneRender.setBG("dialogue");
                MobilePhoneRender.protrait.Clear();
                MobilePhoneRender.landscape.Clear();
                MobilePhoneRender.protrait.Add(new VolatileRenderPack(dialogue.getCurrentString, 25, 40, 240, 400, null));
                MobilePhoneRender.landscape.Add(new VolatileRenderPack(dialogue.getCurrentString, 40, 25, 400, 240, null));
            }
        }

        public static void exitMenu()
        {
            if(isAppRunning.Value)
            {
                Api.SetAppRunning(false);
                isAppRunning.Value = false;
                MobilePhoneRender.protrait.Clear();
                MobilePhoneRender.landscape.Clear();
            }
            else if (isJPadRunning.Value)
            {
                isJPadRunning.Value = false;
                MobilePhoneRender.protrait.Clear();
                MobilePhoneRender.landscape.Clear();
            }
        }

        public static void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (ModEntry.config != null && ModEntry.config.Open.JustPressed() && ModEntry.config.OpenByKey && 
                DataLoader.Shops(Game1.content).ContainsKey("ofts.JojaExp.jojaLocal") && 
                Context.IsWorldReady && Game1.activeClickableMenu == null)
            {
                openMenu();
                return;
            }
            if (!Context.IsWorldReady || Game1.activeClickableMenu != null) return;
            bool found = false;
            foreach (InputButton key in Game1.options.actionButton)
            {
                if (e.IsDown(key.ToSButton()))
                {
                    found = true;
                    break;
                }
            }
            foreach (InputButton key in Game1.options.useToolButton)
            {
                if (e.IsDown(key.ToSButton()))
                {
                    found = true;
                    break;
                }
            }
            if (!found) return;
            if (Game1.player.ActiveObject != null && (
                Game1.player.ActiveObject.QualifiedItemId == "(O)ofts.jojaExp.item.package.global" ||
                Game1.player.ActiveObject.QualifiedItemId == "(O)ofts.jojaExp.item.package.local"))
            {
                foreach (KeyValuePair<string, NetString> p in Game1.player.ActiveObject.modData.FieldDict)
                {
                    try
                    {
                        for (int cnt = int.Parse(p.Value.Value); cnt > 0; cnt -= 999)
                        {
                            Game1.currentLocation.debris.Add(Game1.createItemDebris(ItemRegistry.Create(p.Key, Math.Min(cnt, 999)), Game1.player.Position, 0));
                        }
                    }
                    catch (Exception ex)
                    {
                        ModEntry._Monitor.Log($"JojaExp cannot unpack the following item: \n{ex}", LogLevel.Error);
                    }
                }
                Game1.player.reduceActiveItemByOne();
            }
            else if (Game1.player.ActiveItem != null && ModEntry.config != null && ModEntry.config.OpenByPad &&
                Game1.player.ActiveItem.QualifiedItemId == "(T)ofts.jojaExp.item.jpad" && 
                DataLoader.Shops(Game1.content).ContainsKey("ofts.JojaExp.jojaLocal") && 
                Context.IsWorldReady && Game1.activeClickableMenu == null)
            {
                isJPadRunning.Value = true;
                MobilePhoneRender.setBG("question");
                MobilePhoneRender.protrait.Add(new RenderPack(ModEntry._Helper.Translation.Get("app.welcome"), 30, 60, 250, 120, Game1.dialogueFont));
                MobilePhoneRender.protrait.Add(new RenderPack(ModEntry._Helper.Translation.Get("local"), 35, 203, 210, 60, null, true));
                MobilePhoneRender.protrait.Add(new RenderPack(ModEntry._Helper.Translation.Get("global"), 35, 255, 210, 60, null, true));
                MobilePhoneRender.protrait.Add(new RenderPack(ModEntry._Helper.Translation.Get("qi"), 35, 306, 210, 60, null, true));
                MobilePhoneRender.protrait.Add(new RenderPack(ModEntry._Helper.Translation.Get("help"), 35, 363, 210, 60, null, true));
                MobilePhoneRender.landscape.Add(new RenderPack(ModEntry._Helper.Translation.Get("app.welcome"), 55, 35, 150, 190, Game1.dialogueFont));
                MobilePhoneRender.landscape.Add(new RenderPack(ModEntry._Helper.Translation.Get("local"), 230, 50, 210, 60, null, true));
                MobilePhoneRender.landscape.Add(new RenderPack(ModEntry._Helper.Translation.Get("global"), 230, 101, 210, 60, null, true));
                MobilePhoneRender.landscape.Add(new RenderPack(ModEntry._Helper.Translation.Get("qi"), 230, 152, 210, 60, null, true));
                MobilePhoneRender.landscape.Add(new RenderPack(ModEntry._Helper.Translation.Get("help"), 230, 203, 210, 60, null, true));
                openMenu();
            }
        }
    }
}
