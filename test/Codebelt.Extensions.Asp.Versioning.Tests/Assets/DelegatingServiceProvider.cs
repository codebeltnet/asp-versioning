using System;

namespace Codebelt.Extensions.Asp.Versioning.Assets
{
    internal sealed class DelegatingServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public DelegatingServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }
    }
}
