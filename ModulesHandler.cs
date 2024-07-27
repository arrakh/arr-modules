using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EventHandler = Arr.EventsSystem.EventHandler;

namespace Arr.ModulesSystem
{
    public class ModulesHandler
    {
        private Dictionary<Type, IModule> modules;
        private List<IModule> initializedModules;
        private List<IModule> loadedModules;
        private EventHandler eventHandler;

        private IExternalModulesProvider externalModulesProvider = null;

        public ModulesHandler(IEnumerable<IModule> modules, EventHandler eventHandler,
            IExternalModulesProvider externalModulesProvider) : this(modules, eventHandler)
        {
            this.externalModulesProvider = externalModulesProvider;
        }

        public ModulesHandler(IEnumerable<IModule> modules, EventHandler eventHandler)
        {
            this.modules = new();
            this.eventHandler = eventHandler;
            initializedModules = new();
            loadedModules = new();

            foreach (var module in modules) RegisterModule(module);
        }

        private void RegisterModule(IModule module)
        {
            if (module is ModuleGroup group)
            {
                foreach (var m in group.Modules) RegisterModule(m);
                return;
            }
            
            var type = module.GetType();
            
            if (modules.ContainsKey(type))
            {
#if ARR_SHOW_EXCEPTIONS
                throw new Exception($"Trying to add duplicate instance of type {type.Name}");
#endif
                return;
            }
            
            modules[type] = module;
        }

        public bool TryGetModule(Type type, out IModule module) => modules.TryGetValue(type, out module);

        public async Task Start()
        {
            //Order module in ascending manner, where if it is not IOrderedModule it will have an order of 0
            var orderedModules = modules.Values
                .OrderBy(x => x is IOrderedModule ordered ? ordered.Order : 0).ToList();
            
            foreach (var module in orderedModules)
            {
                eventHandler.RegisterMultiple(module);
                await module.Initialize();
                initializedModules.Add(module);
            }

            foreach (var pair in modules)
                InjectDependencies(pair.Key, pair.Value);

            foreach (var module in orderedModules)
            {
                await module.Load();
                loadedModules.Add(module);
            }
        }

        private void InjectDependencies(Type moduleType, IModule instance)
        {
            var fields = moduleType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                var attrib = field.GetCustomAttribute(typeof(InjectModuleAttribute));
                if (attrib is not InjectModuleAttribute) continue;
                var type = field.FieldType;

                if (!typeof(IModule).IsAssignableFrom(type))
                {
#if ARR_SHOW_EXCEPTIONS
                    throw new Exception($"Trying to inject type {type.Name} but it is not a Module!");
#endif
                    continue;
                }
                
                if (!modules.TryGetValue(type, out var module))
                {
                    if (externalModulesProvider == null || !externalModulesProvider.TryGetModule(type, out module))
                    {
#if ARR_SHOW_EXCEPTIONS
                        throw new Exception($"Trying to inject type {type.Name} but could not find the Module!");
#endif
                        continue;
                    }
                }
                
                TypedReference tr = __makeref(instance);
                field.SetValueDirect(tr, module);
            }
        }

        public async Task Stop()
        {
            //Order module in descending manner, where if it is not IOrderedModule it will have an order of 0
            var orderedModules = initializedModules
                .OrderByDescending(x => x is IOrderedModule ordered ? ordered.Order : 0).ToList();
            
            foreach (var module in orderedModules) eventHandler.UnregisterMultiple(module);
            
            orderedModules = loadedModules
                .OrderByDescending(x => x is IOrderedModule ordered ? ordered.Order : 0).ToList();

            foreach (var module in orderedModules) await module.Unload();
        }
    }
}