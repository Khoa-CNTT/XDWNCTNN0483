using System.ComponentModel.DataAnnotations;

namespace Webshopping.Repository
{
    public class FileExtentionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Example: Validate file extension
            if (value is string fileName)
            {
                var allowedExtensions = new[] { ".jpg", ".png", ".jpeg" }; // Allowed file extensions
                var fileExtension = System.IO.Path.GetExtension(fileName);

                if (!allowedExtensions.Contains(fileExtension.ToLower()))
                {
                    return new ValidationResult($@"The file extension '{fileExtension}' is not allowed. 
                    Allowed extensions are: {string.Join(", ", allowedExtensions)}.");
                }
            }

            return ValidationResult.Success; // Validation passed
        }
    }
}