using System;
using System.Collections.Generic;
using Autofac;
using NuLink.Lib.Abstractions;
using NuLink.Lib.Commands;
using NuLink.Lib.MsBuildFormat;
using NuLink.Lib.NuGetFormat;
using NuLink.Lib.SourceControls;
using NuLink.Lib.Workspaces;

namespace NuLink.Lib
{
    public class CommandFactory
    {
        private readonly CommandOptions _options;
        private readonly Action<ContainerBuilder> _overrideServices;

        public CommandFactory(
            CommandOptions options,
            Action<ContainerBuilder> overrideServices = null)
        {
            _options = options;
            _overrideServices = overrideServices;
        }

        public INuLinkCommand CreateCommand(string name)
        {
            var builder = new ContainerBuilder();

            RegisterComponents();

            var container = builder.Build();

            return container.ResolveKeyed<INuLinkCommand>(name);
            
            void RegisterComponents()
            {
                RegisterDefaultServices();
                RegisterCommands();
                
                _overrideServices?.Invoke(builder);
            }

            void RegisterDefaultServices()
            {
                builder.RegisterType<RealEnvironment>()
                    .As<IImmutableEnvironment, IEnvironmentEffect>()
                    .SingleInstance();

                builder.RegisterType<WorkspaceLoader>()
                    .As<IWorkspaceLoader>()
                    .SingleInstance();

                builder.RegisterType<AutoDetectingSourceControl>()
                    .As<ISourceControl>()
                    .SingleInstance();

                builder.RegisterType<JsonConfigPersistor>()
                    .AsSelf()
                    .SingleInstance();

                builder.RegisterType<SlnFilePersistor>()
                    .AsSelf()
                    .SingleInstance();

                builder.RegisterType<NuGetMetadataLoader>()
                    .AsSelf()
                    .SingleInstance();
            }

            void RegisterCommands()
            {
                builder.RegisterType<InitCommand>()
                    .Keyed<INuLinkCommand>("init")
                    .InstancePerDependency();
            }
        }
    }
}