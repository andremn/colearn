namespace FinalProject.Model
{
    public abstract class TagDataTransfer : IDataTransfer
    {
        public int Id { get; set; }
        
        public string Text { get; set; }
    }
}
