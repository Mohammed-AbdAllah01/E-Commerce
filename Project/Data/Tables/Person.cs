
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project.Enums;
using System.ComponentModel.DataAnnotations;

namespace Project.Tables
{

    public abstract class Person : IdentityUser
    {



        public string? IMG { get; set; }  // Path to the image file in the file system
        public PersonType  Type { get; set;}
        public required string State { get; set;}       
        public required string Governorate { get; set; }       
        public required string Location { get; set; }
        public required AccStatus Status { get; set; } 
        public GenderType Gender { get; set; }
        public string? VerificationCode { get; set; }
        public DateTime? VerificationCodeExpiry { get; set; }
    }
}
