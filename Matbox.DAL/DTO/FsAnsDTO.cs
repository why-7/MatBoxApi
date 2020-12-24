using System.IO;

namespace Matbox.DAL.DTO
{
    public class FsAnsDTO
    {
        public int StatusCode { get; set; }
        public string Comment { get; set; }
        public FileStream Fs { get; set; }
    }
}