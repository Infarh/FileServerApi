namespace FileServerApi.Infrastructure;

internal static class ReaderWriterLockSlimEx
{
    private abstract class Lock : IDisposable
    {
        protected readonly ReaderWriterLockSlim _Lock;

        protected Lock(ReaderWriterLockSlim Lock) => _Lock = Lock;

        public abstract void Dispose();
    }

    private class ReadLock : Lock
    {
        public ReadLock(ReaderWriterLockSlim Lock) : base(Lock) { }

        public override void Dispose()
        {
            if(_Lock.IsReadLockHeld)
                _Lock.ExitReadLock();
        }
    }

    private class WriteLock : Lock
    {
        public WriteLock(ReaderWriterLockSlim Lock) : base(Lock) { }

        public override void Dispose()
        {
            if(_Lock.IsReadLockHeld)
                _Lock.ExitReadLock();
        }
    }

    public static IDisposable LockRead(this ReaderWriterLockSlim Lock)
    {
        Lock.EnterReadLock();
        return new ReadLock(Lock);
    }

    public static IDisposable LockWrite(this ReaderWriterLockSlim Lock)
    {
        Lock.EnterReadLock();
        return new WriteLock(Lock);
    }

    public static LockObject<T> Obj<T>(this ReaderWriterLockSlim Lock, T Obj) => new(Lock, Obj);

    public readonly ref struct LockObject<T>
    {
        private readonly ReaderWriterLockSlim _Lock;
        private readonly T _Obj;

        public LockObject(ReaderWriterLockSlim Lock, T Obj)
        {
            _Lock = Lock;
            _Obj = Obj;
        }

        public TValue Read<TValue>(Func<T, TValue> Reader)
        {
            using (_Lock.LockRead())
                return Reader(_Obj);
        }

        public TValue Read<TValue, TP>(TP Parameter, Func<T, TP, TValue> Reader)
        {
            using (_Lock.LockRead())
                return Reader(_Obj, Parameter);
        }

        public void Write(Action<T> Writer)
        {
            using (_Lock.LockWrite())
                Writer(_Obj);
        }

        public TResult Write<TResult>(Func<T, TResult> Writer)
        {
            using (_Lock.LockWrite())
                return Writer(_Obj);
        }

        public void Write<TValue>(TValue Value, Action<T, TValue> Writer)
        {
            using (_Lock.LockWrite())
                Writer(_Obj, Value);
        }

        public TResult Write<TValue, TResult>(TValue Value, Func<T, TValue, TResult> Writer)
        {
            using (_Lock.LockWrite())
                return Writer(_Obj, Value);
        }
    }
}
