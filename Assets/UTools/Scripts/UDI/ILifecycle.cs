namespace UTools
{
    public interface IInitializable
    {
        void Initialize();
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

    public interface IDisposable
    {
        void Dispose();
    }

    public interface IPausable
    {
        void Pause();
        void Resume();
    }
}