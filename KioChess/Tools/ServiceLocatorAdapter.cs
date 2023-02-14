﻿using Unity;
using CommonServiceLocator;

public class ServiceLocatorAdapter : ServiceLocatorImplBase
{
    private readonly IUnityContainer _unityContainer;

    public ServiceLocatorAdapter(IUnityContainer unityContainer)
    {
        _unityContainer = unityContainer;
    }

    protected override object DoGetInstance(Type serviceType, string key)
    {
        return _unityContainer.Resolve(serviceType, key);
    }

    protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
    {
        return _unityContainer.ResolveAll(serviceType);
    }
}
