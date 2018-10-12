namespace StandardCollections.Events
{
    public delegate void ItemReplacingEventHandler<T>(object sender, ItemReplacingEventArgs<T> e);
    public class ItemReplacingEventArgs<T>
    {
        public bool Handled { get; set; }
        public T NewItem { get; private set; }
        public T StoredItem { get; private set; }

        public ItemReplacingEventArgs(T newItem, T storedItem)
        {
            this.NewItem = newItem;
            this.StoredItem = storedItem;
        }
    }
}
