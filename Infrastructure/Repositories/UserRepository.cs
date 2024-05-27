using Dapper;
using Domain.Entities;
using Infrastructure.Data;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserRepository
    {
        private readonly DatabaseContext _context;

        public UserRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task AddUser(User user)
        {
            var query = "INSERT INTO Users (Username, Password, FirstName, LastName, Device, IpAddress, Balance) VALUES (@Username, @Password, @FirstName, @LastName, @Device, @IpAddress, @Balance)";
            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, user);
            }
        }

        public async Task<User> GetUserByUsername(string username)
        {
            var query = "SELECT * FROM Users WHERE Username = @Username";
            using (var connection = _context.CreateConnection())
            {
                return await connection.QuerySingleOrDefaultAsync<User>(query, new { Username = username });
            }
        }

        public async Task UpdateUser(User user)
        {
            var query = "UPDATE Users SET Balance = @Balance WHERE Id = @Id";
            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, user);
            }
        }
    }
}
