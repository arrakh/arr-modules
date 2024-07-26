using System.Threading.Tasks;

namespace Arr.ModulesSystem
{
    public abstract class ModuleGroup : IModule
    {
        public virtual IModule[] Modules => null;
        
        public async Task Initialize()
        {
            foreach (var module in Modules)
                await module.Initialize();
        }

        public async Task Load()
        {
            foreach (var module in Modules)
                await module.Load();
        }

        public async Task Unload()
        {
            foreach (var module in Modules)
                await module.Unload();
        }
    }
}