using System.Threading;
using System.Threading.Tasks;

namespace UTools
{
    public interface IInitializable
    {
        void Initialize();
    }

    public interface IAsyncInitializable
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }

    public interface ITickable
    {
        void Tick();
    }

    public interface ILateTickable
    {
        void LateTick();
    }

    public interface IFixedTickable
    {
        void FixedTick();
    }

    /// <summary>
    /// 自定义的可销毁接口，用于在 UDI 容器中管理对象的生命周期。
    /// 注意：为避免与 System.IDisposable 冲突，使用 IUDisposable 命名。
    /// </summary>
    public interface IUDisposable
    {
        void Dispose();
    }

    public interface IPausable
    {
        void Pause();
        void Resume();
    }
}
