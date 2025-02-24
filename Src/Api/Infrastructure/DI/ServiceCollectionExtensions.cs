using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Infrastructure.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AutoRegisterServices(
        this IServiceCollection services,
        Action<AutoRegistrationOptions> configure)
    {
        var options = new AutoRegistrationOptions();
        configure(options);

        // Get all assemblies matching the patterns
        var assemblies = options.AssemblyPatterns
            .SelectMany(pattern => AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && MatchesWildcard(a.GetName().Name ?? "", pattern)))
            .Distinct()
            .ToList();

        foreach (var assembly in assemblies)
        {
            // Get all non-abstract classes from the assembly that are not generic type definitions
            var types = assembly.GetExportedTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition)
                .ToList();

            foreach (var type in types)
            {
                // Check for explicit registration attribute
                var attribute = type.GetCustomAttribute<ServiceRegistrationAttribute>();
                if (attribute != null)
                {
                    RegisterType(services, type, attribute.ServiceType, attribute.Lifetime);
                    continue;
                }

                // Check for interface matching pattern (IClassName for ClassName)
                if (options.EnableInterfaceMatching)
                {
                    var interfaceType = type.GetInterfaces()
                        .FirstOrDefault(i => i.Name.Equals($"I{type.Name}"));

                    if (interfaceType != null)
                    {
                        RegisterType(services, type, interfaceType, options.DefaultLifetime);
                    }
                }
            }
        }

        return services;
    }

    private static void RegisterType(
        IServiceCollection services,
        Type implementationType,
        Type? serviceType,
        ServiceLifetime lifetime)
    {
        serviceType ??= implementationType;

        var descriptor = new ServiceDescriptor(
            serviceType,
            implementationType,
            lifetime);

        // Remove any existing registration for this service type
        var existing = services.FirstOrDefault(d => d.ServiceType == serviceType);
        if (existing != null)
        {
            services.Remove(existing);
        }

        services.Add(descriptor);
    }

    private static bool MatchesWildcard(string input, string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return false;

        var parts = pattern.Split('*', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length == 0)
            return true; // Pattern is all asterisks

        if (!pattern.StartsWith("*") && !input.StartsWith(parts[0]))
            return false;

        if (!pattern.EndsWith("*") && !input.EndsWith(parts[^1]))
            return false;

        var currentIndex = 0;
        foreach (var part in parts)
        {
            var index = input.IndexOf(part, currentIndex, StringComparison.OrdinalIgnoreCase);
            if (index == -1)
                return false;
            currentIndex = index + part.Length;
        }

        return true;
    }
}

public class AutoRegistrationOptions
{
    public List<string> AssemblyPatterns { get; } = new();
    public bool EnableInterfaceMatching { get; set; } = true;
    public ServiceLifetime DefaultLifetime { get; set; } = ServiceLifetime.Transient;

    public AutoRegistrationOptions AddAssemblyPattern(string pattern)
    {
        AssemblyPatterns.Add(pattern);
        return this;
    }
}
