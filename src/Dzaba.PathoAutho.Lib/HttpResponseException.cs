using System.Net;

namespace Dzaba.PathoAutho.Lib;

[Serializable]
public class HttpResponseException : Exception
{
	public HttpStatusCode StatusCode { get; }

	public HttpResponseException(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
    }

    public HttpResponseException(HttpStatusCode statusCode, string message)
		: base(message)
	{
		StatusCode = statusCode;
	}

	public HttpResponseException(HttpStatusCode statusCode, string message, Exception inner)
		: base(message, inner)
    {
        StatusCode = statusCode;
    }
}
