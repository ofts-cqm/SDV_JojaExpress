using StardewValley;

namespace JojaExpress
{
    public class View
    {
        private List<KeyValuePair<ISalable, ItemStockInformation>> list;
        private List<int> indices;
        public int Count => indices.Count;

        public View(List<KeyValuePair<ISalable, ItemStockInformation>> list)
        {
            this.list = list;
            indices = new List<int>(list.Count);
            for(int i = 0; i < list.Count; i++)
            {
                indices.Add(i);
            }
        }

        public KeyValuePair<ISalable, ItemStockInformation> this[int index]
        {
            get { return list[indices[index]]; }
            set { list[indices[index]] = value; }
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(indices[index]);
            for(int i = index; i < indices.Count; i++)
            {
                indices[i]--;
            }
        }
    }
}
