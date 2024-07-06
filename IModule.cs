using System.Threading.Tasks;

namespace Arr.ModulesSystem
{
    public interface IModule
    {
        public Task Initialize();

        public Task Load();

        public Task Unload();
    }
}