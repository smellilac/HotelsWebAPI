public class UserRepository : IUserRepository
{
    private List<UserDto> _users => new()
    {
        new UserDto("Monica", "123"),
        new UserDto("John", "1234"),
        new UserDto("Nancy", "1235")
    };

    public UserDto GetUser(UserModel userModel) => 
        _users.SingleOrDefault (u =>
            string.Equals(u.UserName, userModel.UserName) &&
            string.Equals(u.Password, userModel.Password)) ??
            throw new Exception();
}