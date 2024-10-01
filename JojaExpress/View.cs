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
            this.list = list.Keys.ToList();
            originalDic = list;
        }

        public void filter(string word)
        {
            list.Clear();
            foreach (ISalable salable in originalDic.Keys)
            {
                if(salable.DisplayName.Contains(word)) list.Add(salable);
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
