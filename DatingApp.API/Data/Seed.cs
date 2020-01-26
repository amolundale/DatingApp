using System.Collections.Generic;
using System.Linq;
using DatingApp.API.Models;
using Newtonsoft.Json;

namespace DatingApp.API.Data
{
    public class Seed
    {
        public static void SeedUsers(DataContext context)
        {
            if(!context.Users.Any())
            {
                var UserData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(UserData);
                foreach(var user in users)
                {
                    byte[] passwordHash, passwordSAlt;
                    CreatePasswordHash("password", out passwordHash, out passwordSAlt);
                    user.PasswardHash = passwordHash;
                    user.PasswordSalt = passwordSAlt;

                    user.Username = user.Username.ToLower();
                    context.Users.Add(user);
             
                }
                context.SaveChanges();
            }
        }

         public static  void CreatePasswordHash(string password, out byte[] passwordHash, out  byte[] passwordSalt)
          {
              using(var hmac = new System.Security.Cryptography.HMACSHA512())
              {
                byte[] key = hmac.Key;
                passwordSalt = key;
                byte[] v = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                passwordHash = v;
              }
          }

    }
}