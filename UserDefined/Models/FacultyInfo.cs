using System.ComponentModel;

using CQRS.Common.Attributes;
using CQRS.Common.Models;

namespace UserDefined.Models.Models
{
    public interface IFacultyInfo: IModel
    {
        Faculty Faculty { get;  }
        string? Description { get; }
    }

    [DBConnect(ProvideSeedData = true)]
    public class FacultyInfo: KeylessModel<FacultyInfo>, IFacultyInfo
    {
        Faculty faculty;
        public FacultyInfo() { }
        public FacultyInfo(Faculty faculty, string? description = null)
        {
            Faculty = faculty;
            Description = description;
        }

        public Faculty Faculty { get => faculty; set => faculty = value; }

        [ReadOnly(true)]
        public string? Description { get; private set; }

        public override object? this[string? propertyName]
        {
            get
            {
                if (string.IsNullOrEmpty(propertyName))
                    return null;

                propertyName = propertyName.ToLower();
                switch (propertyName)
                {
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

        protected override bool Parse(string? propertyName, object? propertyValue, out object? parsedValue, bool updateValueIfParsed)
        {
            parsedValue = null;
            propertyName = propertyName?.ToLower();
            if (string.IsNullOrEmpty(propertyName) || propertyValue == null)
                return false;

            switch (propertyName)
            {
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
                case "description":
                    if (propertyValue != null)
                    {
                        var value = propertyValue.ToString();
                        if (updateValueIfParsed)
                            Description = value;
                        parsedValue = value;
                        return true;
                    }
                    break;
                default:
                    break;
            }
            return false;
        }
        protected override Task<Tuple<bool, string>> CopyFrom(IModel model)
        {
            if (!(model is IFacultyInfo))
                return Task.FromResult(Tuple.Create(false, GetModelExceptionMessage(ExceptionType.InAppropriateModelSupplied, model?.ToString())));

            var info = (IFacultyInfo)model;
            Faculty = info.Faculty;
            Description = info.Description;
            return Task.FromResult(Tuple.Create(true, "All success"));
        }

        protected override IEnumerable<IModel> GetInitialData()
        {
            return new FacultyInfo[]
            {
                new FacultyInfo(Faculty.Arts, "This is for people who want to persue artistic professions"),
                new FacultyInfo(Faculty.Commerce, "This is for people who want to persue financial professions"),
                new FacultyInfo(Faculty.Arts, "This is for people who want to persue scientific professions"),
            };
        }

        //-:cnd:noEmit
#if MODEL_USEDTO
        protected override IModel? ToDTO(Type type)
        {
            return base.ToDTO(type);
        }
#endif
        //-:cnd:noEmit
    }
}
