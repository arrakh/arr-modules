using System;

namespace Arr.ModulesSystem
{
    public interface IExternalModulesProvider
    {
        public bool TryGetModule(Type type, out IModule instance);
    }
}