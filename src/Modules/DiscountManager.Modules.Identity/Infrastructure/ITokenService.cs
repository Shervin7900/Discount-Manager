using DiscountManager.Modules.Identity.Domain;

namespace DiscountManager.Modules.Identity.Infrastructure;

public interface ITokenService
{
    string GenerateJwtToken(ApplicationUser user);
}
