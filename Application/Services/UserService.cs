using Domain.Entities;
using Infrastructure.Repositories;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task Signup(User user)
        {
            user.Balance = 0; // Initial balance
            await _userRepository.AddUser(user);
        }

        public async Task<User> Authenticate(string username, string password, string ipAddress, string device)
        {
            var user = await _userRepository.GetUserByUsername(username);
            if (user != null && user.Password == password)
            {
                // Check if first time login
                if (user.Balance == 0)
                {
                    user.Balance = 5; // Add 5 GBP balance as gift
                    await _userRepository.UpdateUser(user);
                }

                // Save login details to the database (implement as needed)

                return user;
            }

            return null;
        }

        public async Task<decimal> GetBalance(string username)
        {
            var user = await _userRepository.GetUserByUsername(username);
            return user?.Balance ?? 0;
        }
    }
}
