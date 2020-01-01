using DatingApp.API.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Data;




namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;

        public AuthRepository(DataContext context)
        {
            _context = context;

        }
        public async Task<User> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);

            if(user == null)
            return null;

            byte[] hashArr = user.PasswardHash;
            byte[] saltArr = user.PasswordSalt;

            if(!VerifyPasswordHash(password, hashArr , saltArr))
             return null;

             return user;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
              {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                for(int i=0;i<computedHash.Length;i++)
                {
                    if(computedHash[i] != passwordHash[i]) return false;
                }
              }

              return true;
        }
        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash,passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswardHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.Users.AddAsync(user);

            await _context.SaveChangesAsync();

            return user;
        }

          public  void CreatePasswordHash(string password, out byte[] passwordHash, out  byte[] passwordSalt)
          {
              using(var hmac = new System.Security.Cryptography.HMACSHA512())
              {
                byte[] key = hmac.Key;
                passwordSalt = key;
                byte[] v = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                passwordHash = v;
              }
          }

        public async Task<bool> UserExits(string username)
        {
            if(await _context.Users.AnyAsync(x => x.Username == username))
            return true;

            return false;
        }
    }
}