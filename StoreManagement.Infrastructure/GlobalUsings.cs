// Global using directives

global using System.ComponentModel.DataAnnotations;
global using System.Data;
global using Dapper;
global using MediatR;
global using Microsoft.Data.SqlClient;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Storage;
global using Microsoft.Extensions.Configuration;
global using StoreManagement.Domain.Aggregates.BankAccounts;
global using StoreManagement.Domain.Aggregates.Customers;
global using StoreManagement.Domain.Aggregates.Installments;
global using StoreManagement.Domain.Aggregates.Inventory;
global using StoreManagement.Domain.Aggregates.Invoices;
global using StoreManagement.Domain.Aggregates.Products;
global using StoreManagement.Domain.Aggregates.Stores;
global using StoreManagement.Domain.Common.Interface;
global using StoreManagement.Domain.Enums;
global using StoreManagement.Infrastructure.Data.DbContexts;
global using StoreManagement.Infrastructure.Extensions;