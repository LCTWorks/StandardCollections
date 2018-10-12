using System;
using System.Collections.Generic;
using System.Text;

namespace StandardCollections.Events
{
    public delegate void ItemReplacedEventHandler<T>(object sender, ItemReplacedEventArgs<T> e);
    public class ItemReplacedEventArgs<T>
    {
        public T ItemAdded { get; private set; }
        public T ItemRemoved { get; private set; }

        public ItemReplacedEventArgs(T itemAdded, T itemRemoved)
        {
            this.ItemAdded = itemAdded;
            this.ItemRemoved = itemRemoved;
        }
    }
}
