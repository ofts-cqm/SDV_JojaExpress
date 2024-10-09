using Force.DeepCloner;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;

namespace JojaExpress
{
    public class PackedItemDefinition : ObjectDataDefinition
    {
        public static Translation? packedName, packedDescription;

        public override string Identifier => "(JOJAEXP.PI)";

        public override string StandardDescriptor => "JOJAEXP.PI";

        public override Item CreateItem(ParsedItemData data) => data == null ? 
            throw new ArgumentNullException("data") : new PackedItem(data.ItemId, 1, -1);

        public override IEnumerable<string> GetAllIds()
        {
            List<string> list = new();
            list.AddRange(base.GetAllIds());
            for (int i = 0; i < list.Count; i++)
                list[i] = "_" + list[i];
            return list;
        }

        public override ParsedItemData GetData(string itemId)
        {
            ObjectData? rawData = GetRawData(itemId[0] == '_' ? itemId[1..] : itemId)?.DeepClone() ?? null;
            if (rawData == null)  return null;
            if (itemId.StartsWith("_ofts"))
            {
                return new ParsedItemData(
                this, itemId, rawData.SpriteIndex, rawData.Texture ?? "Maps\\springobjects",
                rawData.Name, TokenParser.ParseText(rawData.DisplayName),
                TokenParser.ParseText(rawData.Description), rawData.Category, rawData.Type, rawData,
                isErrorItem: false, rawData.ExcludeFromRandomSale);
            }

            rawData.Name = "Packed " + rawData.Name;
            rawData.DisplayName = (packedName ?? "Packed ") + rawData.DisplayName;
            rawData.Description = packedDescription?.Tokens(new { name = rawData.DisplayName }) ?? 
                $"A box of packed {rawData.DisplayName}, it looks like a lot. It seems that it can be opened directly, better than Mr. Qi's Mystery Box.";
            rawData.Type = "basic";
            rawData.Category = -999;

            return new ParsedItemData(
                this, itemId, rawData.SpriteIndex, rawData.Texture ?? "Maps\\springobjects", 
                rawData.Name, TokenParser.ParseText(rawData.DisplayName),
                TokenParser.ParseText(rawData.Description), -999, rawData.Type, rawData, 
                isErrorItem: false, rawData.ExcludeFromRandomSale);
        }
    }
}
