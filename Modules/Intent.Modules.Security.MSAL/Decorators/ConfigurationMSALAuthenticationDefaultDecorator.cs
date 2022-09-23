using Intent.Engine;
using Intent.Modules.Common;
using Intent.Modules.Security.MSAL.Templates.ConfigurationMSALAuthentication;
using Intent.RoslynWeaver.Attributes;
using System.Collections.Generic;
using System.Linq;

[assembly: DefaultIntentManaged(Mode.Fully)]
[assembly: IntentTemplate("Intent.ModuleBuilder.Templates.TemplateDecorator", Version = "1.0")]

namespace Intent.Modules.Security.MSAL.Decorators
{
    [IntentManaged(Mode.Merge)]
    public class ConfigurationMSALAuthenticationDefaultDecorator : MSALAuthenticationDecorator, IDeclareUsings
    {
        [IntentManaged(Mode.Fully)]
        public const string DecoratorId = "Intent.Security.MSAL.ConfigurationMSALAuthenticationDefaultDecorator";

        [IntentManaged(Mode.Fully)]
        private readonly ConfigurationMSALAuthenticationTemplate _template;
        [IntentManaged(Mode.Fully)]
        private readonly IApplication _application;

        [IntentManaged(Mode.Fully, Body = Mode.Fully)]
        public ConfigurationMSALAuthenticationDefaultDecorator(ConfigurationMSALAuthenticationTemplate template, IApplication application)
        {
            _template = template;
            _application = application;
        }

        public IEnumerable<string> DeclareUsings()
        {
            return new[]
            {
                "Microsoft.AspNetCore.Authentication",
                "Microsoft.AspNetCore.Authentication.JwtBearer",
                "Microsoft.AspNetCore.Authorization",
                "Microsoft.Identity.Web"
            };
        }

        public override string GetConfigurationCode()
        {
            return @"
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection(""AzureAd""))
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddMicrosoftGraph(builder.Configuration.GetSection(""MicrosoftGraph""))
                .AddInMemoryTokenCaches();
            ;";
        }
    }
}