using System;
using System.Reflection;

namespace BusBuddy.UI.Tests
{
    /// <summary>
    /// A helper class for accessing private members of an object during testing.
    /// Used to test internal implementation details without exposing them publicly.
    /// </summary>
    internal class PrivateObject
    {
        private readonly object _instance;
        private readonly Type _type;

        /// <summary>
        /// Initializes a new instance of the PrivateObject class with the specified target object.
        /// </summary>
        /// <param name="instance">The target object to access private members on.</param>
        /// <exception cref="ArgumentNullException">Thrown when instance is null.</exception>
        public PrivateObject(object instance)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _type = instance.GetType();
        }

        /// <summary>
        /// Gets the value of a private field from the target object.
        /// </summary>
        /// <param name="fieldName">The name of the private field to access.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="ArgumentException">Thrown when the field is not found.</exception>
        public object GetField(string fieldName)
        {
            var field = _type.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (field == null)
                throw new ArgumentException($"Field '{fieldName}' not found in type {_type.Name}");

            return field.GetValue(_instance)!;
        }

        /// <summary>
        /// Sets the value of a private field on the target object.
        /// </summary>
        /// <param name="fieldName">The name of the private field to set.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentException">Thrown when the field is not found.</exception>
        public void SetField(string fieldName, object value)
        {
            var field = _type.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (field == null)
                throw new ArgumentException($"Field '{fieldName}' not found in type {_type.Name}");

            field.SetValue(_instance, value);
        }

        /// <summary>
        /// Invokes a private method on the target object.
        /// </summary>
        /// <param name="methodName">The name of the private method to invoke.</param>
        /// <param name="parameters">Optional parameters to pass to the method.</param>
        /// <returns>The return value of the method, or null if void.</returns>
        /// <exception cref="ArgumentException">Thrown when the method is not found.</exception>
        public object Invoke(string methodName, params object[] parameters)
        {
            var method = _type.GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (method == null)
                throw new ArgumentException($"Method '{methodName}' not found in type {_type.Name}");

            return method.Invoke(_instance, parameters)!;
        }

        /// <summary>
        /// Gets the value of a field or property from the target object.
        /// </summary>
        /// <param name="name">The name of the field or property to access.</param>
        /// <returns>The value of the field or property.</returns>
        /// <exception cref="ArgumentException">Thrown when neither a field nor property with the given name is found.</exception>
        public object GetFieldOrProperty(string name)
        {
            // Try to get as a field first
            var field = _type.GetField(name,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (field != null)
                return field.GetValue(_instance)!;

            // Then try as a property
            var property = _type.GetProperty(name,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (property != null)
                return property.GetValue(_instance)!;

            throw new ArgumentException($"Neither field nor property '{name}' found in type {_type.Name}");
        }
    }
}

