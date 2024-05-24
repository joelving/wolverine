using JasperFx.CodeGeneration.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Wolverine.Codegen;

internal class SingletonPlan : ServicePlan
{
    public SingletonPlan(ServiceDescriptor descriptor) : base(descriptor)
    {
        if (descriptor.Lifetime != ServiceLifetime.Singleton)
        {
            throw new ArgumentOutOfRangeException(nameof(descriptor),
                $"Must be a singleton, but {descriptor} was {descriptor.Lifetime}");
        }
    }

    protected override bool requiresServiceProvider(IMethodVariables method)
    {
        return false;
    }

    public override string WhyRequireServiceProvider(IMethodVariables method)
    {
        return "It does not";
    }

    public override Variable CreateVariable(ServiceVariables resolverVariables)
    {
        return new InjectedSingleton(Descriptor);
    }
}