using System.IO;

namespace Matbox.DAL.DTO
{
    public class FsAnsDTO : AnsDTO
    {
        public FileStream Fs { get; set; }
    }
}