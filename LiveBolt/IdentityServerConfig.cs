using System.Collections.Generic;
using IdentityServer4.Models;

namespace LiveBolt
{
    public class IdentityServerConfig
    {
        // Scopes define the API resources in the system
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("liveboltapi", "LiveBolt API")
            };
        }

        // Clients want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "client",
                    ClientName = "LiveBolt Client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    
                    RequireConsent = false,

                    AllowedScopes =
                    {
                        "liveboltapi",
                    },

                    AllowOfflineAccess = true
                }
            };
        }
    }
}