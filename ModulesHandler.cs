using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using EventHandler = Arr.EventsSystem.EventHandler;

namespace Arr.ModulesSystem
{
    public class ModulesHandler
    {
        private Dictionary<Type, IModule> modules;
        private EventHandler eventHandler;

        public ModulesHandler(IEnumerable<IModule> modules, EventHandler eventHandler)
        {
            this.modules = new();
            foreach (var module in modules)
            {
                var type = module.GetType();
                if (this.modules.ContainsKey(type))
                {
#if ARR_SHOW_EXCEPTIONS
                    throw new Exception($"Trying to add duplicate instance of type {type.Name}");
#endif
                    continue;
                }
                this.modules[type] = module;
            }
            
            this.eventHandler = eventHandler;
        }

        public async Task Start()
        {
            foreach (var module in modules.Values)
            {
                eventHandler.RegisterMultiple(module);
                await module.Initialize();
            }

            foreach (var pair in modules)
                InjectDependencies(pair.Key, pair.Value);

            foreach (var module in modules.Values)
                await module.Load();
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
#if ARR_SHOW_EXCEPTIONS
                    throw new Exception($"Trying to inject type {type.Name} but could not find the Module!");
#endif
                    continue;
                }
                
                TypedReference tr = __makeref(instance);
                field.SetValueDirect(tr, module);
            }
        }

        public async Task Stop()
        {
            foreach (var module in modules.Values)
            {
                eventHandler.UnregisterMultiple(module);
                await module.Unload();
            }
        }
    }
}