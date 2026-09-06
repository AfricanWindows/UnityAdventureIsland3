namespace Game.Core
{
    /// <summary>
    /// Makes one T on demand, and that is the whole contract.
    ///
    /// The pool depends on THIS and on nothing else, so it never learns what a laser is,
    /// who assembles it, or that Instantiate exists at all (Dependency Inversion).
    /// Swapping the way lasers are created - a different prefab, a different builder,
    /// an addressable load - never touches a single line inside the pool.
    /// </summary>
    /// <typeparam name="T">What comes out. Covariant, so an IFactory of a concrete
    /// projectile can be used where an IFactory of its base class is expected.</typeparam>
    public interface IFactory<out T>
    {
        T Create();
    }
}
