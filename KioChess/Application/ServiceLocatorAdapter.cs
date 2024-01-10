using System;
using System.Collections.Generic;
using CommonServiceLocator;
using Unity;

namespace Kgb.ChessApp;

public class ServiceLocatorAdapter : ServiceLocatorImplBase
{
    private readonly IUnityContainer _unityContainer;

    public ServiceLocatorAdapter(IUnityContainer unityContainer)
    {
        _unityContainer = unityContainer;
    }

    protected override object DoGetInstance(Type serviceType, string key) => _unityContainer.Resolve(serviceType, key);

    protected override IEnumerable<object> DoGetAllInstances(Type serviceType) => _unityContainer.ResolveAll(serviceType);
}