/// <summary>
/// A base class for the singleton design pattern.
/// </summary>
/// <typeparam name="T">Class type of the singleton</typeparam>
public abstract class Singleton<T> where T : class
{
    #region Members

    /// <summary>
    /// Static instance. Needs to use lambda expression
    /// to construct an instance (since constructor is private).
    /// </summary>
    private static readonly System.Lazy<T> sInstance = new System.Lazy<T>(() => CreateInstanceOfT());

    #endregion Members

    #region Properties

    /// <summary>
    /// Gets the instance of this singleton.
    /// </summary>
    public static T Instance { get { return sInstance.Value; } }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Creates an instance of T via reflection since T's constructor is expected to be private.
    /// </summary>
    /// <returns></returns>
    private static T CreateInstanceOfT()
    {
        return System.Activator.CreateInstance(typeof(T), true) as T;
    }

    #endregion Methods
}

/*
public abstract class Singleton<T> where T : class, new()
{
    private static T _instance;

    public static T Instance
    {
        get {
            if (_instance == null)
                _instance = new T();
            return _instance;
        }
    }
}
*/