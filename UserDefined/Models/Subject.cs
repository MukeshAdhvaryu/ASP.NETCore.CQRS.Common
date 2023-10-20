using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using CQRS.Common.Attributes;
using CQRS.Common.Models;

//-:cnd:noEmit
#if MODEL_USEDTO
using UserDefined.DTOs;
#endif
//+:cnd:noEmit

namespace UserDefined.Models
{
    public enum Faculty : byte
    {
        Arts,
        Commerce,
        Science

    }

    #region ISubject
    public interface ISubject : IModel<int>
    {
        [Required]
        string? Name { get; }

        [Required]
        Faculty Faculty { get; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        string Description { get; }
        
        Book Book { get; }
    }
    #endregion

    #region Subject
    [Model(Scope = ServiceScope.Scoped, Name = "Subject")]
    //[DBConnect(Database = "SubjectDB", ConnectionKey = ConnectionKey.SQLServer)]
    [DBConnect(ProvideSeedData = true)]
    public class Subject : ModelInt32<Subject>, ISubject
    {
        #region VARIABLES
        Faculty faculty;
        Book book;
        #endregion

        #region CONSTRUCTORS
        public Subject() : 
            base(false)
        { }
        public Subject(string name, Faculty stream):
            base(true)
        {
            Name = name;
            faculty = stream;
        }
        public Subject(int id, string name, Faculty stream) :
            base(false)
        {
            this.ID = id;
            Name = name;
            faculty = stream;
        }
        #endregion

        #region PROPERTIES
        [Required]
        public string? Name { get; set; }

        [Required]
        public Faculty Faculty { get => faculty; set => faculty = value; }


        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string Description => Faculty + ": " + Name;

        public override object? this[string? propertyName]
        {
            get
            {
                if (string.IsNullOrEmpty(propertyName))
                    return null;

                propertyName = propertyName.ToLower();
                switch (propertyName)
                {
                    case "name":
                        return Name;
                    case "faculty":
                        return Faculty;
                    case "description":
                        return Description;
                    default:
                        break;
                }
                return base[propertyName];
            }
        }

        public Book Book { get => book; set => book = value; }
        #endregion

        #region PARSE
        protected override bool Parse(string? propertyName, object? propertyValue, out object? parsedValue, bool updateValueIfParsed)
        {
            if( base.Parse(propertyName, propertyValue, out parsedValue, updateValueIfParsed))
                return true;
            
            propertyName = propertyName?.ToLower();
            if (string.IsNullOrEmpty(propertyName) || propertyValue == null)
                return false;

            switch (propertyName)
            {
                case "name":
                    if (propertyValue is string)
                    {
                        var value = (string)propertyValue;
                        if (updateValueIfParsed)
                            Name = value;
                        parsedValue = value;
                        return true;
                    }
                    break;
                case "description":
                    if (propertyValue is string)
                    {
                        parsedValue = (string)propertyValue;
                        return true;
                    }
                    break;

                case "faculty":
                    Faculty f;
                    if (propertyValue is Faculty)
                    {
                        f = (Faculty)propertyValue;
                        if (updateValueIfParsed)
                            faculty = f;
                        parsedValue = f;
                        return true;
                    }
                    if (propertyValue is string && (Enum.TryParse((string)propertyValue, out f)) ||
                        propertyValue != null && (Enum.TryParse(propertyValue.ToString(), out f)))
                    {
                        if (updateValueIfParsed)
                            faculty = f;
                        parsedValue = f;
                        return true;
                    }
                    break;
                case "book.title":
                    if (propertyValue is string)
                    {
                        var title = (string)propertyValue;
                        if (updateValueIfParsed)
                            book.Title = title;
                        parsedValue = title;
                        return true;
                    }
                    break;
                case "book.amount":
                    if (propertyValue is float)
                    {
                        var a = (float)propertyValue;
                        if (updateValueIfParsed)
                            book.Amount = a;
                        parsedValue = a;
                        return true;
                    }
                    if (propertyValue is string)
                    {
                        if (float.TryParse((string)propertyValue, out float a))
                        {
                            if(updateValueIfParsed)
                                book.Amount = a;
                            parsedValue = a;
                            return true;
                        }
                    }
                    break;
                default:
                    break;
            }
            return false;
        }
        #endregion

        #region COPY FROM
        protected override Task<Tuple<bool, string>> CopyFrom(IModel model)
        {
            if (model is ISubject)
            {
                var subject = (ISubject)model;
                faculty = subject.Faculty;
                Name = subject.Name;
                return Task.FromResult(Tuple.Create(true, "All success"));
            }

            //-:cnd:noEmit
#if MODEL_USEDTO
            if (model is SubjectOutDTO)
            {
                var createSubjectDTO = (SubjectOutDTO)model;
                if (string.IsNullOrEmpty(createSubjectDTO.Name))
                {
                    var message = GetModelExceptionMessage(ExceptionType.MissingRequiredValue, nameof(Name));
                    return Task.FromResult(Tuple.Create(false, message));
                }
                Name = createSubjectDTO.Name;
                faculty = createSubjectDTO.Faculty;
                Book= createSubjectDTO.Book;
                return Task.FromResult(Tuple.Create(true, "All success"));
            }
            if (model is SubjectInDTO)
            {
                var createSubjectDTO = (SubjectInDTO)model;
                if (string.IsNullOrEmpty(createSubjectDTO.Name))
                {
                    var message = GetModelExceptionMessage(ExceptionType.MissingRequiredValue, nameof(Name));
                    return Task.FromResult(Tuple.Create(false, message));
                }
                Name = createSubjectDTO.Name;
                faculty = createSubjectDTO.Faculty;
                Book= createSubjectDTO.Book;
                return Task.FromResult(Tuple.Create(true, "All success"));
            }
#endif
            //+:cnd:noEmit
            return Task.FromResult(Tuple.Create(false, GetModelExceptionMessage(ExceptionType.InAppropriateModelSupplied, model?.ToString())));
        }
        #endregion

        #region GET INITIAL DATA
        protected override IEnumerable<IModel> GetInitialData()
        {
            return new Subject[]
                {
                    new Subject("Psycology", Faculty.Science),
                    new Subject("Chemistry", Faculty.Science),
                    new Subject("Physics", Faculty.Science),
                    new Subject("Maths", Faculty.Science),
                    new Subject("Software Engineering", Faculty.Science),

                    new Subject("Geography", Faculty.Science),
                    new Subject("Economics", Faculty.Commerce),
                    new Subject("Accountancy", Faculty.Commerce),
                    new Subject("English", Faculty.Arts),
                    new Subject("Hindi", Faculty.Arts),
                    new Subject("French", Faculty.Arts),
                    new Subject("History", Faculty.Arts),
                };
        }
        #endregion

        #region Model To DTO
        //-:cnd:noEmit
#if MODEL_USEDTO
        protected override IModel? ToDTO(Type type)
        {
           if(type == typeof(SubjectOutDTO))
                return new SubjectOutDTO(this);
            if (type == typeof(SubjectInDTO))
                return new SubjectInDTO(this);
            return base.ToDTO(type);
        }

#endif
        //+:cnd:noEmit
        #endregion
    }
    #endregion

}