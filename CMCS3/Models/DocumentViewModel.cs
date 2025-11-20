namespace CMCS.Models
{
    public class DocumentViewModel
    {
        public int DocumentID { get; set; }
        public int ClaimID { get; set; }
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
    }
}
