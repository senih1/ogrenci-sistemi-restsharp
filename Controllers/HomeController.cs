using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ogrenci_sistemi_restsharp.Models;
using RestSharp;
using System.Diagnostics;
using System.Reflection;
using static ogrenci_sistemi_restsharp.Models.LoginModel;

namespace ogrenci_sistemi_restsharp.Controllers
{
    public class HomeController : Controller
    {
        public bool CheckLogin()
        {
            if (HttpContext.Session.GetString("Username") != null)
            {
                return true;
            }
            return false;
        }
        public void GetSessions()
        {
            ViewData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetString("UserId");
            ViewData["RoleId"] = HttpContext.Session.GetString("RoleId");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
        }
        public IActionResult Index()
        {
            if (!CheckLogin())
            {
                TempData["Alert"] = "Önce bir hesaba giriş yapmalısın.";
                TempData["AlertCss"] = "alert-danger";
                return RedirectToAction("Login", "Login");
            }

            return RedirectToAction("getstudents");
        }

        [Route("/form")]
        public IActionResult AddStudent()
        {
            GetSessions();
            if (!CheckLogin())
            {
                TempData["Alert"] = "Önce bir hesaba giriş yapmalısın.";
                TempData["AlertCss"] = "alert-danger";
                return RedirectToAction("Login", "Login");
            }

            return View();
        }

        [Route("/api")]
        [HttpPost]
        public IActionResult AddStudent(Student model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Alert"] = $"Ekleme işlemi başarısız.";
                TempData["AlertCss"] = "alert-danger";
                return RedirectToAction("GetStudents", "Home");
            }

            var client = new RestClient("https://ogrenci.senihay.com/");
            var request = new RestRequest("api", Method.Post);

            request.AddJsonBody(model);
            var response = client.Post<Student>(request);
            
            if (response != null)
            {
                TempData["Alert"] = $"Öğrenci başarı ile eklendi!";
                TempData["AlertCss"] = "alert-success";
                return RedirectToAction("GetStudents", "Home");
            }

            TempData["Alert"] = $"Ekleme işlemi başarısız.";
            TempData["AlertCss"] = "alert-danger";
            return RedirectToAction("GetStudents", "Home");
        }

        [Route("/list")]
        [HttpGet]
        public IActionResult GetStudents()
        {
            if (!CheckLogin())
            {
                TempData["Alert"] = "Önce bir hesaba giriş yapmalısın.";
                TempData["AlertCss"] = "alert-danger";
                return RedirectToAction("Login", "Login");
            }

            GetSessions();

            var client = new RestClient("https://ogrenci.senihay.com/");
            var request = new RestRequest("api");
            var response = client.Get(request);
            var students = JsonConvert.DeserializeObject<List<Student>>(response.Content);

            return View(students);
        }

        [Route("/api/{id}")]
        [HttpPost]
        public IActionResult DeleteStudent(int id)
        {
            var client = new RestClient("https://ogrenci.senihay.com");
            var request = new RestRequest($"api/{id}", Method.Delete);
            var response = client.Execute(request);

            if(response.IsSuccessful && response != null)
            {
                TempData["Alert"] = $"Silme işlemi başarılı. {id} numaralı öğrenci silindi.";
                TempData["AlertCss"] = "alert-success";
                return RedirectToAction("GetStudents", "Home");
            }
            TempData["Alert"] = $"Silme işlemi başarısız.";
            TempData["AlertCss"] = "alert-danger";
            return RedirectToAction("GetStudents", "Home");
        }

        [Route("/edit/{id}")]
        [HttpGet]
        public IActionResult EditStudent(int id)
        {
            if (!CheckLogin())
            {
                TempData["Alert"] = "Önce bir hesaba giriş yapmalısın.";
                TempData["AlertCss"] = "alert-danger";
                return RedirectToAction("Login", "Login");
            }

            GetSessions();

            var client = new RestClient("https://ogrenci.senihay.com/");
            var request = new RestRequest($"api/{id}");
            var response = client.Get(request);
            var student = JsonConvert.DeserializeObject<Student>(response.Content);

            return View(student);
        }

        [Route("/edit")]
        [HttpPost]
        public IActionResult EditStudent(Student model)
        {
            var client = new RestClient("https://ogrenci.senihay.com/");
            var request = new RestRequest($"/api/{model.Id}", Method.Patch);
            request.AddJsonBody(model);
            var response = client.Execute(request);

            if (response.IsSuccessful && response != null)
            {
                TempData["Alert"] = $"Güncelleme işlemi başarılı. {model.Id} numaralı öğrenci güncellendi.";
                TempData["AlertCss"] = "alert-success";
                return RedirectToAction("GetStudents", "Home");
            }

            TempData["Alert"] = $"Güncelleme işlemi başarısız.";
            TempData["AlertCss"] = "alert-danger";
            return RedirectToAction("GetStudents", "Home");
        }
    }
}
