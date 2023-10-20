//-:cnd:noEmit
#if MODEL_USEDTO
//+:cnd:noEmit
using CQRS.Common.Attributes;
using CQRS.Common.Models;

using UserDefined.Models;

namespace UserDefined.DTOs
{
    #region SubjectOutDTO
    [Model(Name = "Subject")]
    public struct SubjectOutDTO : IModel
    {
        public SubjectOutDTO(ISubject subject)
        {
            Name = subject.Name;
            Faculty = subject.Faculty;
            ID = subject.ID;
            Book = subject.Book;
        }
        public string? Name { get; set; }
        public Faculty Faculty { get; set; }
        public int ID { get; set; }
        public Book Book { get; set; }
    }
    #endregion
}

//-:cnd:noEmit
#endif
//+:cnd:noEmit
