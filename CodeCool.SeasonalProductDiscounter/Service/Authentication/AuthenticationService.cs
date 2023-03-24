using CodeCool.SeasonalProductDiscounter.Model.Users;
using CodeCool.SeasonalProductDiscounter.Service.Users;

namespace CodeCool.SeasonalProductDiscounter.Service.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;

    public AuthenticationService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }


    public bool Authenticate(User user)
    {
        var users = _userRepository.GetAll();
        foreach (var listUser in users)
        {
            if (listUser.UserName == user.UserName && listUser.Password == user.Password) return true;
        }
        return false;
    }
}
