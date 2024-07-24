using System.Text.Json;
using Talepreter.Common;

namespace Talepreter.DB.Common
{
    /// <summary>
    /// Very generic container structure to hold data for plugins
    /// Only to be used for storage (DB), nowhere else
    /// Can serialize self just fine without any issue
    /// </summary>
    public class Container
    {
        public static JsonSerializerOptions SerializationOptions { get; private set; }

        static Container()
        {
            JsonSerializerOptions options = new()
            {
                WriteIndented = false,
                IgnoreReadOnlyFields = true,
                IgnoreReadOnlyProperties = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            SerializationOptions = options;
        }

        public Container()
        {
        }

        public Container(string id)
        {
            Id = id;
        }

        public string Id { get; set; } = default!;

        public Dictionary<string, string> Tags { get; set; } = [];
        public Dictionary<string, Container> Children { get; set; } = [];

        public bool? GetB(string name, bool? value = null)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            if (Tags.TryGetValue(name, out var s))
                return s.ToBool();
            if (value != null)
                Tags[name] = value.Value.ToString();
            return value;
        }
        public bool ExpectB(string name)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            if (Tags.TryGetValue(name, out var s))
                return s.ToBool();
            throw new KeyNotFoundException($"{name} not found in tags");
        }
        public Container SetB(string name, bool value)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            Tags[name] = value.ToString();
            return this;
        }

        public int? GetI(string name, int? value = null)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            if (Tags.TryGetValue(name, out var s))
                return s.ToInt();
            if (value != null)
                Tags[name] = value.Value.ToString();
            return value;
        }
        public int ExpectI(string name)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            if (Tags.TryGetValue(name, out var s))
                return s.ToInt();
            throw new KeyNotFoundException($"{name} not found in tags");
        }
        public Container SetI(string name, int value)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            Tags[name] = value.ToString();
            return this;
        }

        public string? GetS(string name, string? value = null)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            if (Tags.TryGetValue(name, out var s))
                return s;
            if (value != null)
                Tags[name] = value!;
            return value;
        }
        public string ExpectS(string name)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            if (Tags.TryGetValue(name, out var s))
                return s;
            throw new KeyNotFoundException($"{name} not found in tags");
        }
        public Container SetS(string name, string value)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            Tags[name] = value;
            return this;
        }

        public long? GetL(string name, long? value = null)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            if (Tags.TryGetValue(name, out var s))
                return s.ToLong();
            if (value != null)
                Tags[name] = value.Value.ToString();
            return value;
        }
        public long ExpectL(string name)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            if (Tags.TryGetValue(name, out var s))
                return s.ToLong();
            throw new KeyNotFoundException($"{name} not found in tags");
        }
        public Container SetL(string name, long value)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            Tags[name] = value.ToString();
            return this;
        }

        public E? GetE<E>(string name, E? value = null)
            where E : struct, Enum
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            if (Tags.TryGetValue(name, out var s))
                return Enum.Parse<E>(s!);
            if (value != null)
                Tags[name] = value.Value.ToString();
            return value;
        }
        public E ExpectE<E>(string name)
            where E : struct, Enum
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            if (Tags.TryGetValue(name, out var s))
                return Enum.Parse<E>(s!);
            throw new KeyNotFoundException($"{name} not found in tags");
        }
        public Container SetE<E>(string name, E value)
            where E : struct, Enum
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            Tags[name] = value.ToString();
            return this;
        }

        public Container? GetC(string name, Container? value = null)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            if (Children.TryGetValue(name, out var s))
                return s;
            if (value != null)
            {
                Children[name] = value;
                return value;
            }
            return this;
        }
        public Container ExpectC(string name)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            if (Children.TryGetValue(name, out var s))
                return s;
            throw new KeyNotFoundException($"{name} not found in tags");
        }
        public Container SetC(string name, Container value)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            ArgumentNullException.ThrowIfNull(name, nameof(value));
            Children[name] = value;
            return value;
        }

        public ContainerWrapper<K>? GetC<K>(string name, Container? value = null)
            where K : ContainerWrapper<K>, new()
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            if (Children.TryGetValue(name, out var s))
            {
                var res = new K();
                res.SetInternalContainer(s);
                return res;
            }
            if (value != null)
            {
                Children[name] = value;
                var res = new K();
                res.SetInternalContainer(value);
                return res;
            }
            return null;
        }
        public ContainerWrapper<K> ExpectC<K>(string name)
            where K : ContainerWrapper<K>, new()
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            if (Children.TryGetValue(name, out var s))
            {
                var res = new K();
                res.SetInternalContainer(s);
                return res;
            }
            throw new KeyNotFoundException($"{name} not found in tags");
        }
        public ContainerWrapper<K> SetC<K>(string name, Container value)
            where K : ContainerWrapper<K>, new()
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            ArgumentNullException.ThrowIfNull(name, nameof(value));
            Children[name] = value;
            value.Id = name;
            var res = new K();
            res.SetInternalContainer(value);
            return res;
        }

        public void Remove(string name)
        {
            Tags.Remove(name);
            Children.Remove(name);
        }
        public void RemoveT(string name)
        {
            Tags.Remove(name);
        }
        public void RemoveC(string name)
        {
            Children.Remove(name);
        }

        public void Clear()
        {
            Tags.Clear();
            Children.Clear();
        }
        public void ClearT()
        {
            Tags.Clear();
        }
        public void ClearC()
        {
            Children.Clear();
        }
    }
}
