namespace Skaar.Decorator;

/// <summary>
/// A interface indicating that the class is a decorator wrapper..
/// </summary>
/// <typeparam name="T">The type of the inner value this type wraps.</typeparam>
public interface IDecorator<T>
{
    ///<summary>The inner value this type wraps.</summary>
    T Inner { get; }
}
