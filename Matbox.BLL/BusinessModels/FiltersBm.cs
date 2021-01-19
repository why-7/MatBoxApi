namespace Matbox.BLL.BusinessModels
{
    public class FiltersBm
    {
        public int Category { get; set; }
        public string UserId { get; set; }
        public long MinSize { get; set; }
        public long MaxSize { get; set; }
    }
}