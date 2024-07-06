using System.Threading.Tasks;

namespace Arr.ModulesSystem
{
    public abstract class BaseModule : IModule
    {
        protected virtual Task OnInitialized() => Task.CompletedTask;

        protected virtual Task OnLoad() => Task.CompletedTask;

        protected virtual Task OnUnload() => Task.CompletedTask;

        public Task Initialize() => OnInitialized();

        public Task Load() => OnLoad();

        public Task Unload() => OnUnload();
    }
}