﻿using QuestPDF.Infrastructure;

namespace StoreManagement.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static IHost? AppHost { get; private set; }
        public static IConfiguration? Configuration { get; private set; }

        public App()
        {
            // Initialize Serilog (before host is built)
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "Bootstrap-.txt"),
                    rollingInterval: RollingInterval.Day)
                .CreateBootstrapLogger();

            // Build configuration
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Configure Serilog with settings
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .CreateLogger();

            Log.Information("Serilog Logger configured.");

            // Build the host
            AppHost = Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(Configuration);
                    services.AddLogging(loggingBuilder =>
                    {
                        loggingBuilder.ClearProviders(); // Remove other logging providers
                        loggingBuilder.AddSerilog(dispose: true);
                    });
                    ConfigureServices(services, context.Configuration);
                })
                .Build();
        }


        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            await AppHost!.StartAsync();

            Log.Information("Application Starting Up...");

            // Global exception handling
            SetupGlobalExceptionHandling();

            try
            {
                var serviceProvider = AppHost.Services;
                serviceProvider.ApplyPendingMigrations();
                var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Fatal error during application startup. Could not resolve or show MainWindow.");

                MessageBox.Show(
                    "A critical error occurred and the application cannot start. Please check the logs for more details.",
                    "Application Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                if (Current != null) Current.Shutdown(-1);
            }
        }

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            services.AddInfrastructure(configuration);

            services.AddPublicServiceCollection();

            services.AddViewModelServiceCollection();

            // Main Window
            services.AddTransient<MainWindow>();
        }

        private void SetupGlobalExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Log.Fatal((Exception)args.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");
                ShowErrorAndShutdown((Exception)args.ExceptionObject, "unhandled domain exception");
            };

            DispatcherUnhandledException += (sender, args) =>
            {
                Log.Error(args.Exception, "DispatcherUnhandledException");
                args.Handled = true;
                ShowErrorAndShutdown(args.Exception, "unhandled UI exception");
            };


            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Log.Error(args.Exception, "TaskScheduler.UnobservedTaskException");
                args.SetObserved();
            };
        }

        private void ShowErrorAndShutdown(Exception ex, string errorType)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => ShowErrorAndShutdown(ex, errorType));
                return;
            }

            var errorMessage =
                $"An unexpected {errorType} occurred: {ex.Message}\n\nThe application will now close. Please check the logs for more details.";
            Log.Fatal(ex, $"Unhandled {errorType} leading to shutdown.");

            MessageBox.Show(errorMessage, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);

            Current.Shutdown();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            Log.Information("Application Shutting Down...");
            if (AppHost != null)
            {
                await AppHost.StopAsync();
                AppHost.Dispose();
            }

            await Log.CloseAndFlushAsync();
            base.OnExit(e);
        }
    }
}