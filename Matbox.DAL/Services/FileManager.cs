using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Matbox.DAL.Services
{
    public static class FileManager
    {
        public static async Task<string> SaveFile(byte[] fileBytes, string hash, int countOfHash)
        {
            var path = "../Matbox.DAL/Files/" + hash;
            if (countOfHash == 0)
            {
                await File.WriteAllBytesAsync(path, fileBytes);
            }
            
            return path;
        }
        
        public static string GetHash(byte[] uploadedFileBytes)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(uploadedFileBytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}