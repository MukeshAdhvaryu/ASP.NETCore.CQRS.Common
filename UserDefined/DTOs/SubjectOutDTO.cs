//-:cnd:noEmit
#if MODEL_USEDTO
//+:cnd:noEmit
using System.Text.Json.Serialization;

using CQRS.Common.Models;

using UserDefined.Models;

namespace UserDefined.DTOs
{
    #region SubjectOutDTO
    public class SubjectOutDTO : IModel
    {
        public SubjectOutDTO() { }
        public SubjectOutDTO(Subject subject)
        {
            Name = subject.Name;
            Faculty = subject.Faculty;
            ID = subject.ID;
        }
        public string? Name { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Faculty Faculty { get; set; }
        public int ID { get; }
    }
    #endregion
}

//-:cnd:noEmit
#endif
//+:cnd:noEmit
