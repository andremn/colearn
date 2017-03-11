using System;

namespace FinalProject.Searching
{
    [Serializable]
    public class QuestionSearchResult
    {
        public QuestionSearchResult()
        {
        }

        public QuestionSearchResult(int id, string title)
        {
            Id = id;
            Title = title;
        }

        public int Id { get; set; }

        public string Title { get; set; }
    }
}