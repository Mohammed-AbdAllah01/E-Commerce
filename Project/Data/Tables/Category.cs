using System.ComponentModel.DataAnnotations;

namespace Project.Tables
{
    public class Category
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public required string Name { get; set; }

        public required string Type { get; set; }

        public ICollection<Product>? products { get; set; }  //one To Many  
    }
}
