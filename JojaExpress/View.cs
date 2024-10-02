using StardewValley;

namespace JojaExpress
{
    public class View
    {
        private List<ISalable> list;
        private Dictionary<ISalable, ItemStockInformation> originalDic;
        public int Count => list.Count;

        public View(Dictionary<ISalable, ItemStockInformation> list)
        {
            originalDic = list;
            this.list = list.Keys.ToList().Where((salable) => originalDic[salable].Stock > 0).ToList();
        }

        public void filter(string word)
        {
            list.Clear();
            foreach (var pair in originalDic)
            {
                if(pair.Key.DisplayName.Contains(word, StringComparison.OrdinalIgnoreCase) && pair.Value.Stock > 0) list.Add(pair.Key);
            }
        }

        public ISalable this[int index]
        {
            get { return list[index]; }
            set { list[index] = value; }
        }

        public void RemoveAt(int index)
        {
            originalDic.Remove(list[index]);
            list.RemoveAt(index);
        }

        public ItemStockInformation getValue(int index)
        {
            return originalDic[list[index]];
        }
    }
}
