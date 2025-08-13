using Microsoft.AspNetCore.Http;

namespace CreditCardRegistration.Extensions
{
	public static class HttpContextExtensions
	{
		public static string? GetRemoteIPAddress(this HttpContext context)
		{
			return context.Connection.RemoteIpAddress?.ToString();
		}
	}
}