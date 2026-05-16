using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyFlow.Model
{
    public class AppUser
    {
        public string Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPassword { get; set; }
        public string? UserMobile { get; set; }
        public string? UBDate { get; set; }
        public string? RegDate { get; set; }
        public bool IsAdmin { get; set; } = false;
        public string? UserLogoBase64 { get; set; }
        public string? UserLogoId { get; set; }
        //UserLogoBase64 → the actual image stored as a text string (Base64 is a way to convert an image into a long string of text so it can be saved in Firebase)
        //UserLogoId → the ID/reference to the image if it's stored in Firebase Storage
    }
}
