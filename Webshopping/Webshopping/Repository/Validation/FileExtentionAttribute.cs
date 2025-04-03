using System.ComponentModel.DataAnnotations;

namespace Webshopping.Repository;

public class FileExtentionAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        // Example: Validate file extension
        if (value is IFormFile file)
        {
            var allowedExtensions = new[] { "jpg", "png", "jpeg" }; // Allowed file extensions
            var fileExtension = System.IO.Path.GetExtension(file.FileName); // Use FileName property
            bool result = allowedExtensions.Any(ext => fileExtension.EndsWith(ext, StringComparison.OrdinalIgnoreCase));

            if (!result)
            {
                return new ValidationResult($@"The file extension '{fileExtension}' is not allowed. 
                    Allowed extensions are: {string.Join(", ", allowedExtensions)}.");
            }
        }
        return ValidationResult.Success; // Validation passed
    }
}