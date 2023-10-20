//-:cnd:noEmit
#if MODEL_USEDTO
//+:cnd:noEmit

using CQRS.Common.Models;

using UserDefined.Models;

namespace UserDefined.DTOs
{
    public struct SubjectInDTO : IModel
    {
        public SubjectInDTO(ISubject subject)
        {
            Name = subject.Name;
            Faculty = subject.Faculty;
            Book = subject.Book;
        }
        public string? Name { get; set; }
        public Faculty Faculty { get; set; }
        public Book Book { get; set; }
    }
}

//-:cnd:noEmit
#endif
//+:cnd:noEmit
