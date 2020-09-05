using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UdemyRealWordUnitTest.Web.Models
{
    public partial class Product
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public decimal? Price { get; set; }
        [Required]
        public int? Stock { get; set; }
        [Required]
        public string Color { get; set; }
        [Required]
        public int Id { get; set; }
    }
}
