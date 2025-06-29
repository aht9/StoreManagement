namespace StoreManagement.Application.DTOs.Parties;

/// <summary>
/// DTO برای نمایش اطلاعات طرف حساب‌ها (مشتریان و فروشگاه‌ها) در UI.
/// </summary>
public class PartyDto
{
    /// <summary>
    /// شناسه موجودیت (مشتری یا فروشگاه)
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// نام کامل (برای مشتری: نام + نام خانوادگی، برای فروشگاه: نام فروشگاه)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// یک کد قابل نمایش برای جستجو و شناسایی.
    /// برای مشتری: کد ملی. برای فروشگاه: می‌تواند خالی باشد یا شناسه دیگری در آینده باشد.
    /// </summary>
    public string DisplayCode { get; set; }

    /// <summary>
    /// نوع طرف حساب: "مشتری" یا "فروشگاه"
    /// </summary>
    public string PartyType { get; set; }
}