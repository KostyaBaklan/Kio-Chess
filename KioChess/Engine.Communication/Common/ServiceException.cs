namespace Engine.Communication.Common;

public class ServiceException : Exception
{
    public ServiceException() : base()
    {
    }

    public ServiceException(string message) : base(message)
    {
    }

    public ServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class ServiceConnectionException : ServiceException
{
    public ServiceConnectionException(string message) : base(message)
    {
    }

    public ServiceConnectionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class ServiceTimeoutException : ServiceException
{
    public ServiceTimeoutException(string message) : base(message)
    {
    }
}

public class ServiceNotFoundException : ServiceException
{
    public string ServiceName { get; }

    public ServiceNotFoundException(string serviceName) 
        : base($"Service '{serviceName}' not found or not running")
    {
        ServiceName = serviceName;
    }
}
