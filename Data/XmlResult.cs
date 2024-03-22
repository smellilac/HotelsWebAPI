
public class XmlResult<T> : IResult
{
    private static readonly XmlSerializer _xmlSerializer = new(typeof(T));
    private readonly T? _result;
    public XmlResult(T result) => _result = result;
    public Task ExecuteAsync(HttpContext httpContext)
    {
        using var ms = new MemoryStream();

        _xmlSerializer.Serialize(ms, _result);
        httpContext.Response.ContentType = "application/xml";
        ms.Position = 0;        // the pointer to read data from the ms stream 
        //will be at the beginning of the data every time the method is called
        return ms.CopyToAsync(httpContext.Response.Body);
    }
}

public static class XmlResultExtensions
{
    public static IResult Xml<T>(this IResultExtensions _, T result ) =>
        new XmlResult<T>(result);
}