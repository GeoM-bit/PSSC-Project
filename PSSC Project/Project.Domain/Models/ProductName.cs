namespace Project.Domain.Models
{
    public class ProductName
    {
        public string Name { get; set; }
        public ProductName(string name)
        {
                Name = name;
        }
    }
}
