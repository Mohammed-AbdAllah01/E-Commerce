namespace Project.DTOs
{
    public class ResetPasswordDTO
    {
        public string Email { get; set; }
       // public string Token { get; set; }
        public string NewPassword { get; set; }

        //public string VerificationCode { get; set; } //دي بدل التوكن لو هنستخدم كود مش لينك 
    }
}
