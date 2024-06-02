using System.Text;

namespace Engine.Models.Helpers;

public static class ExceptionExtensions
{
    public static string ToFormattedString(this Exception exception)
    {
        IEnumerable<string> messages = exception
            .GetAllExceptions()
            .Where(e => !string.IsNullOrWhiteSpace(e.Message))
            .Select(e =>
            {
                StringBuilder builder = new StringBuilder();

                builder.AppendLine(e.Message.Trim()).AppendLine(e.StackTrace);

                return builder.ToString();
            });

        string flattened = string.Join(Environment.NewLine, messages); // <-- the separator here
        return flattened;
    }

    public static IEnumerable<Exception> GetAllExceptions(this Exception exception)
    {
        yield return exception;

        if (exception is AggregateException aggrEx)
        {
            foreach (Exception innerEx in aggrEx.InnerExceptions.SelectMany(e => e.GetAllExceptions()))
            {
                yield return innerEx;
            }
        }
        else if (exception.InnerException != null)
        {
            foreach (Exception innerEx in exception.InnerException.GetAllExceptions())
            {
                yield return innerEx;
            }
        }
    }
}
