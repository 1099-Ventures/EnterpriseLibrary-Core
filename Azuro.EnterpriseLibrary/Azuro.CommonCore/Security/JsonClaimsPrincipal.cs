using System.Collections.Generic;
using System.Security.Claims;

namespace Azuro.Common.Security
{
	public class JsonClaimsPrincipal
	{
		public string AuthenticationType { get; set; }
		public string NameClaimType { get; set; }
		public string RoleClaimType { get; set; }
		public List<JsonClaim> Claims { get; set; } = new List<JsonClaim>();

		public JsonClaimsPrincipal() { }

		public JsonClaimsPrincipal(string authenticationType, string nameClaimType, string roleClaimType)
		{
			AuthenticationType = authenticationType;
			NameClaimType = nameClaimType;
			RoleClaimType = roleClaimType;
		}

		public JsonClaimsPrincipal(ClaimsPrincipal principal)
		{
			AuthenticationType = ((ClaimsIdentity)principal.Identity).AuthenticationType;
			NameClaimType = ((ClaimsIdentity)principal.Identity).NameClaimType;
			RoleClaimType = ((ClaimsIdentity)principal.Identity).RoleClaimType;

			foreach (var c in ((ClaimsIdentity)principal.Identity).Claims)
			{
				Claims.Add(new JsonClaim
				{
					Type = c.Type,
					Value = c.Value,
					ValueType = c.ValueType,
					Issuer = c.Issuer,
					OriginalIssuer = c.OriginalIssuer,
				});
			}
		}

		public ClaimsPrincipal ToClaimsPrincipal()
		{
			var claimsPrincipal = new ClaimsPrincipal();

			var identity = new ClaimsIdentity(AuthenticationType, NameClaimType, RoleClaimType);
			claimsPrincipal.AddIdentity(identity);

			foreach (var c in Claims)
			{
				var claim = new Claim(c.Type, c.Value, c.ValueType, c.Issuer, c.OriginalIssuer);
				identity.AddClaim(claim);
			}

			return claimsPrincipal;
		}
	}
}
