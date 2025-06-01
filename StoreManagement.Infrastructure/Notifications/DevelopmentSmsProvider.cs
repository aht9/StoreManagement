namespace StoreManagement.Infrastructure.Notifications;

public class DevelopmentSmsProvider : ISmsProvider
{
    public string Name => "Development";

    public Task<SmsResult> SendAsync(SmsMessage message, CancellationToken cancellationToken = default)
    {
        string verificationCode = ExtractVerificationCode(message.Content);
        return Task.FromResult(SmsResult.Successful(verificationCode));
    }

    public Task<SmsDeliveryStatus> CheckDeliveryStatusAsync(SmsMessage message, CancellationToken cancellationToken = default)
    {
        // In development mode, always return delivered status
        return Task.FromResult(SmsDeliveryStatus.Delivered);
    }

    public bool CanHandleTemplate(SmsTemplate template)
    {
        // Development provider can handle any template
        return true;
    }


    //برای استخراج کد تایید از محتوای پیام(به صورت آزمایشی برای استفاده در برنامه)
    private string ExtractVerificationCode(string content)
    {
        // Try to extract the verification code from the message content
        // This assumes the format "کد تأیید شما: 123456"
        try
        {
            var parts = content.Split(':');
            if (parts.Length >= 2)
            {
                // Get the part after the colon, trim it, and take only digits
                var codePart = parts[1].Trim();
                var digits = new string(codePart.Where(char.IsDigit).ToArray());

                if (!string.IsNullOrEmpty(digits))
                {
                    return digits;
                }
            }

            // If we couldn't extract the code, return a placeholder
            return "UNKNOWN_CODE";
        }
        catch
        {
            return "UNKNOWN_CODE";
        }
    }
}