
using Microsoft.AspNetCore.Mvc;
using CMCS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace CMCS.Controllers
{
    public class DocumentsController : Controller
    {
        // Reflection access to private _claims in ClaimsController like ApprovalsController does
        private static System.Reflection.FieldInfo claimsField =
            typeof(ClaimsController).GetField("_claims", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        private static Dictionary<int, ClaimViewModel> GetClaimsDict()
        {
            return claimsField?.GetValue(null) as Dictionary<int, ClaimViewModel>;
        }

        private static string StorageFolder => Path.Combine(Directory.GetCurrentDirectory(), "EncryptedFiles");

        private static readonly string passphrase = "cmcs-prototype-passphrase-2025"; // simple prototype key

        // GET: Documents/Upload/5
        [HttpGet]
        public IActionResult Upload(int id)
        {
            var claims = GetClaimsDict();
            if (claims == null || !claims.ContainsKey(id))
                return NotFound();

            return View(claims[id]);
        }

        // POST: Documents/Upload/5
        [HttpPost]
        [RequestSizeLimit(10_000_000)] // 10MB
        public IActionResult UploadFile(int id, IFormFile file)
        {
            var claims = GetClaimsDict();
            if (claims == null || !claims.ContainsKey(id))
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("Track", "Claims");
            }

            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "No file selected.";
                return RedirectToAction("Upload", new { id });
            }

            // File validations
            var allowed = new[] { ".pdf", ".docx", ".xlsx" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
            {
                TempData["Error"] = "Invalid file type. Only .pdf, .docx and .xlsx are allowed.";
                return RedirectToAction("Upload", new { id });
            }
            const long maxBytes = 5 * 1024 * 1024; // 5 MB
            if (file.Length > maxBytes)
            {
                TempData["Error"] = "File too large. Limit is 5 MB.";
                return RedirectToAction("Upload", new { id });
            }

            Directory.CreateDirectory(StorageFolder);

            // Read file bytes
            byte[] plain;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                plain = ms.ToArray();
            }

            // Encrypt bytes
            var storedFileName = $"{id}_{Guid.NewGuid()}{ext}.enc";
            var storedPath = Path.Combine(StorageFolder, storedFileName);
            try
            {
                EncryptToFile(plain, storedPath, passphrase);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to encrypt and store file: " + ex.Message;
                return RedirectToAction("Upload", new { id });
            }

            // Link to claim
            var claim = claims[id];
            var doc = new DocumentViewModel
            {
                ClaimID = id,
                FileName = file.FileName,
                FilePath = storedFileName
            };
            claim.Documents.Add(doc);
            claim.UploadedFiles.Add(file.FileName);

            TempData["Message"] = "File uploaded successfully.";
            if (string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", System.StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = true, redirect = Url.Action("Details", "Claims", new { id = claim.ClaimId }) });
            }
            return RedirectToAction("Details", "Claims", new { id = claim.ClaimId });
        }

        // GET: Documents/Download?claimId=1&file=storedName
        [HttpGet]
        public IActionResult Download(int claimId, string file)
        {
            var claims = GetClaimsDict();
            if (claims == null || !claims.ContainsKey(claimId))
                return NotFound();

            var claim = claims[claimId];
            var doc = claim.Documents.FirstOrDefault(d => d.FilePath == file || d.FileName == file);
            if (doc == null)
                return NotFound();

            var storedPath = Path.Combine(StorageFolder, doc.FilePath);
            if (!System.IO.File.Exists(storedPath))
                return NotFound();

            try
            {
                var bytes = DecryptFromFile(storedPath, passphrase);
                var contentType = GetContentTypeByExtension(Path.GetExtension(doc.FileName));
                // If PDF, serve inline so browsers can preview; otherwise force download
                if (contentType == "application/pdf")
                {
                    return new FileContentResult(bytes, contentType)
                    {
                        FileDownloadName = doc.FileName
                    };
                }
                return File(bytes, contentType, doc.FileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to decrypt file: " + ex.Message;
                return RedirectToAction("Details", "Claims", new { id = claimId });
            }
        }

        private static string GetContentTypeByExtension(string ext)
        {
            ext = ext?.ToLowerInvariant();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };
        }

        // Simple AES-CBC encryption with PBKDF2-derived key. Stores IV (16 bytes) + ciphertext in file.
        private static void EncryptToFile(byte[] data, string path, string pass)
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            var salt = Encoding.UTF8.GetBytes("cmcs-salt-v1");
            using var kdf = new Rfc2898DeriveBytes(pass, salt, 100000, System.Security.Cryptography.HashAlgorithmName.SHA256);
            aes.Key = kdf.GetBytes(32);
            aes.GenerateIV();
            aes.Padding = PaddingMode.PKCS7;
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            // write IV first
            fs.Write(aes.IV, 0, aes.IV.Length);
            using var crypto = new CryptoStream(fs, aes.CreateEncryptor(), CryptoStreamMode.Write);
            crypto.Write(data, 0, data.Length);
            crypto.FlushFinalBlock();
        }

        private static byte[] DecryptFromFile(string path, string pass)
        {
            using var aes = Aes.Create();
            var salt = Encoding.UTF8.GetBytes("cmcs-salt-v1");
            using var kdf = new Rfc2898DeriveBytes(pass, salt, 100000, System.Security.Cryptography.HashAlgorithmName.SHA256);
            aes.Key = kdf.GetBytes(32);
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            // read IV
            var iv = new byte[16];
            var read = fs.Read(iv, 0, iv.Length);
            if (read != iv.Length) throw new Exception("Invalid file format (IV).");
            aes.IV = iv;
            using var crypto = new CryptoStream(fs, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var ms = new MemoryStream();
            crypto.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
