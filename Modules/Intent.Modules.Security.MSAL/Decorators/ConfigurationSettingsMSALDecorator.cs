using Intent.Engine;
using Intent.Modules.VisualStudio.Projects.Templates.CoreWeb.AppSettings;
using Intent.RoslynWeaver.Attributes;

[assembly: DefaultIntentManaged(Mode.Fully)]
[assembly: IntentTemplate("Intent.ModuleBuilder.Templates.TemplateDecorator", Version = "1.0")]

namespace Intent.Modules.Security.MSAL.Decorators
{
    [IntentManaged(Mode.Merge)]
    public class ConfigurationSettingsMSALDecorator : AppSettingsDecorator
    {
        [IntentManaged(Mode.Fully)]
        public const string DecoratorId = "Intent.Security.MSAL.ConfigurationSettingsMSALDecorator";

        [IntentManaged(Mode.Fully)]
        private readonly AppSettingsTemplate _template;
        [IntentManaged(Mode.Fully)]
        private readonly IApplication _application;

        [IntentManaged(Mode.Fully, Body = Mode.Fully)]
        public ConfigurationSettingsMSALDecorator(AppSettingsTemplate template, IApplication application)
        {
            _template = template;
            _application = application;
        }

        public override void UpdateSettings(AppSettingsEditor appSettings)
        {
            appSettings.AddPropertyIfNotExists("AzureAd", new
            {
                Instance = "https://login.microsoftonline.com/",
                Domain = "",
                TenantId = "",
                ClientId = "",
                Audience = "",
                Scopes = "",
                CallbackPath = "/signin-oidc",
                SignedOutCallbackPath = "/signout-callback-oidc",
                ClientSecret = ""
            });
        }
    }
}