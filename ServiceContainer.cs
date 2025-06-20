// filepath: c:\Users\steve.mckitrick\Desktop\BusBuddy\BusBuddy.DependencyInjection\ServiceContainer.cs
using Microsoft.Extensions.DependencyInjection;
using BusBuddy.Data;
using BusBuddy.UI.Services;
using BusBuddy.Business;
using System;
using System.Linq;
using System.Windows.Forms;

namespace BusBuddy.DependencyInjection
{
    public class ServiceContainer : BusBuddy.UI.Services.IServiceProvider, IFormFactory
    {
        private readonly ServiceCollection _services = new ServiceCollection();
        private System.IServiceProvider _serviceProvider;

        public void ConfigureServices()
        {
            _services.AddScoped<BusBuddyContext>();
            _services.AddScoped<INavigationService, NavigationService>();
            _services.AddScoped<BusBuddy.UI.Services.IDatabaseHelperService, BusBuddy.UI.Services.DatabaseHelperService>();
            _services.AddSingleton<IFormFactory>(this);
            _serviceProvider = _services.BuildServiceProvider();
        }

        public T GetService<T>()
        {
            return _serviceProvider.GetService<T>() ?? throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
        }

        public object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

        public void Dispose()
        {
            if (_serviceProvider is IDisposable disposableProvider)
            {
                disposableProvider.Dispose();
            }
        }

        public T CreateForm<T>() where T : Form
        {
            var form = (T)(Activator.CreateInstance(typeof(T)) ?? throw new InvalidOperationException($"Could not create instance of {typeof(T).Name}"));
            InjectDependencies(form);
            return form;
        }

        public T CreateForm<T>(params object[] parameters) where T : Form
        {
            var form = (T)(Activator.CreateInstance(typeof(T), parameters) ?? throw new InvalidOperationException($"Could not create instance of {typeof(T).Name}"));
            InjectDependencies(form);
            return form;
        }

        public Form CreateForm(Type formType)
        {
            if (!typeof(Form).IsAssignableFrom(formType))
                throw new ArgumentException($"Type {formType.Name} must inherit from Form");

            var form = (Form)(Activator.CreateInstance(formType) ?? throw new InvalidOperationException($"Could not create instance of {formType.Name}"));
            InjectDependencies(form);
            return form;
        }

        private void InjectDependencies(Form form)
        {
            var properties = form.GetType().GetProperties()
                .Where(p => p.CanWrite && p.PropertyType.IsInterface);

            foreach (var property in properties)
            {
                var service = _serviceProvider.GetService(property.PropertyType);
                if (service != null)
                {
                    property.SetValue(form, service);
                }
            }
        }
    }
}
