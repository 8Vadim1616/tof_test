using Assets.Scripts.Libraries.RSG;

namespace Assets.Scripts
{
    public interface IDataLoader
    {
        IPromise Load(PreloaderScreen preloaderScreen);
    }
}