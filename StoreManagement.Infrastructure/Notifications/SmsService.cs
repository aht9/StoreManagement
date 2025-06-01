using StoreManagement.Domain.ValueObjects;

namespace StoreManagement.Infrastructure.Notifications;

public class SmsService(
    ISmsProviderFactory providerFactory,
    IGenericRepository<SmsMessage> smsRepository,
    IGenericRepository<SmsTemplate> templateRepository,
    IGenericRepository<SmsLog> logRepository)
    : ISmsService
{
    private readonly ISmsProviderFactory _providerFactory = providerFactory;
    private readonly IGenericRepository<SmsMessage> _smsRepository = smsRepository;
    private readonly IGenericRepository<SmsTemplate> _templateRepository = templateRepository;
    private readonly IGenericRepository<SmsLog> _logRepository = logRepository;

    public async Task<SmsMessage> SendAsync(string phoneNumber, string content, string? providerName = null,
        CancellationToken cancellationToken = default)
    {
        var phoneNumberResult = PhoneNumber.Create(phoneNumber);
        if (phoneNumberResult.IsFailure)
        {
            throw new ArgumentException(phoneNumberResult.Error);
        }

        var formattedPhoneNumber = phoneNumberResult.Value.GetFormattedNumber(false);

        ISmsProvider provider = providerName != null
            ? _providerFactory.GetProvider(providerName)
            : _providerFactory.GetDefaultProvider();

        var smsMessage = new SmsMessage(formattedPhoneNumber, content, provider.Name);
        await _smsRepository.AddAsync(smsMessage, cancellationToken);
        try
        {
            var requestLog = new SmsLog(smsMessage.Id, provider.Name, SmsLogType.RequestSent, content);
            await _logRepository.AddAsync(requestLog, cancellationToken);

            // ارسال پیام
            var result = await provider.SendAsync(smsMessage, cancellationToken);

            if (result.Success)
            {
                smsMessage.SetTrackingCode(result.TrackingCode);
                smsMessage.MarkAsSent();

                var responseLog = new SmsLog(
                    smsMessage.Id,
                    provider.Name,
                    SmsLogType.ResponseReceived,
                    $"TrackingCode: {result.TrackingCode}");

                await _logRepository.AddAsync(responseLog, cancellationToken);
            }
            else
            {
                smsMessage.MarkAsFailed(result.ErrorMessage);

                var errorLog = new SmsLog(
                    smsMessage.Id,
                    provider.Name,
                    SmsLogType.Error,
                    result.ErrorMessage);

                await _logRepository.AddAsync(errorLog, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            smsMessage.MarkAsFailed($"Exception: {ex.Message}");

            var exceptionLog = new SmsLog(
                smsMessage.Id,
                provider.Name,
                SmsLogType.Error,
                ex.ToString());

            await _logRepository.AddAsync(exceptionLog, cancellationToken);
        }

        await _smsRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        return smsMessage;
    }

    public async Task<SmsMessage> SendWithTemplateAsync(string phoneNumber, long templateId, Dictionary<string, string> parameters, string? providerName = null,
        CancellationToken cancellationToken = default)
    {
        var template = await _templateRepository.GetByIdAsync(templateId, cancellationToken);
        if (template == null)
        {
            throw new ArgumentException($"Template with ID {templateId} not found.");
        }

        if (!template.IsActive)
        {
            throw new ArgumentException($"Template with ID {templateId} is not active.");
        }

        ISmsProvider provider = providerName != null
            ? _providerFactory.GetProvider(providerName)
            : _providerFactory.GetDefaultProvider();

        if (!provider.CanHandleTemplate(template))
        {
            throw new ArgumentException($"Provider {provider.Name} does not support this template.");
        }

        var phoneNumberResult = PhoneNumber.Create(phoneNumber);
        if (phoneNumberResult.IsFailure)
        {
            throw new ArgumentException(phoneNumberResult.Error);
        }
        var formattedPhoneNumber = phoneNumberResult.Value.GetFormattedNumber(false);

        var content = template.FormatContent(parameters);

        var smsMessage = new SmsMessage(formattedPhoneNumber, template.Id, parameters, provider.Name);
        smsMessage.SetContent(content);
        await _smsRepository.AddAsync(smsMessage, cancellationToken);

        return await SendAsync(formattedPhoneNumber, content, provider.Name, cancellationToken);
    }

    public async Task<SmsDeliveryStatus> CheckDeliveryStatusAsync(long messageId, CancellationToken cancellationToken = default)
    {
        var message = await _smsRepository.GetByIdAsync(messageId, cancellationToken);
        if (message == null)
        {
            throw new ArgumentException($"SMS message with ID {messageId} not found.");
        }

        // اگر پیام هنوز ارسال نشده است
        if (message.Status == SmsStatus.Pending)
        {
            return SmsDeliveryStatus.Unknown;
        }

        // اگر پیام قبلاً به عنوان تحویل‌داده‌شده علامت‌گذاری شده است
        if (message.Status == SmsStatus.Delivered)
        {
            return SmsDeliveryStatus.Delivered;
        }

        // اگر پیام قبلاً به عنوان ناموفق علامت‌گذاری شده است
        if (message.Status == SmsStatus.Failed)
        {
            return SmsDeliveryStatus.Failed;
        }

        // اگر کد رهگیری وجود ندارد
        if (string.IsNullOrEmpty(message.TrackingCode))
        {
            return SmsDeliveryStatus.Unknown;
        }

        try
        {
            // بررسی وضعیت تحویل
            var provider = _providerFactory.GetProvider(message.ProviderName);
            var status = await provider.CheckDeliveryStatusAsync(message, cancellationToken);

            // بروزرسانی وضعیت پیام
            switch (status)
            {
                case SmsDeliveryStatus.Delivered:
                    message.MarkAsDelivered();
                    break;
                case SmsDeliveryStatus.Failed:
                    message.MarkAsFailed("Delivery failed according to provider");
                    break;
            }

            // ثبت لاگ بروزرسانی وضعیت
            var statusLog = new SmsLog(
                message.Id,
                message.ProviderName,
                SmsLogType.StatusUpdate,
                status.ToString());
            await _logRepository.AddAsync(statusLog, cancellationToken);

            // ذخیره تغییرات
            await _smsRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            return status;
        }
        catch (Exception ex)
        {
            // ثبت لاگ خطا
            var errorLog = new SmsLog(
                message.Id,
                message.ProviderName,
                SmsLogType.Error,
                $"Error checking status: {ex.Message}");
            await _logRepository.AddAsync(errorLog, cancellationToken);
            await _smsRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            return SmsDeliveryStatus.Unknown;
        }
    }

    public async Task<bool> RetryFailedMessageAsync(long messageId, string? providerName = null,
        CancellationToken cancellationToken = default)
    {
        var message = await _smsRepository.GetByIdAsync(messageId, cancellationToken);
        if (message == null)
        {
            throw new ArgumentException($"SMS message with ID {messageId} not found.");
        }

        // بررسی امکان تلاش مجدد
        if (!message.CanRetry(3)) // حداکثر 3 بار تلاش مجدد
        {
            return false;
        }

        // انتخاب Provider
        ISmsProvider provider;
        if (providerName != null)
        {
            provider = _providerFactory.GetProvider(providerName);
            message = new SmsMessage(message.PhoneNumber, message.Content, provider.Name);
            await _smsRepository.AddAsync(message, cancellationToken);
        }
        else
        {
            provider = _providerFactory.GetProvider(message.ProviderName);
            message.IncrementRetryCount();
        }

        try
        {
            // ثبت لاگ تلاش مجدد
            var retryLog = new SmsLog(
                message.Id,
                provider.Name,
                SmsLogType.RequestSent,
                $"Retry #{message.RetryCount}");
            await _logRepository.AddAsync(retryLog, cancellationToken);

            // ارسال پیام
            var result = await provider.SendAsync(message, cancellationToken);

            if (result.Success)
            {
                // ثبت کد رهگیری و علامت‌گذاری به عنوان ارسال‌شده
                message.SetTrackingCode(result.TrackingCode);
                message.MarkAsSent();

                // ثبت لاگ پاسخ
                var responseLog = new SmsLog(
                    message.Id,
                    provider.Name,
                    SmsLogType.ResponseReceived,
                    $"TrackingCode: {result.TrackingCode}");
                await _logRepository.AddAsync(responseLog, cancellationToken);

                // ذخیره تغییرات
                await _smsRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                return true;
            }
            else
            {
                // علامت‌گذاری به عنوان ناموفق
                message.MarkAsFailed(result.ErrorMessage);

                // ثبت لاگ خطا
                var errorLog = new SmsLog(
                    message.Id,
                    provider.Name,
                    SmsLogType.Error,
                    result.ErrorMessage);
                await _logRepository.AddAsync(errorLog, cancellationToken);

                // ذخیره تغییرات
                await _smsRepository.UnitOfWork.SaveChangesAsync(cancellationToken);


                return false;
            }
        }
        catch (Exception ex)
        {
            // علامت‌گذاری به عنوان ناموفق
            message.MarkAsFailed($"Exception: {ex.Message}");

            // ثبت لاگ خطا
            var exceptionLog = new SmsLog(
                message.Id,
                provider.Name,
                SmsLogType.Error,
                ex.ToString());
            await _logRepository.AddAsync(exceptionLog, cancellationToken);

            // ذخیره تغییرات
            await _smsRepository.UnitOfWork.SaveChangesAsync(cancellationToken);


            return false;
        }
    }
}