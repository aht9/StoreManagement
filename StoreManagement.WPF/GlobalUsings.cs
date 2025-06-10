// Global using directives

global using System.Collections.ObjectModel;
global using System.IO;
global using System.Windows;
global using CommunityToolkit.Mvvm.ComponentModel;
global using CommunityToolkit.Mvvm.Input;
global using MediatR;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Serilog;
global using StoreManagement.Application.Behaviors;
global using StoreManagement.Application.DTOs.Customers;
global using StoreManagement.Application.Features.Customers.Command;
global using StoreManagement.Application.Features.Customers.Query;
global using StoreManagement.Domain.Aggregates.Customers;
global using StoreManagement.Infrastructure;
global using StoreManagement.Infrastructure.Data.DbContexts;
global using StoreManagement.WPF.Extensions;
global using StoreManagement.WPF.ViewModels;