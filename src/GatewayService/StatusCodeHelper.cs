namespace GatewayService;

public static class StatusCodeHelper
{
    public static bool IsSuccessCode(int code) => code < 300;
    public static bool IsClientError(int code) => 400 <= code && code < 500;
    public static bool IsServerError(int code) => code >= 500;
}
