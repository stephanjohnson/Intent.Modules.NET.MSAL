using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Intent.Engine;
using Intent.Modules.AspNetCore.Events;
using Intent.Modules.AspNetCore.Swashbuckle.Interop.MSAL.Events;
using Intent.Modules.VisualStudio.Projects.Templates.CoreWeb.AppSettings;
using Intent.RoslynWeaver.Attributes;
using Newtonsoft.Json.Linq;

[assembly: DefaultIntentManaged(Mode.Merge)]
[assembly: IntentTemplate("Intent.ModuleBuilder.Templates.TemplateDecorator", Version = "1.0")]

namespace Intent.Modules.AspNetCore.Swashbuckle.Interop.MSAL.Decorators
{
    [IntentManaged(Mode.Merge)]
    public class InteropAppSettingsDecorator : AppSettingsDecorator
    {
        [IntentManaged(Mode.Fully)]
        public const string DecoratorId = "Intent.AspNetCore.Swashbuckle.Interop.MSAL.InteropAppSettingsDecorator";

        [IntentManaged(Mode.Fully)]
        private readonly AppSettingsTemplate _template;
        [IntentManaged(Mode.Fully)]
        private readonly IApplication _application;
        private readonly List<SwaggerOAuth2SchemeEvent> _swaggerSchemes;

        [IntentManaged(Mode.Fully, Body = Mode.Fully)]
        public InteropAppSettingsDecorator(AppSettingsTemplate template, IApplication application)
        {
            _template = template;
            _application = application;
            Priority = 100;
            _application.EventDispatcher.Subscribe<SwaggerOAuth2SchemeEvent>(Handle);
            _swaggerSchemes = new List<SwaggerOAuth2SchemeEvent>();
        }

        private void Handle(SwaggerOAuth2SchemeEvent @event)
        {
            _swaggerSchemes.Add(@event);
        }

        public override void UpdateSettings(AppSettingsEditor appSettings)
        {
            var settings = appSettings.GetProperty("Swashbuckle");

            dynamic securitySchemes = settings.SwaggerGen.SwaggerGeneratorOptions.SecuritySchemes;
            if (securitySchemes == null)
            {
                securitySchemes = JObject.FromObject(new
                {
                    oauth2 = new
                    {
                        Type = "OAuth2",
                        Flows = new Dictionary<string, object>()
                    }
                });
                settings.SwaggerGen.SwaggerGeneratorOptions.SecuritySchemes = securitySchemes;
            }

            foreach (var scheme in _swaggerSchemes)
            {
                if (securitySchemes.oauth2.Flows[scheme.SchemeName] == null)
                {
                    securitySchemes.oauth2.Flows[scheme.SchemeName] = JObject.FromObject(new
                    {
                        AuthorizationUrl = scheme.AuthorizationUrl,
                        RefreshUrl = scheme.RefreshUrl,
                        TokenUrl = scheme.TokenUrl,
                        scheme.Scopes
                    });
                }
            }

            dynamic oauthConfigObj = settings.SwaggerUI.OAuthConfigObject;
            if (oauthConfigObj == null)
            {
                oauthConfigObj = JObject.FromObject(new
                {
                    ClientId = _swaggerSchemes.OrderByDescending(k => k.Priority).FirstOrDefault()?.ClientId ?? string.Empty,
                    ClientSecret = (string)null,
                    AppName = _template.OutputTarget.Application.Name,
                    UsePkceWithAuthorizationCodeGrant = true,
                    ScopeSeparator = " "
                });
                settings.SwaggerUI.OAuthConfigObject = oauthConfigObj;
            }

            if (_swaggerSchemes.Select(s => s.ClientId).Contains((string)oauthConfigObj.ClientId)
                || oauthConfigObj.ClientId == null
                || oauthConfigObj.ClientId == string.Empty)
            {
                oauthConfigObj.ClientId = _swaggerSchemes.OrderByDescending(k => k.Priority).FirstOrDefault()?.ClientId ?? string.Empty;
            }
        }
    }
}