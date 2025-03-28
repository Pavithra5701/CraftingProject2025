using CraftingProject.Models;
using System.Data.SqlClient;
using System.Data;

namespace CraftingProject.Repository
{
    public class CraftRepository
    {
        private readonly string _connectionString;

        public CraftRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Fetch crafts from the database using ADO.NET
        public List<Craft> GetCrafts()
        {
            List<Craft> crafts = new List<Craft>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("GetCrafts", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                crafts.Add(new Craft
                                {
                                    Id = reader.GetInt32(0),
                                    CraftName = reader.GetString(1),
                                    ImageUrl = reader.IsDBNull(2) ? null : reader.GetString(2)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching crafts: " + ex.Message);
            }

            return crafts; // Return an empty list if an error occurs
        }

        public bool PlaceOrder(Order order)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_InsertOrder", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Name", order.Name);
                cmd.Parameters.AddWithValue("@Mobile", order.Mobile);
                cmd.Parameters.AddWithValue("@Address", order.Address);
                cmd.Parameters.AddWithValue("@CraftName", order.CraftName);
                cmd.Parameters.AddWithValue("@PaymentType", order.PaymentType);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
    }
}
