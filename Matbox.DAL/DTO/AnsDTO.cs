using System.Collections.Generic;
using System.Linq;
using Matbox.DAL.Models;

namespace Matbox.DAL.DTO
{
    public class AnsDTO
    {
        public int StatusCode { get; set; }
        public string Comment { get; set; }
        public IEnumerable<Material> Materials { get; set; }
    }
}