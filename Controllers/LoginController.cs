using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ogrenci_sistemi_restsharp.Codes.Models;
using ogrenci_sistemi_restsharp.Models;
using Dapper;
using Microsoft.Win32;
using static ogrenci_sistemi_restsharp.Models.LoginModel;
using RestSharp;

namespace ogrenci_sistemi_restsharp.Controllers
{
    public class LoginController : Controller
    {
        string connectionString = "Server=X;Initial Catalog=X;User Id =X; Password =X;TrustServerCertificate=X";
        public bool CheckLogin()
        {
            if (HttpContext.Session.GetString("Username") != null)
            {
                return true;
            }
            return false;
        }
        public bool VerifyCaptcha(string captchaToken)
        {
            var client = new RestClient("https://www.google.com/recaptcha");
            var request = new RestRequest("api/siteverify", Method.Post);
            request.AddParameter("secret", "6Ld3ZDgqAAAAAEcLUWVpUt5_8foDJo-hEnJkZ54R");
            request.AddParameter("response", captchaToken);

            var response = client.Execute<CaptchaResponse>(request);

            if (response.Data.Success && response.Data.Score > 0.6)
            {
                return true;
            }
            return false;
        }

        [Route("register")]
        [HttpPost]
        public IActionResult Register(RegisterModel model)
        {
            var captchaToken = Request.Form["g-recaptcha-response"];

            if (!VerifyCaptcha(captchaToken))
            {
                TempData["Alert"] = "Captcha doğrulaması hatalı!";
                TempData["AlertCss"] = "alert-danger";
                return RedirectToAction("Login", "Login");
            }

            if (!ModelState.IsValid)
            {
                TempData["AlertCss"] = "alert-danger";
                TempData["Alert"] = "Eksik form bilgisi.";
                return RedirectToAction("Login", model);
            }

            if (model.Password != model.PasswordCheck)
            {
                TempData["AlertCss"] = "alert-danger";
                TempData["Alert"] = "Sifre dogrulamasi hatali.";
                return View("Login", model);
            }

            model.Password = Helper.Hash(model.Password);

            using var connection = new SqlConnection(connectionString);
            var sql = "INSERT INTO ogrenciUsers (username, email, password, createdDate) VALUES (@Username, @Email, @Password, @CreatedDate)";

            var data = new
            {
                model.Username,
                model.Email,
                model.Password,
                CreatedDate = DateTime.Now,
            };

            var rowsAffected = connection.Execute(sql, data);

            TempData["AlertCss"] = "alert-success";
            TempData["Alert"] = "Kullanıcı kayıdı başarı ile oluşturuldu!";
            return RedirectToAction("Login");
        }

        [Route("login")]
        public IActionResult Login()
        {
            if (CheckLogin())
            {
                TempData["Alert"] = "Zaten bir hesaba giriş yapılmış.";
                TempData["AlertCss"] = "alert-success";
                return RedirectToAction("Index", "Home");
            }

            ViewData["Username"] = HttpContext.Session.GetString("Username");
            return View(new RegisterModel());
        }

        [Route("login")]
        [HttpPost]
        public IActionResult Login(User model)
        {
            var captchaToken = Request.Form["g-recaptcha-response"];

            if (!VerifyCaptcha(captchaToken))
            {
                TempData["Alert"] = "Captcha doğrulaması hatalı!";
                TempData["AlertCss"] = "alert-danger";
                return RedirectToAction("Login", "Login");
            }

            var inputPasswordHash = Helper.Hash(model.Password);

            using var connection = new SqlConnection(connectionString);

            var sql = "SELECT * FROM ogrenciUsers WHERE Username = @Username AND Password = @Password";
            var user = connection.QuerySingleOrDefault<User>(sql, new { model.Username, Password = inputPasswordHash });

            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserId", user.Id.ToString());

                TempData["Alert"] = $"Giriş başarılı. Hoşgeldin {user.Username}";
                TempData["AlertCss"] = "alert-success";
                return RedirectToAction("GetStudents","Home");
            }
            else
            {
                TempData["Alert"] = "Kullanıcı adı veya şifre hatalı.";
                TempData["AlertCss"] = "alert-danger";
                return RedirectToAction("Login");
            }
        }

        [Route("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Username");
            HttpContext.Session.Remove("RoleId");
            HttpContext.Session.Remove("Email");

            TempData["AlertCss"] = "alert-warning";
            TempData["Alert"] = "Hesaptan çıkış yapıldı.";

            return RedirectToAction("Login");
        }
    }
}
