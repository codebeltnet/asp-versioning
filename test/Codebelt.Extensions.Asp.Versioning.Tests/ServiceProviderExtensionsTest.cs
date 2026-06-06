using System;
using System.Linq;
using Codebelt.Extensions.Asp.Versioning.Assets;
using Codebelt.Extensions.Xunit;
using Cuemon.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Codebelt.Extensions.Asp.Versioning
{
    public class ServiceProviderExtensionsTest : Test
    {
        public ServiceProviderExtensionsTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GetServiceDescriptors_ShouldGetDescriptors_WhenProviderIsWrappedByAspVersioningInjectApiVersion()
        {
            var services = new ServiceCollection();
            services.AddSingleton(new object());

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var wrappedProvider = new global::Asp.Versioning.Builder.EndpointBuilderFinalizer.InjectApiVersion(serviceProvider);

                var descriptors = wrappedProvider.GetServiceDescriptors().ToList();

                Assert.EndsWith("Asp.Versioning.Builder.EndpointBuilderFinalizer+InjectApiVersion", wrappedProvider.GetType().FullName, StringComparison.Ordinal);
                Assert.Contains(descriptors, descriptor => descriptor.ServiceType == typeof(object));
            }
        }

        [Fact]
        public void GetServiceDescriptors_ShouldGetDescriptors_WhenProviderWrapsServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddSingleton(new object());

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var wrappedProvider = new DelegatingServiceProvider(serviceProvider);

                var descriptors = wrappedProvider.GetServiceDescriptors().ToList();

                Assert.Contains(descriptors, descriptor => descriptor.ServiceType == typeof(object));
            }
        }
    }
}

namespace Asp.Versioning.Builder
{
    internal static class EndpointBuilderFinalizer
    {
        internal sealed class InjectApiVersion : IServiceProvider
        {
            private readonly IServiceProvider _serviceProvider;

            public InjectApiVersion(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public object GetService(Type serviceType)
            {
                return _serviceProvider.GetService(serviceType);
            }
        }
    }
}
