using ITTPWebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

//using static ITTPWebAPI.Models.User;

namespace ITTPWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserContext _context;

        public UsersController(UserContext context)
        {
            _context = context;
        }

        // CRUD-методы контроллера UsersController
        //
        // 1) Создание пользователя по логину, паролю, имени, полу и дате рождения
        // + указание будет ли пользователь админом
        // (Доступно Админам)
        [HttpPost("Create")]
        public async Task<ActionResult<User>> CreateUser(string login, string password, User user)
        {
            User admin = await _context.GetAdmin(login, password);

            if (admin == null)
            {
                return NotFound("Админ не найден!");
            }
            else if (admin.Password != password)
            {
                return BadRequest("Пароль неверный!");
            }

            User newUser = new User()
            {
                Guid = Guid.NewGuid(),
                Name = user.Name,
                Login = user.Login,
                Password = user.Password,
                Gender = user.Gender,
                Birthday = user.Birthday,
                Admin = user.Admin,
                CreatedOn = DateTime.Now,
                CreatedBy = admin.Login,
                ModifiedOn = DateTime.Now,
                ModifiedBy = admin.Login
            };

            await _context.Users.AddAsync(newUser);
            _context.SaveChanges();

            return Ok(newUser);
        }

        // 2) Изменение имени, пола или даты рождения пользователя
        // Может менять Администратор, либо лично пользователь, если он активен
        // (отсутствует RevokedOn)
        [HttpPut("UpdateUserInfo/NameGenderBirthday")]
        public async Task<ActionResult> UpdateUserInfo(
            string login, 
            string password,
            NewInfoNameGenderBirthday nameGenderBirthday, 
            string userLogin)
        {
            User user;
            
            if (userLogin == login)
            {
                user = await _context.GetActiveUser(login);

                if (user != null && user.Password != password)
                {
                    return BadRequest("Пароль неверный!");
                }
            }
            else
            {
                User admin = await _context.GetAdmin(login, password);

                if (admin == null)
                {
                    return NotFound("Админ не найден!");
                }
                else if (admin.Password != password)
                {
                    return BadRequest("Пароль неверный!");
                }

                user = await _context.GetUser(userLogin);
            }

            if (user == null)
            {
                return NotFound("Пользователь не найден!");
            }

            User updatedUser = await _context.Users.FindAsync(user.Guid);
            
            updatedUser.Name = nameGenderBirthday.Name;
            updatedUser.Gender = nameGenderBirthday.Gender;
            updatedUser.Birthday = nameGenderBirthday.Birthday;
            updatedUser.ModifiedOn = DateTime.Now;
            updatedUser.ModifiedBy = login;

            _context.Users.Update(updatedUser);
            _context.SaveChanges();

            return Ok();
        }

        // 3) Изменение пароля
        // Пароль может менять либо Администратор, либо лично пользователь, если он активен
        // (отсутствует RevokedOn)
        [HttpPut("UpdateUserInfo/Password")]
        public async Task<ActionResult> UpdateUserInfo(
            string login, 
            string password, 
            NewInfoPassword newPassword, 
            string userLogin)
        {
            User user;

            if (userLogin == login)
            {
                user = await _context.GetActiveUser(login);

                if (user != null && user.Password != password)
                {
                    return BadRequest("Пароль неверный!");
                }
            }
            else
            {
                User admin = await _context.GetAdmin(login, password);

                if (admin == null)
                {
                    return NotFound("Админ не найден!");
                }
                else if (admin.Password != password)
                {
                    return BadRequest("Пароль неверный!");
                }

                user = await _context.GetUser(userLogin);
            }

            if (user == null)
            {
                return NotFound("Пользователь не найден!");
            }

            User updatedUser = await _context.Users.FindAsync(user.Guid);
            updatedUser.Password = newPassword.Password;
            updatedUser.ModifiedOn = DateTime.Now;
            updatedUser.ModifiedBy = login;

            _context.Users.Update(updatedUser);
            _context.SaveChanges();

            return Ok();
        }

        // 4) Изменение логина
        // Логин может менять либо Администратор, либо лично пользователь, если он активен
        // (отсутствует RevokedOn)
        // Логин должен оставаться уникальным
        [HttpPut("UpdateUserInfo/Login")]
        public async Task<ActionResult> UpdateUserInfo(
            string login,
            string password, 
            NewInfoLogin newLogin, 
            string userLogin)
        {
            User user;
            if (userLogin == login)
            {
                user = await _context.GetActiveUser(login);

                if (user != null && user.Password != password)
                {
                    return BadRequest("Пароль неверный!");
                }
            }
            else
            {
                User admin = await _context.GetAdmin(login, password);

                if (admin == null)
                {
                    return NotFound("Админ не найден!");
                }
                else if (admin.Password != password)
                {
                    return BadRequest("Пароль неверный!");
                }

                user = await _context.GetUser(userLogin);
            }

            if (user == null)
            {
                return NotFound("Пользователь не найден!");
            }

            User updatedUser = await _context.Users.FindAsync(user.Guid);

            User checkUser = await _context.GetActiveUser(newLogin.Login);

            if (checkUser != null)
            {
                return BadRequest("Пользователь с таким логином уже существует!");
            }
            
            updatedUser.Login = newLogin.Login;
            updatedUser.ModifiedOn = DateTime.Now;
            updatedUser.ModifiedBy = user.Login;

            _context.Users.Update(updatedUser);
            _context.SaveChanges();

            return Ok();
        }

        // 5) Запрос списка всех активных (отсутствует RevokedOn) пользователей, список отсортирован по CreatedOn
        // Доступно Админам
        [HttpGet("RequestUser/Active")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(string login, string password)
        {
            User admin = await _context.GetAdmin(login, password);

            if (admin == null)
            {
                return NotFound("Админ не найден!");
            }
            else if (admin.Password != password)
            {
                return BadRequest("Пароль неверный!");
            }

            return await _context.Users.Where(user => user.RevokedBy == null)
                .OrderBy(created => created.CreatedOn)
                .ToListAsync();
        }

        // 6) Запрос пользователя по логину, в списке долны быть имя, пол и дата рождения статус активный или нет
        // Доступно Админам
        [HttpGet("RequestUser/Login")]
        public async Task<ActionResult<IEnumerable<User>>> GetUser(string login, string password, string userLogin)
        {
            User admin = await _context.GetAdmin(login, password);
            
            if (admin == null)
            {
                return NotFound("Админ не найден!");
            }
            else if (admin.Password != password)
            {
                return BadRequest("Пароль неверный!");
            }

            User user = await _context.Users.FirstOrDefaultAsync(user => user.Login == userLogin);
            
            if (user == null)
            {
                return BadRequest("Пользователь не найден!");
            }
            
            return Ok(new { Name = user.Name, Gender = user.Gender, 
                RevokedOn = user.RevokedOn });
        }

        // 7) Запрос пользователя по логину и паролю
        // Доступно только самому пользователю, если он активен
        // (отсутствует RevokedOn)
        [HttpGet("RequestUser/LoginPassword")]
        public async Task<ActionResult<IEnumerable<User>>> GetUser(string login, string password)
        {
            User user = await _context.GetUser(login);

            if (user == null)
            {
                return NotFound("Пользователь не найден!");
            }
            else if (user.Password != password)
            {
                return BadRequest("Пароль неверный!");
            }
            else if (user.Admin == true)
            {
                return BadRequest("Действие доступно только пользователю!");
            } 
            else if (user.RevokedBy == null)
            {
                return NotFound("Запись удалена!");
            }

            return Ok(user);
        }

        // 8) Запрос всех пользователей старше определённого возраста
        // Доступно Админам
        [HttpGet("RequestUser/OlderAge")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(string login, string password, int age)
        {
            User admin = await _context.GetAdmin(login, password);

            if (admin == null)
            {
                return NotFound("Админ не найден!");
            }
            else if (admin.Password != password)
            {
                return BadRequest("Пароль неверный!");
            }

            var birthday = new DateTime(year: (DateTime.Today.Year - age), 
                month: DateTime.Today.Month, day: DateTime.Today.Day);

            return await _context.Users.Where(users => users.Birthday.HasValue)
                .Where(user => user.Birthday < birthday)
                .ToListAsync();
        }

        // 9) Удаление пользователя по логину (полное и мягкое) 
        // Доступно админам
        [HttpDelete("DeleteUser")] 
        public async Task<ActionResult> DeleteUser(string login, string password, string userLogin, string softOrComplete)
        {
            User admin = await _context.GetAdmin(login, password);

            if (admin == null)
            {
                return NotFound("Админ не найден!");
            }
            else if (admin.Password != password)
            {
                return BadRequest("Пароль неверный!");
            }

            User user = await _context.GetUser(userLogin);

            if (user == null)
            {
                return NotFound("Пользователь не найден!");
            }

            if (softOrComplete == "Soft")
            {
                var deletedUser = await _context.Users.FindAsync(user.Guid);
                deletedUser.RevokedOn = DateTime.Now;
                deletedUser.RevokedBy = login;
                _context.SaveChanges();
                return Ok(deletedUser);
            }

            if (softOrComplete == "Complete")
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                return Ok();
            }

            return NoContent();
        }
 
        // 10) Восстановление пользователя
        // Доступно админам
        [HttpDelete("RecoveryUser")]
        public async Task<ActionResult> RecoveryUser(string login, string password, string userLogin)
        {
            User admin = await _context.GetAdmin(login, password);

            if (admin == null)
            {
                return NotFound("Админ не найден!");
            }
            else if (admin.Password != password)
            {
                return BadRequest("Пароль неверный!");
            }

            User recoveryUser = await _context.GetActiveUser(userLogin);

            if (recoveryUser == null)
            {
                return NotFound("Пользователь не найден или не был удален!");
            }

            recoveryUser.RevokedOn = default;
            recoveryUser.RevokedBy = null;
            _context.SaveChanges();
            
            return Ok(recoveryUser);
        }


        public class NewInfoNameGenderBirthday
        {
            [Required]
            [RegularExpression(@"^[a-zA-Z\d]+$")]
            public string Name { get; set; }
            
            public int Gender { get; set; }

            public DateTime? Birthday { get; set; }
        }

        public class NewInfoPassword
        {
            [Required]
            [RegularExpression(@"^[a-zA-Z\d]+$")]
            public string Password { get; set; }
        }

        public class NewInfoLogin
        {
            [Required]
            [RegularExpression(@"^[a-zA-Z\d]+$")]
            public string Login { get; set; }
        }
    }
}
