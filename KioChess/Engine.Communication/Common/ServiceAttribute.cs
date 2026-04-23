namespace Engine.Communication.Common;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ServiceAttribute : Attribute
{
    public string ServiceName { get; }
    public string PipeName { get; set; }
    public string Description { get; set; }

    public ServiceAttribute(string serviceName)
    {
        ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
    }
}
