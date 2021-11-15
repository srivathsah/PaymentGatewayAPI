namespace Domain;

public interface IOption<T>
{
    bool IsSome { get; }
    bool IsSomeOf(T value);
    bool IsNone { get; }
    TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone);
    IOption<TResult>? Bind<TResult>(Func<T, IOption<TResult>> f);
    IOption<TResult>? Map<TResult>(Func<T, TResult> f);
    T Or(T aDefault);
    void Iter(Action<T> onSome);
    IList<T>? ToList();
}

public static class Some
{
    public static IOption<T>? Of<T>(T value) => Some<T>.Of(value);
}

public static class OptionPrelude
{
    public static IOption<T>? FromNullable<T>(T? v) where T : struct =>
      v.HasValue ? Some<T>.Of(v.Value) : new None<T>();

    public static IOption<T>? FromMaybeNull<T>(T v) where T : class =>
      v != null ? Some<T>.Of(v) : new None<T>();

    public static IEnumerable<T>? ToEnumerable<T>(IOption<T> value) =>
      value.Match(v => new[] { v }, Enumerable.Empty<T>);

    public static IEnumerable<T>? Concat<T>(IEnumerable<IOption<T>> values) =>
      values.SelectMany(ToEnumerable);

    public static IOption<TElem>? TryFind<TElem>(IEnumerable<TElem> values, Func<TElem, bool> predicate) =>
      values.Any(predicate) ? Some<TElem>.Of(values.First(predicate)) : new None<TElem>();

    public static IOption<TElem>? TryFirst<TElem>(IEnumerable<TElem> values) =>
      values.Any() ? Some<TElem>.Of(values.First()) : new None<TElem>();

    public static IOption<TElem>? TryKey<TKey, TElem>(TKey key, IDictionary<TKey, TElem> dict) =>
      dict.ContainsKey(key) ? Some<TElem>.Of(dict[key]) : new None<TElem>();

    public static IOption<TResult>? FromOneOrManyOrNone<TElem, TResult>(IEnumerable<TElem> values, Func<TElem, TResult> whenOne, Func<IEnumerable<TElem>, TResult> whenMany) =>
        (values.Count()) switch
        {
            1 => Some<TResult>.Of(whenOne(values.First())),
            0 => new None<TResult>(),
            _ => Some<TResult>.Of(whenMany(values)),
        };
}

public record Some<T> : IOption<T>
{
    public T Value { get; }

    private Some(T value)
    {
        Value = value;
    }

    public static IOption<T>? Of(T value) => new Some<T>(value);

    public bool IsSome => true;
    public bool IsSomeOf(T value) => Value?.Equals(value) == true;
    public bool IsNone => false;

    public TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> _) => onSome(Value);

    public IOption<TResult>? Bind<TResult>(Func<T, IOption<TResult>>? f) => f?.Invoke(Value);
    public IOption<TResult>? Map<TResult>(Func<T, TResult> f) => new Some<TResult>(f(Value));
    public T Or(T _) => Value;
    public void Iter(Action<T>? onSome) => onSome?.Invoke(Value);
    public IList<T>? ToList() => new List<T> { Value };
}

public record None<T> : IOption<T>
{
    public bool IsSome => false;
    public bool IsSomeOf(T value) => false;
    public bool IsNone => true;
    public TResult Match<TResult>(Func<T, TResult> _, Func<TResult> onNone) => onNone();
    public IOption<TResult>? Bind<TResult>(Func<T, IOption<TResult>> _) => new None<TResult>();
    public IOption<TResult>? Map<TResult>(Func<T, TResult> _) => new None<TResult>();
    public T Or(T aDefault) => aDefault;
    public void Iter(Action<T> _) { }
    public IList<T>? ToList() => new List<T>();
}
