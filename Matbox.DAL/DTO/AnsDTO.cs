using System.Collections.Generic;

namespace Matbox.DAL.DTO
{
    public class AnsDTO
    {
        public bool IsValidBl { get; set; }
        public string Comment { get; set; }
        public IEnumerable<MaterialDto> MaterialsDtos { get; set; }
    }
}