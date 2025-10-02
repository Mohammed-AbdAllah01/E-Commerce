using Project.Enums;
using System.ComponentModel.DataAnnotations;

namespace Project.DTOs
{
    public class dtoNewUser
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public required string UserName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        // public IFormFile? IMG { get; set; }  // Optional image path (URL or local path)


        [Required]
        [StringLength(100, MinimumLength = 6)] // Ensure password length
        public required string Password { get; set; }

        [Required]
        [Phone] // Validates phone number format
        public required string Phone { get; set; }

        [Required]
        public required string State { get; set; }

        [Required]
        public required string Governorate { get; set; }

        [Required]
        public required string Location { get; set; }

        [Required]
        public required string Gender { get; set; }
    }


public class dtoNewCustomer : dtoNewUser
    {
        public required DateTime BirthDate { get; set; }

    }
    public class dtoNewAdmin : dtoNewUser
    {
        public required long NationalId { get; set; }
        public required DateTime BirthDate { get; set; }

    }
    public class dtoNewDeliveryRep : dtoNewUser
    {
        public required long NationalId { get; set; }
        public required DateTime BirthDate { get; set; }
        public required string adminId { get; set; }
    }
    public class dtoNewMerchant : dtoNewUser
    {
        public required long NationalId { get; set; }
    }




    public class dtoProfile
    {
        public required string Name { get; set; }

        public required string Image { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; } 
        public required string Governorate { get; set; }
        public required string Location { get; set; }
        public required string State { get; set; }



    }




    }
