namespace FinalProject.Model
{
    public class GradeDataTransfer
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        public int Order { get; set; }

        public override string ToString()
        {
            return Name ?? string.Empty;
        }
    }
}
