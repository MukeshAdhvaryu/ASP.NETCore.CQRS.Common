using System.ComponentModel;

using CQRS.Common.Attributes;
using CQRS.Common.Models;
using CQRS.Common.Parameters;

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

        public Faculty Faculty { get => faculty; set => faculty = value; }

        [ReadOnly(true)]
        public string? Description { get; private set; }

        public FacultyInfo() { }
        public FacultyInfo(Faculty faculty, string? description = null)
        {
            Faculty = faculty;
            Description = description;
        }
        protected override Message Parse(ObjParameter parameter, out object? currentValue, out object? parsedValue, bool updateValueIfParsed = false)
        {
            var value = parameter is ModelParameter ? ((ModelParameter)parameter).FirstValue : parameter.Value;
            currentValue = null;
            parsedValue = null;
            var name = parameter.Name;

            switch (name)
            {
                case nameof(Faculty):
                    currentValue = faculty;
                    Faculty f;
                    if (value is Faculty)
                    {
                        var result = (Faculty)value;
                        parsedValue = result;
                        if (updateValueIfParsed)
                            faculty = result;
                        return Message.Sucess(name);
                    }
                    if (value is string && (Enum.TryParse((string)value, out f)) ||
                        value != null && (Enum.TryParse(value.ToString(), out f)))
                    {
                        parsedValue = f;
                        if (updateValueIfParsed)
                            faculty = f;
                        return Message.Sucess(name);
                    }
                    if (value == null)
                        return Message.MissingValue(name);
                    break;
                case nameof(Description):
                    currentValue = Description;

                    if (value != null)
                    {
                        parsedValue = parameter.Value.ToString();
                        if (updateValueIfParsed)
                            Description = parsedValue.ToString();
                        return Message.Sucess(name);
                    }
                    return Message.Ignored(name);
                default:
                    break;
            }
            return Message.Failure(name);
        }

        protected override Task<bool> CopyFrom(IModel model)
        {
            if(model is IFacultyInfo)
            {
                var info = (IFacultyInfo)model;
                Faculty = info.Faculty;
                Description = info.Description;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
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

        protected override IModel? ToDTO(Type type)
        {
            return base.ToDTO(type);
        }
    }
}
