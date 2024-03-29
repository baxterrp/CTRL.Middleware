﻿using CTRL.Authentication.Configuration;
using CTRL.Authentication.Constants;
using CTRL.Authentication.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace CTRL.Authentication.Implementation
{
    public class ApiAuthenticationHandler : AuthenticationHandler<ApiAuthenticationOptions>
    {
        private readonly IAuthenticationTokenManager _authenticationTokenManager;
        private const string _cookieName = "IdentityCookie";

        public ApiAuthenticationHandler(
            IAuthenticationTokenManager authenticationTokenManager,
            IOptionsMonitor<ApiAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _authenticationTokenManager = authenticationTokenManager ?? throw new ArgumentNullException(nameof(authenticationTokenManager));
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                if (!Request.Path.ToUriComponent().StartsWith("/api/authentication"))
                {
                    if (!Request.Headers.ContainsKey(_cookieName))
                    {
                        return Task.FromResult(AuthenticateResult.Fail(Messages.Unauthorized));
                    }

                    string stringCookie = Request.Headers[_cookieName];
                    var identityCookie = JsonConvert.DeserializeObject<IdentityCookie>(stringCookie);

                    IPrincipal principal = _authenticationTokenManager.ValidateToken(identityCookie.Token);
                    if (principal is null)
                    {
                        return Task.FromResult(AuthenticateResult.Fail(Messages.Unauthorized));
                    }

                    var parsedToken = new JwtSecurityTokenHandler().ReadJwtToken(identityCookie.Token);
                    var expectedUserName = parsedToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name && c.Value == identityCookie.UserName)?.Value ?? string.Empty;

                    if (expectedUserName != identityCookie.UserName)
                    {
                        return Task.FromResult(AuthenticateResult.Fail(Messages.Unauthorized));
                    }

                    var ticket = new AuthenticationTicket(principal as ClaimsPrincipal, "ApiAuthentication");

                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }

                return Task.FromResult(AuthenticateResult.NoResult());
            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail(Messages.Unauthorized));
            }
        }
    }
}
