using AmoElDiseno.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AmoElDiseno.Controllers
{
    public class UsersLoginController : Controller
    {
        private readonly AppDbContext _context;

        public UsersLoginController(AppDbContext context)
        {
            _context = context;
        }

        // Acción para mostrar la vista de login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Acción para manejar el POST del login
        [HttpPost]
        public async Task<IActionResult> Login(string nombreUsuario, string contrasena)
        {
            // Buscar el usuario por su nombre de usuario
            var usuario = _context.Users!.FirstOrDefault(u => u.UserName == nombreUsuario);

            // Si el usuario no existe o la contraseña no coincide
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(contrasena, usuario.Password))
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos");
                ViewData["Error"] = "Usuario o Contraseña incorrectos!";
                return View();
            }

            // Crear los claims (información del usuario)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.UserName!),
                new Claim(ClaimTypes.Role, usuario.Role!) // Asignar el rol
            };

            // Crear la identidad del usuario
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Autenticar al usuario
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // Redirigir dependiendo del rol
            if (usuario.Role == "Admin")
            {
                return RedirectToAction("Index", "Orders"); // Redirige a una acción de administrador
            }
            else
            {
                return RedirectToAction("Index", "Orders"); // Redirige a una acción de usuario
            }
        }


        // Acción para cerrar sesión
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // Vista de acceso denegado
        public IActionResult AccesoDenegado()
        {
            return View();
        }


        //Use for create accounts manually with the code
        /*public IActionResult ManualCreateAdmin()
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("admin123");

            var adminUser = new User
            {
                UserName = "admin",
                Password = hashedPassword,
                Role = "Admin"
            };

            _context.Users!.Add(adminUser);
            _context.SaveChanges();

            return Ok("Usuario admin creado manualmente.");
        }*/

    }
}
