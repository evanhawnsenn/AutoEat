using VRage.Plugins;

namespace AutoEatPulsar
{
    public sealed class Plugin : IPlugin
    {
        public void Init(object gameInstance) { }
        public void Update() { }
        public void Dispose() { AutoEatRuntime.Stop(); }
    }
}
