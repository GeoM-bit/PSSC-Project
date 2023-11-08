using System.ComponentModel.DataAnnotations;

namespace PSSC_Project.Models
{
    public class User
    {
        [Key]
        public int User_Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
