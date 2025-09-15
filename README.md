# StoreManagement | 🏪 Store Management System | سیستم مدیریت فروشگاه

[![Build Status](https://img.shields.io/github/actions/workflow/status/aht9/StoreManagement/dotnet-desktop.yml?branch=main)](https://github.com/aht9/StoreManagement/actions)  
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)  
[![Version](https://img.shields.io/github/v/release/aht9/StoreManagement)](https://github.com/aht9/StoreManagement/releases)


---

## 🚀 Overview | معرفی کلی

This is a full-featured **Store Management System** built using **.NET Core 9**, **WPF**, and a rich set of architectural patterns and libraries. Designed with scalability, maintainability, and clean separation of concerns in mind.

این پروژه یک سیستم کامل مدیریت فروشگاه است که با استفاده از **.NET Core 9**، **WPF** و مجموعه‌ای از الگوهای معماری مدرن طراحی شده. هدف آن ایجاد ساختاری قابل توسعه، قابل نگهداری و با جداسازی مسئولیت‌هاست.

---

## 🧠 Architecture & Technologies | معماری و تکنولوژی‌ها

| 🔧 تکنولوژی / Technology | 📌 توضیحات / Description |
|--------------------------|---------------------------|
| 🧱 **Rich Domain Model** | منطق کسب‌وکار در قلب دامنه |
| 📚 **CQRS + MediatR** | جداسازی خواندن و نوشتن + مدیریت درخواست‌ها |
| 🛢 **EF Core + Dapper** | ORM قدرتمند + دسترسی مستقیم به دیتابیس |
| 🎯 **Specifications Pattern** | فیلترهای قابل ترکیب برای کوئری‌ها |
| 🔄 **AutoMapper** | نگاشت خودکار بین DTO و مدل‌ها |
| ✅ **FluentValidation** | اعتبارسنجی تمیز و قابل تست |
| 🖼 **WPF + MVVM** | رابط کاربری مدرن با جداسازی منطقی |
| 🎨 **MaterialDesignInXamlToolkit** | طراحی متریال برای UI زیبا و مدرن |
| 🧪 **Dependency Injection** | تزریق وابستگی با Microsoft.Extensions.DependencyInjection |
| 🧩 **Dialog & Notification Abstraction** | سرویس‌های قابل تست برای دیالوگ و اعلان‌ها |
| 🧠 **Full Domain-Driven Design** | معماری کامل مبتنی بر DDD |

---

## 📸 Screenshots | تصاویر محیط برنامه *(اختیاری)*

### مدیریت تراکنش ها
![Dashboard Screenshot](assets/Transaction.png)

### مدیریت دسته بندی ها
![Products Screenshot](assets/Category.png)

### فروش
![Sale Screenshot](assets/Sale.png)

### مدیریت اقساط
![Installement Screenshot](assets/Installment.png)

---

## 🛠 How to Run | نحوه اجرا

```bash
git clone https://github.com/aht9/StoreManagement.git
cd StoreManagement
# Open with Visual Studio 2022+
# Make sure SQL Server is running
# Apply EF migrations or configure Dapper connection
```

---

## 📦 Features | قابلیت‌ها

- ✅ مدیریت کالاها، مشتریان و فاکتورها
- 📊 گزارش‌گیری از فروش‌ها
- 🧠 منطق دامنه غنی و قابل تست
- 🔄 معماری CQRS برای جداسازی عملیات‌ها
- 🎨 رابط کاربری مدرن با طراحی متریال
- 🔔 سرویس‌های اعلان و دیالوگ قابل تست
- 🧪 اعتبارسنجی با FluentValidation

---

## 📄 License | مجوز

This project is licensed under the MIT License.  
این پروژه تحت مجوز MIT منتشر شده است.

---




