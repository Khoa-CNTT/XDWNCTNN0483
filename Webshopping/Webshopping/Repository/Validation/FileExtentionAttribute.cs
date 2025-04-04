using System.ComponentModel.DataAnnotations;

namespace Webshopping.Repository;

public class FileExtentionAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };  // Bao gồm dấu chấm.
            var fileExtension = System.IO.Path.GetExtension(file.FileName);

            if (!allowedExtensions.Contains(fileExtension))
            {
                return new ValidationResult($"Phần mở rộng tệp '{fileExtension}' không được phép. Các phần mở rộng cho phép là: {string.Join(", ", allowedExtensions)}.");
            }
        }
        return ValidationResult.Success;
    }
}
