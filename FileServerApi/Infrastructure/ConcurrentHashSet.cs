using System.Collections;

namespace FileServerApi.Infrastructure;

public class ConcurrentHashSet<T> : IDisposable, IEnumerable<T>
{
    private readonly ReaderWriterLockSlim _Lock = new(LockRecursionPolicy.SupportsRecursion);

    private readonly HashSet<T> _HashSet;

    public ConcurrentHashSet(HashSet<T> Set) => _HashSet = Set;

    public ConcurrentHashSet() : this(new HashSet<T>()) { }

    public ConcurrentHashSet(IEnumerable<T> Items) : this(new HashSet<T>(Items)) { }
    public ConcurrentHashSet(IEnumerable<T> Items, IEqualityComparer<T> Comparer) : this(new HashSet<T>(Items, Comparer)) { }
    public ConcurrentHashSet(IEqualityComparer<T> Comparer) : this(new HashSet<T>(Comparer)) { }
    public ConcurrentHashSet(int Capacity) : this(new HashSet<T>(Capacity)) { }
    public ConcurrentHashSet(int Capacity, IEqualityComparer<T> Comparer) : this(new HashSet<T>(Capacity, Comparer)) { }

    public int Count => _Lock.Obj(_HashSet).Read(set => set.Count);

    public bool Add(T item) => _Lock.Obj(_HashSet).Write(item, (set, v) => set.Add(v));

    public bool Contains(T item) => _Lock.Obj(_HashSet).Read(item, (set, p) => set.Contains(p));

    public bool Remove(T item) => _Lock.Obj(_HashSet).Write(item, (set, p) => set.Remove(p));

    public void Clear() => _Lock.Obj(_HashSet).Write(set => set.Clear());

    public void Dispose()
    {
        Dispose(true);
        //GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            _Lock.Dispose();
    }

    //~ConcurrentHashSet()
    //{
    //    Dispose(false);
    //}

    public IEnumerator<T> GetEnumerator()
    {
        using(_Lock.LockRead())
            foreach (var item in _HashSet)
                yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_HashSet).GetEnumerator();
}
