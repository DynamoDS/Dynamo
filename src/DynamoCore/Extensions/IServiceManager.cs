using System;

namespace Dynamo.Extensions
{
    /// <summary>
    /// Defines a mechanism for registering and retrieving a service object; 
    /// that is, an object that provides custom support to other objects.
    /// </summary>
    public interface IServiceManager : IServiceProvider
    {
        /// <summary>
        /// Allows extension applications to register some specific service by its type.
        /// Only one service of a given type can be registered.
        /// </summary>
        /// <typeparam name="T">Type of the service</typeparam>
        /// <param name="service">The service object to register</param>
        /// <returns>A key for the registered service if registeration is 
        /// successful else null.</returns>
        string RegisterService<T>(T service);

        /// <summary>
        /// Unregisters a service of given type registered with given key. 
        /// </summary>
        /// <typeparam name="T">Type of the service</typeparam>
        /// <param name="serviceKey">The service key to ensure that only authorized
        /// client is unregistering this service type.</param>
        /// <returns></returns>
        bool UnregisterService<T>(string serviceKey);

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of the service</typeparam>
        /// <returns>The service object if registered else null</returns>
        T Service<T>();
    }
}
