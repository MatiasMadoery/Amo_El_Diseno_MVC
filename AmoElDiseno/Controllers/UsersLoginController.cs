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
            var usuario = _context.User!.FirstOrDefault(u => u.UserName == nombreUsuario && u.Password == contrasena);
            if (usuario != null)
            {
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
                    return RedirectToAction("Index", "Home"); // Redirige a una acción de administrador
                }
                else
                {
                    return RedirectToAction("Index", "Home"); // Redirige a una acción de usuario
                }
            }
            else
            {
                // Si el login falla
                ModelState.AddModelError("", "Usuario o contraseña incorrectos");
                ViewData["Error"] = "Usuario o Contraseña incorrectos!";
            }           
            return View();
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
    }
   }
