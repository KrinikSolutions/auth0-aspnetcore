﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Auth0.AspNetCore.Mvc.TagHelpers.Lock.OpenIdConnect
{
    [HtmlTargetElement("lock-openidconnect-configuration", ParentTag = "lock")]
    public class LockOpenIdConnectConfigurationTagHelper : TagHelper
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string AuthenticationScheme { get; set; }

        public LockOpenIdConnectConfigurationTagHelper(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
        {
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var options = _serviceProvider.GetServices<OpenIdConnectOptions>().FirstOrDefault(o => o.AuthenticationScheme == AuthenticationScheme);

            if (options != null)
            {
                var lockContext = (LockContext)context.Items[typeof(LockContext)];

                // Set the options
                lockContext.ClientId = options.ClientId;

                // retrieve the domain from the authority
                Uri authorityUri;
                if (Uri.TryCreate(options.Authority, UriKind.Absolute, out authorityUri))
                {
                    lockContext.Domain = authorityUri.Host;
                }

                // Set the redirect
                lockContext.CallbackUrl = BuildRedirectUri(_httpContextAccessor.HttpContext.Request,
                    options.CallbackPath);
            }

            output.SuppressOutput();
            return Task.FromResult(0);
        }

        protected string BuildRedirectUri(HttpRequest httpRequest, PathString redirectPath)
        {
            return httpRequest.Scheme + "://" + httpRequest.Host + redirectPath;
        }
    }
}