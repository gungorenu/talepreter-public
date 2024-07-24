namespace Talepreter.DB.Common
{
    /// <summary>
    /// This is a wrappter for self type, for easy handling plugin data
    /// not supposed to be used anywhere else, most probably by plugins only
    /// </summary>
    public class ContainerWrapper<T>
        where T : ContainerWrapper<T>, new()
    {
        private Container _container = default!;

        protected ContainerWrapper()
        {
            _container = new Container();
        }

        internal void SetInternalContainer(Container container)
        {
            _container = container;
        }

        public ContainerWrapper(Container container)
        {
            _container = container;
        }

        public string Id => _container.Id;

        public string[] Tags() => _container.Tags.Keys.ToArray();
        public string[] Children() => _container.Children.Keys.ToArray();
        public Dictionary<string,string> IterateTags() => _container.Tags;
        public Dictionary<string, Container> IterateChildren() => _container.Children;
        public IEnumerable<K> ChildrenAs<K>() where K: ContainerWrapper<K>, new()
        {
            return _container.Children.Values.Select(x =>
            {
                var r = new K();
                r.SetInternalContainer(x);
                return r;
            });
        }

        public bool? GetB(string name, bool? value = null) => _container.GetB(name, value);
        public bool ExpectB(string name) => _container.ExpectB(name);
        public ContainerWrapper<T> SetB(string name, bool value)
        {
            _container.SetB(name, value);
            return this;
        }

        public int? GetI(string name, int? value = null) => _container.GetI(name, value);
        public int ExpectI(string name) => _container.ExpectI(name);
        public ContainerWrapper<T> SetI(string name, int value)
        {
            _container.SetI(name, value);
            return this;
        }

        public string? GetS(string name, string? value = null) => _container.GetS(name, value);
        public string ExpectS(string name) => _container.ExpectS(name);
        public ContainerWrapper<T> SetS(string name, string value)
        {
            _container.SetS(name, value);
            return this;
        }

        public long? GetL(string name, long? value = null) => _container.GetL(name, value);
        public long ExpectL(string name) => _container.ExpectL(name);
        public ContainerWrapper<T> SetL(string name, long value)
        {
            _container.SetL(name, value);
            return this;
        }

        public E? GetE<E>(string name, E? value = null) where E : struct, Enum => _container.GetE(name, value);
        public E ExpectE<E>(string name) where E : struct, Enum => _container.ExpectE<E>(name);
        public ContainerWrapper<T> SetE<E>(string name, E value) where E : struct, Enum
        {
            _container.SetE(name, value);
            return this;
        }

        public K? GetC<K>(string name, ContainerWrapper<K>? value = null)
            where K : ContainerWrapper<K>, new()
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            if (_container.Children.TryGetValue(name, out var s))
            {
                var res = new K();
                res.SetInternalContainer(s);
                return res;
            }
            if (value != null)
            {
                _container.Children[name] = value._container;
                var res = new K();
                res.SetInternalContainer(value._container);
                return res;
            }
            return null!;
        }
        public K ExpectC<K>(string name)
            where K : ContainerWrapper<K>, new()
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            if (_container.Children.TryGetValue(name, out var s))
            {
                var res = new K();
                res.SetInternalContainer(s);
                return res;
            }
            throw new KeyNotFoundException($"{name} not found in tags");
        }
        public K SetC<K>(string name, ContainerWrapper<K> value)
            where K : ContainerWrapper<K>, new()
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            ArgumentNullException.ThrowIfNull(name, nameof(value));
            _container.Children[name] = value._container;
            var res = new K();
            res.SetInternalContainer(value._container);
            return res;
        }

        public void Remove(string name)
        {
            _container.RemoveT(name);
            _container.RemoveC(name);
        }
        public void RemoveT(string name)
        {
            _container.Tags.Remove(name);
        }
        public void RemoveC(string name)
        {
            _container.Children.Remove(name);
        }

        public void Clear()
        {
            _container.Tags.Clear();
            _container.Children.Clear();
        }
        public void ClearT()
        {
            _container.Tags.Clear();
        }
        public void ClearC()
        {
            _container.Children.Clear();
        }
    }
}
