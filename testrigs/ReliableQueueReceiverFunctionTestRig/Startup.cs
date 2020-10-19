/*
 * This file is part of OpenCollar.Azure.ReliableQueue.
 *
 * OpenCollar.Azure.ReliableQueue is free software: you can redistribute it
 * and/or modify it under the terms of the GNU General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 *
 * OpenCollar.Azure.ReliableQueue is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
 * License for more details.
 *
 * You should have received a copy of the GNU General Public License along with
 * OpenCollar.Azure.ReliableQueue.  If not, see <https://www.gnu.org/licenses/>.
 *
 * Copyright © 2020 Jonathan Evans (jevans@open-collar.org.uk).
 */

using System;
using System.Diagnostics;
using System.IO;

using JetBrains.Annotations;

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OpenCollar.Azure.ReliableQueue;
using OpenCollar.Azure.Storage;

using ReliableQueueReceiverFunctionTestRig;

[assembly: FunctionsStartup(typeof(Startup))]

namespace ReliableQueueReceiverFunctionTestRig
{
    /// <summary>
    ///     Defines the environment in which a function is executing.
    /// </summary>
    public sealed class Startup : FunctionsStartup
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        public Startup()
        {
            Emulator.StartEmulatorIfRequired(IsInAzure);

            var configBuilder = new ConfigurationBuilder();

            ConfigureConfiguration(configBuilder);

            // ReSharper disable once AssignNullToNotNullAttribute
            Configuration = configBuilder.Build();
        }

        /// <summary>
        ///     Gets the object from which to read configuration.
        /// </summary>
        /// <value>
        ///     The object from which to read configuration.
        /// </value>
        [NotNull]
        private IConfigurationRoot Configuration { get; }

        /// <summary>
        ///     Gets a value indicating whether this process is in the Azure cloud.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if this process is in the Azure cloud; otherwise, <see langword="false" />.
        /// </value>
        private static bool IsInAzure => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(@"WEBSITE_HOME_STAMPNAME"));

        /// <summary>
        ///     Configures the function application.
        /// </summary>
        /// <param name="builder">
        ///     The builder with which to configure the application.
        /// </param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;

            services.AddLogging(logging =>
            {
                Debug.Assert(logging != null, nameof(logging) + " != null");

                ConfigureLogging(Configuration, logging);
            });

            ConfigureServices(services);
        }

        /// <summary>
        ///     Configures the logging service.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration service.
        /// </param>
        /// <param name="builder">
        ///     The builder to configure.
        /// </param>
        /// <exception cref="ConfigurationException">
        /// </exception>
        private static void ConfigureLogging([NotNull] IConfigurationRoot configuration, [NotNull] ILoggingBuilder builder)
        {
            // Start with this as the base-line and then allow configuration and other manual changes to tweak it...
            builder.SetMinimumLevel(LogLevel.Information);

            // ReSharper disable PossibleNullReferenceException
            builder.AddConfiguration(configuration.GetSection(@"Logging"));
            if(!IsInAzure && Debugger.IsAttached)
            {
                // Add debugger logging only when a debugger is attached.
                builder.AddConsole();

                // builder.AddDebug(); <--- This seems to break the app when run from func.exe on the desktop, at least
                // it does for me (JDE).
                builder.SetMinimumLevel(LogLevel.Trace);
            }
        }

        /// <summary>
        ///     Configures the configuration services.
        /// </summary>
        /// <param name="configBuilder">
        ///     The configuration builder.
        /// </param>
        private static void ConfigureConfiguration([NotNull] IConfigurationBuilder configBuilder)
        {
            var isInAzure = IsInAzure;

            string rootDirectory;
            if(isInAzure)
            {
#pragma warning disable CS8604 // Possible null reference argument.
                rootDirectory = Path.Combine(Environment.GetEnvironmentVariable(@"HOME"), @"site", @"wwwroot");
#pragma warning restore CS8604 // Possible null reference argument.
            }
            else
            {
                rootDirectory = Environment.CurrentDirectory;
            }

            // Order is important and appsettings should come after host
            configBuilder.SetBasePath(rootDirectory).AddJsonFile(@"host.json", false, true).AddJsonFile(@"appsettings.json", false, true)
                .AddEnvironmentVariables();

            // Only use the developer settings if the debugger is attached and the file exists.
            if(!isInAzure && Debugger.IsAttached)
            {
                // We don't get development app settings for free on Function Apps, but we can emulate it easily enough.
                var devConfigPath = Path.Combine(rootDirectory, @"local.settings.json");
                if(File.Exists(devConfigPath))
                {
                    configBuilder.AddJsonFile(devConfigPath, true, true);
                }
            }
        }

        /// <summary>
        ///     Configures the services that will be available in the web application.
        /// </summary>
        /// <param name="services">
        ///     The collection in which to register services.
        /// </param>
        /// <remarks>
        ///     This method gets called by the runtime. Use this method to add services to the container.
        /// </remarks>
        private void ConfigureServices([NotNull] IServiceCollection services)
        {
            services.AddSingleton(Configuration); // Register the configuration as a service so other code can access it.
            services.AddReliableQueues();
        }
    }
}