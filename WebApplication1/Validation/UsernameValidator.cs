using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace WebApplication1.Validation
{
    public class UsernameValidator<TUser> : IUserValidator<TUser>
      where TUser : IdentityUser
    {
        public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager,
                                                  TUser user)
        {
            if (user.UserName == null)
            {
                return Task.FromResult(IdentityResult.Success);
            }
            else
            {
                var userExist = manager.FindByNameAsync(user.UserName);
                if (userExist.Result == null)
                {
                    return Task.FromResult(IdentityResult.Success);
                }
            }

            return Task.FromResult(
                     IdentityResult.Failed(new IdentityError
                     {
                         Code = "400",
                         Description = "UserName is not unique 123."
                     }));
        }
    }
}
