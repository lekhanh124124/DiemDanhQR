// File: Helpers/Functions/AvatarHelper.cs
namespace DiemDanhQR_API.Helpers
{
    public static class AvatarHelper
    {
        private static readonly string[] _allowedExt = [".jpg", ".jpeg", ".png", ".webp"];
        private const long _maxSize = 5_000_000; // 5MB

        public static async Task<string?> SaveAvatarAsync(IFormFile? file, string webRootPath, string userId)
        {
            if (file == null || file.Length == 0) return null;
            if (file.Length > _maxSize) throw new Exception("Ảnh vượt quá 5MB.");
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExt.Contains(ext)) throw new Exception("Chỉ hỗ trợ JPG/PNG/WebP.");

            // Fallback nếu webRootPath = null/empty
            var root = string.IsNullOrWhiteSpace(webRootPath)
                ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
                : webRootPath;

            var dir = Path.Combine(root, "uploads", "avatars");
            Directory.CreateDirectory(dir);

            var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";
            var path = Path.Combine(dir, fileName);

            await using var fs = File.Create(path);
            await file.CopyToAsync(fs);

            return $"/uploads/avatars/{fileName}";
        }
    }
}
