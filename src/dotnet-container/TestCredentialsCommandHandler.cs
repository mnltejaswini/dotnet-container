﻿using dotnet_container.Options;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace dotnet_container
{
    internal class TestCredentialsCommandHandler
    {
        public static Command CreateCommand() => new Command(
            name: "test-credentials",
            description: "Test credentials used to connect to container registry",
            symbols: new Option[]
            {
                RegistryOption.Create(),
                UsernameOption.Create(),
                PasswordOption.Create(),
            },
            handler: CommandHandler.Create<IConsole, string?, string?, string?>(TestCredentialsAsync),
            isHidden: true
            );
        private static async Task TestCredentialsAsync(IConsole console, string? registry, string? username, string? password)
        {
            try
            {
                RegistryOption.EnsureNotNullorMalformed(registry);
                UsernameOption.EnsureNotNull(ref username);
                PasswordOption.EnsureNotNull(ref password);
            }
            catch (ArgumentException e)
            {
                console.Error.WriteLine($"Push failed due to bad/missing argument:\t{e.ParamName}");
                return;
            }

            var requestPath = new UriBuilder("https", registry, 443, "v2/").Uri;
            var request = new HttpRequestMessage(HttpMethod.Get, requestPath);
            request.AddBasicAuthorizationHeader(username, password);

            var statusCode = (await new HttpClient().SendAsync(request)).StatusCode;

            if (statusCode == HttpStatusCode.Unauthorized)
            {
                console.Out.WriteLine("Invalid credentials");
            }
            else if (statusCode == HttpStatusCode.OK)
            {
                console.Out.WriteLine("Credentials are valid");
            }
            else
            {
                throw new ApplicationException($"Credential test failed with {statusCode}");
            }
        }
    }
}