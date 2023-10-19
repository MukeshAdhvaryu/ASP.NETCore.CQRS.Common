//-:cnd:noEmit
#if MODEL_USEDTO
//+:cnd:noEmit
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using CQRS.Common.Models;

using UserDefined.Models;

namespace UserDefined.DTOs
{
    public class SubjectInDTO : IModel
    {
        public SubjectInDTO() { }
        public SubjectInDTO(Subject subject)
        {
            Name = subject.Name;
            Faculty = subject.Faculty;
        }
       
        [Required]
        public string? Name { get; set; }

        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Faculty Faculty { get; set; }
    }
}

//-:cnd:noEmit
#endif
//+:cnd:noEmit
