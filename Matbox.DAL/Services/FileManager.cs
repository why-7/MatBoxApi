using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Matbox.DAL.Services
{
    public static class FileManager
    {
        public static async Task<string> SaveFile(byte[] fileBytes)
        {
            var hash = GetHash(fileBytes);
            var path = "../Matbox.DAL/Files/" + hash;
            if (!File.Exists(path))
            {
                await File.WriteAllBytesAsync(path, fileBytes);
            }
            
            return hash;
        }
        
        public static string GetHash(byte[] uploadedFileBytes)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(uploadedFileBytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}