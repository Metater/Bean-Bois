using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NetRefLookup<T> where T : NetworkBehaviour
{
    private readonly Dictionary<uint, T> lookup = new();

    public IEnumerable<T> Refs => lookup.Values;

    public void Add(T reference)
    {
        lookup[reference.netId] = reference;
    }

    public void Remove(T reference)
    {
        lookup.Remove(reference.netId);
    }

    public bool TryGetWithNetId(uint netId, out T reference)
    {
        return lookup.TryGetValue(netId, out reference);
    }

    /*
    public bool TryAdd(T reference)
    {
        return lookup.TryAdd(reference.netId, reference);
    }

    public bool TryGet(Predicate<T> predicate, out T reference)
    {
        foreach (var r in Refs)
        {
            if (predicate(r))
            {
                reference = r;
                return true;
            }
        }
        reference = default;
        return false;
    }

    public bool TryRemove(T reference)
    {
        return lookup.Remove(reference.netId);
    }

    public bool TryRemoveWithNetId(uint netId, out T reference)
    {
        return lookup.Remove(netId, out reference);
    }
    */
}