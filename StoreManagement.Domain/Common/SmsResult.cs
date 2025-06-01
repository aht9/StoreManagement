namespace StoreManagement.Domain.Common;

public class SmsResult
{
    public bool Success { get; private set; }
    public string? TrackingCode { get; private set; }
    public string? ErrorMessage { get; private set; }

    private SmsResult(bool success, string? trackingCode = null, string? errorMessage = null)
    {
        Success = success;
        TrackingCode = trackingCode;
        ErrorMessage = errorMessage;
    }

    public static SmsResult Successful(string trackingCode)
    {
        return new SmsResult(true, trackingCode);
    }

    public static SmsResult Failed(string errorMessage)
    {
        return new SmsResult(false, errorMessage: errorMessage);
    }
}