using System.Collections.Generic;

namespace Matbox.DAL.DTO
{
    public class AnsDTO
    {
        public int StatusCode { get; set; }
        public string Comment { get; set; }
        public IEnumerable<MaterialDto> MaterialsDtos { get; set; }
    }
}