using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Bulky.Models.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Name")]
        [MaxLength(30, ErrorMessage = "Max lenght 30!")]
        public string Title { get; set; }
        public string Description { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "Max lenght 50!")]
        public string ISBN { get; set; }

        [Required]
        [MaxLength(30, ErrorMessage = "Max lenght 30!")]
        public string Author { get; set; }

        [Required]
        [DisplayName("List Price")]
        [Range(1, 1000)]
        public double ListPrice { get; set; }

        [Required]
        [DisplayName("Price for 1-50")]
        [Range(1,1000)]
        public double Price { get; set; }
        [Required]
        [DisplayName("Price for 50+")]
        [Range(1, 1000)]
        public double Price50 { get; set; }
        [Required]
        [DisplayName("Price for 100+")]
        [Range(1, 1000)]
        public double Price100 { get; set; }

		[DisplayName("Category")]
		public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
		[ValidateNever]
		public Category Category { get; set; }

		[ValidateNever]
		public string ImageUrl { get; set; }
    }
}
