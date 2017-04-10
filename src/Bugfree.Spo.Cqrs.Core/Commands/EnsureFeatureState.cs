using System;
using System.Linq;
using Microsoft.SharePoint.Client;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public enum DesiredFeatureState
    {
        None = 0,
        Activated,
        Deactivated
    }

    public class EnsureFeatureState : Command
    {
        public class Feature
        {
            public static readonly Guid OpenInClient = new Guid("8a4b8de2-6fd8-41e9-923c-c7c3c00f8295");
            public static readonly Guid MinimalDownloadStrategy = new Guid("87294c72-f260-42f3-a41b-981a2ffce37a");
            public static readonly Guid PublishingInfrastructure = new Guid("f6924d36-2fa8-4f0b-b16d-06b7250180fa");
        }

        public EnsureFeatureState(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, Guid featureId, FeatureDefinitionScope scope, DesiredFeatureState desiredState)
        {
            Logger.Verbose($"Started executing {nameof(EnsureFeatureState)} for id '{featureId}' and scope '{scope}' on '{ctx.Url}'");

            var features =
                scope == FeatureDefinitionScope.Site
                ? ctx.Site.Features
                : scope == FeatureDefinitionScope.Web
                    ? ctx.Web.Features
                    : throw new ArgumentException($"Unsupported scope: {scope}");

            ctx.Load(features);
            ctx.ExecuteQuery();

            var activated = features.Any(f => f.DefinitionId == featureId);
            if (activated && desiredState == DesiredFeatureState.Activated)
            {
                Logger.Warning($"Feature with Id '{featureId}' already activated");
                return;
            }
            else if (activated && desiredState == DesiredFeatureState.Deactivated)
            {
                features.Remove(featureId, false);
                ctx.ExecuteQuery();
            }
            else if (!activated && desiredState == DesiredFeatureState.Activated)
            {
                features.Add(featureId, false, FeatureDefinitionScope.None);
                ctx.ExecuteQuery();
            }
            else if (!activated && desiredState == DesiredFeatureState.Deactivated)
            {
                Logger.Warning($"Feature with Id '{featureId}' already deactivated");
                return;
            }
        }
    }
}
