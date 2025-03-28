using CraftingProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

public class AdminController : Controller
{
    private readonly IConfiguration _configuration;

    public AdminController(IConfiguration configuration)
    {
        _configuration = configuration;
    }



    public IActionResult AdminDashboard()
    {
        var craftList = GetCraftList(); // Fetch craft list from DB

        if (craftList == null || !craftList.Any())
        {
            craftList = new List<CraftModel>(); // Avoid null reference errors
        }

        ViewBag.CraftList = craftList;

        // Ensure ViewBag.Data is initialized properly
        ViewBag.Data = new Dictionary<int, List<CraftDetail>>();

        return View();
    }




    [HttpPost]
    public IActionResult AdminDashboard(string selectedCraft)
    {
        ViewBag.CraftList = GetCraftList();
        var data = GetOrderDetails(selectedCraft);

        ViewBag.SelectedCraft = selectedCraft;
        ViewBag.Stock = GetStockCount(selectedCraft);
        ViewBag.Data = data;

        return View();
    }

    private List<CraftDetail> GetOrderDetails(string selectedCraft)
    {
        List<CraftDetail> orders = new List<CraftDetail>();
        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand("sp_GetOrdersByCraft", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CraftName", selectedCraft);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orders.Add(new CraftDetail
                        {
                            Name = reader["Name"].ToString(),
                            MobileNum = reader["Mobile"].ToString(),
                            Address = reader["Address"].ToString(),
                            PayMode = reader["PaymentType"].ToString(),
                            CraftType = reader["CraftName"].ToString(),
                            OrderDate = reader["OrderDate"].ToString()
                        });
                    }
                }
            }
        }
        return orders;
    }

    private List<CraftModel> GetCraftList()
    {
        List<CraftModel> craftList = new List<CraftModel>();

        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT Id, CraftName FROM Crafts", conn);

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    craftList.Add(new CraftModel
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["CraftName"].ToString()
                    });
                }
            }
        }

        return craftList;
    }



    private int GetStockCount(string craftName)
    {
        int stock = 0;
        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand("SELECT AvailableStock FROM StockDetails WHERE CraftName = @CraftName", conn))
            {
                cmd.Parameters.AddWithValue("@CraftName", craftName);
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    stock = Convert.ToInt32(result);
                }
            }
        }
        return stock;
    }


    [HttpGet]
    public JsonResult GetCraftOrders(int craftId)
    {
        List<CraftDetail> orders = new List<CraftDetail>();

        using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        {
            using (SqlCommand cmd = new SqlCommand("sp_GetOrdersByCraft", con)) // 🔴 Replace with your stored procedure name
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CraftName", craftId);

                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        orders.Add(new CraftDetail
                        {
                            Name = dr["Name"].ToString(),
                            MobileNum = dr["Mobile"].ToString(),
                            Address = dr["Address"].ToString(),
                            PayMode = dr["PaymentType"].ToString(),
                            OrderDate = Convert.ToDateTime(dr["OrderDate"]).ToString("yyyy-MM-dd")
                        });
                    }
                }
            }
        }

        return Json(orders);
    }

    public class CraftDetail
    {
        public string Name { get; set; }
        public string MobileNum { get; set; }
        public string Address { get; set; }
        public string PayMode { get; set; }
        public string CraftType { get; set; }
        public string OrderDate { get; set; }
    }



}
