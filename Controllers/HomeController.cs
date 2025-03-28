using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using CraftingProject.Models;
using CraftingProject.Repository;
public class HomeController : Controller
{
        private readonly CraftRepository _craftRepository;
        private readonly IConfiguration _configuration;


    public HomeController(CraftRepository craftRepository, IConfiguration configuration)
        {
            _craftRepository = craftRepository;
            _configuration = configuration;
    }

    public IActionResult Dashboard()
    {
        List<CraftModel1> crafts = new List<CraftModel1>();

        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand("SELECT CraftName, ImageUrl FROM Crafts", conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        crafts.Add(new CraftModel1
                        {
                            CraftName = reader["CraftName"].ToString(),
                            FilePath = reader["ImageUrl"].ToString()
                        });
                    }
                }
            }
        }

        return View(crafts);
    }

    [HttpPost]
    public IActionResult UploadIdea(IFormFile craftIdeaFile, string craftName)
    {
        if (craftIdeaFile == null || craftIdeaFile.Length == 0 || string.IsNullOrWhiteSpace(craftName))
        {
            TempData["Message"] = "Please select a file and enter a craft name.";
            return RedirectToAction("Dashboard");
        }

        try
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + craftIdeaFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                craftIdeaFile.CopyTo(stream);
            }

            SaveFileToDatabase(craftName, uniqueFileName, "/uploads/" + uniqueFileName);

            TempData["Message"] = "File uploaded successfully!";
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Upload failed: " + ex.Message;
        }

        return RedirectToAction("Dashboard");
    }

    private void SaveFileToDatabase(string craftName, string fileName, string filePath)
    {
        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand("sp_SaveCraftUpload", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CraftName", craftName);
                cmd.Parameters.AddWithValue("@FilePath", filePath);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public IActionResult CraftDetails(string craftName)
    {
        if (string.IsNullOrEmpty(craftName))
        {
            return RedirectToAction("Dashboard"); // Redirect if no craft is selected
        }

        ViewBag.CraftName = craftName;
        return View();
    }

    [HttpPost]
    public IActionResult PlaceOrder([FromBody] Order order)
    {
        if (order == null || string.IsNullOrEmpty(order.Name) || string.IsNullOrEmpty(order.Mobile))
        {
            return Json(new { success = false, message = "Invalid order data" });
        }

        bool result = _craftRepository.PlaceOrder(order);

        return Json(new { success = result });
    }

}
public class CraftCategory
{
    public int Id { get; set; }
    public string CraftName { get; set; }
    public string CraftUrl { get; set; }
}

public class CraftModel1
{
    public string CraftName { get; set; }
    public string FilePath { get; set; }
    public int Id { get; internal set; }
    public string? Name { get; internal set; }
}

