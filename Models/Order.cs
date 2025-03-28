using System;
using System.ComponentModel.DataAnnotations;

namespace CraftingProject.Models
{


    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, MaxLength(10)]
        public string Mobile { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string CraftName { get; set; }

        [Required]
        public string PaymentType { get; set; } // Cash on Delivery / Online Payment

        public DateTime OrderDate { get; set; }
    }

}
