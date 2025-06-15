using System;
using System.Windows.Forms;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Factory interface for creating forms with proper dependency injection
    /// </summary>
    public interface IFormFactory
    {
        T CreateForm<T>() where T : Form;
        T CreateForm<T>(params object[] parameters) where T : Form;
    }

    /// <summary>
    /// Service locator for accessing forms and services
    /// </summary>
    public interface IServiceProvider
    {
        T GetService<T>();
        object GetService(Type serviceType);
    }
}
