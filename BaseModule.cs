using System.Threading.Tasks;

namespace Arr.ModulesSystem
{
    public abstract class BaseModule : IModule
    {
        protected virtual Task OnInitialize() => Task.CompletedTask;

        protected virtual Task OnLoad() => Task.CompletedTask;

        protected virtual Task OnUnload() => Task.CompletedTask;

        public Task Initialize() => OnInitialize();

        public Task Load() => OnLoad();

        public Task Unload() => OnUnload();
    }
}