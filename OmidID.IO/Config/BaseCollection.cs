using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections;

namespace OmidID.IO.SaveMedia.Config {
    public class BaseCollection<T> : ConfigurationElementCollection, IEnumerable<T> where T : ConfigurationElement, IKeyElement, new() {

        public override ConfigurationElementCollectionType CollectionType {
            get {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override ConfigurationElement CreateNewElement() {
            return new T();
        }

        protected override Object GetElementKey(ConfigurationElement element) {
            return ((T)element).Name;
        }

        public T this[int index] {
            get {
                return (T)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        new public T this[string Name] {
            get {
                return (T)BaseGet(Name);
            }
        }

        public int IndexOf(T item) {
            return BaseIndexOf(item);
        }

        public void Add(T item) {
            BaseAdd(item);
        }
        protected override void BaseAdd(ConfigurationElement element) {
            BaseAdd(element, false);
        }

        public void Remove(T item) {
            if (BaseIndexOf(item) >= 0)
                BaseRemove(item.Name);
        }

        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }

        public void Remove(string name) {
            BaseRemove(name);
        }

        public void Clear() {
            BaseClear();
        }

        public new IEnumerator<T> GetEnumerator() {
            var ien = base.GetEnumerator();
            while (ien.MoveNext())
                yield return (T)ien.Current;
        }
    }
}
