using JasperFx.Core;
using JasperFx.Core.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Wolverine.Codegen;

internal class ServiceContainer : IServiceProviderIsService
{
    private readonly IServiceProviderIsService _serviceChecker;
    private ImHashMap<Type, ServicePlan> _defaults = ImHashMap<Type, ServicePlan>.Empty;
    
    private ImHashMap<ServiceDescriptor, ServicePlan> _plans = ImHashMap<ServiceDescriptor, ServicePlan>.Empty;
    
    private ImHashMap<Type, ServiceFamily> _families = ImHashMap<Type, ServiceFamily>.Empty;
    
    
    private readonly IServiceProvider _provider;

    public ServiceContainer(IServiceCollection services, IServiceProvider provider)
    {
        var families = services.GroupBy(x => x.ServiceType)
            .Select(x => new ServiceFamily(x.Key, x));

        foreach (var family in families)
        {
            _families = _families.AddOrUpdate(family.ServiceType, family);
        }
        
        _serviceChecker = provider as IServiceProviderIsService ?? provider.GetService<IServiceProviderIsService>() ?? this;
        _provider = provider;
    }

    bool IServiceProviderIsService.IsService(Type serviceType)
    {
        return false;
    }

    private ServicePlan planFor(ServiceDescriptor descriptor, List<ServiceDescriptor> trail)
    {
        if (_plans.TryFind(descriptor, out var plan)) return plan;

        var family = findFamily(descriptor.ServiceType);
        plan = family.BuildPlan(this, descriptor, trail);

        _plans = _plans.AddOrUpdate(descriptor, plan);
        return plan;
    }

    public bool CouldResolve(Type type) => CouldResolve(type, new());

    public bool CouldResolve(Type type, List<ServiceDescriptor> trail)
    {
        if (_defaults.TryFind(type, out var plan))
        {
            return !(plan is InvalidPlan);
        }
        
        if (_defaults.Contains(type)) return true;

        if (isEnumerable(type))
        {
            return true;
        }
        
        if (_serviceChecker.IsService(type)) return true;

        if (type.IsConcreteWithDefaultCtor())
        {
            return true;
        }
        
        var descriptor = findDefaultDescriptor(type);
        plan = planFor(descriptor, trail);
        return plan is not InvalidPlan;
    }
    
    private bool isEnumerable(Type type)
    {
        if (type.IsArray) return true;

        if (!type.IsGenericType) return false;

        if (type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) return true;
        if (type.GetGenericTypeDefinition() == typeof(IList<>)) return true;
        if (type.GetGenericTypeDefinition() == typeof(List<>)) return true;
        if (type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)) return true;

        return false;
    }
    
    private ServiceFamily findFamily(Type serviceType)
    {
        if (_families.TryFind(serviceType, out var family)) return family;
        
        if (isEnumerable(serviceType))
        {
            family = new ArrayFamily(serviceType);
            _families = _families.AddOrUpdate(serviceType, family);
            return family;
        }

        if (serviceType.IsGenericType && serviceType.IsNotConcrete())
        {
            var templateType = serviceType.GetGenericTypeDefinition();
            var templatedParameterTypes = serviceType.GetGenericArguments();
        
            if (_families.TryFind(templateType, out var generic))
            {
                family = generic.Close(templatedParameterTypes);
                _families = _families.AddOrUpdate(serviceType, family);
                return family;
            }
            else
            {
                // Memoize the "miss"
                family = new ServiceFamily(serviceType, ArraySegment<ServiceDescriptor>.Empty);
                _families = _families.AddOrUpdate(serviceType, family);
                return family;
            }
        }

        if ((serviceType.IsPublic || serviceType.IsNestedPublic)  && serviceType.IsConcrete())
        {
            var descriptor = new ServiceDescriptor(serviceType, serviceType, ServiceLifetime.Scoped);
            family = new ServiceFamily(serviceType, [descriptor]);
            _families = _families.AddOrUpdate(serviceType, family);

            return family;
        }

        return new ServiceFamily(serviceType, []);
    }

    private ServiceDescriptor? findDefaultDescriptor(Type type)
    {
        var family = findFamily(type);
        return family.Default;
    }

    public ServicePlan? FindDefault(Type type, List<ServiceDescriptor> trail)
    {
        if (_defaults.TryFind(type, out var plan)) return plan;

        var family = findFamily(type);
        plan = family.BuildDefaultPlan(this, trail);

        // Memoize the "miss" as well
        _defaults = _defaults.AddOrUpdate(type, plan);
        
        return plan;
    }

    public IReadOnlyList<ServicePlan> FindAll(Type serviceType, List<ServiceDescriptor> trail)
    {
        return findFamily(serviceType).Services.Select(descriptor => planFor(descriptor, trail)).ToArray();
    }

    public object BuildFromType(Type concreteType)
    {
        var constructor = concreteType.GetConstructors().Single();
        var dependencies = constructor.GetParameters().Select(x => _provider.GetService(x.ParameterType)).ToArray();
        return Activator.CreateInstance(concreteType, dependencies);
    }

    public bool HasMultiplesOf(Type variableType)
    {
        return findFamily(variableType).Services.Count > 1;
    }
    
    /// <summary>
    /// Polyfill to make IServiceProvider work like Lamar's ability
    /// to create unknown concrete types
    /// </summary>
    /// <param name="provider"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T QuickBuild<T>()
    {
        return (T)QuickBuild(typeof(T));
    }
    
    /// <summary>
    /// Polyfill to make IServiceProvider work like Lamar's ability
    /// to create unknown concrete types
    /// </summary>
    /// <param name="provider"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public object QuickBuild(Type concreteType)
    {
        var constructor = concreteType.GetConstructors().Single();
        var args = constructor
            .GetParameters()
            .Select(x => _provider.GetService(x.ParameterType))
            .ToArray();

        return Activator.CreateInstance(concreteType, args);
    }
}