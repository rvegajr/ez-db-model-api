using Microsoft.Extensions.DependencyInjection;

namespace Api.Infrastructure.DI;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ServiceRegistrationAttribute : Attribute
{
    public ServiceLifetime Lifetime { get; }
    public Type? ServiceType { get; }

    public ServiceRegistrationAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient, Type? serviceType = null)
    {
        Lifetime = lifetime;
        ServiceType = serviceType;
    }
}
