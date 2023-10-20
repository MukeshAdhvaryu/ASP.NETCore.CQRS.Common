/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
Author: Mukesh Adhvaryu.
*/

//-:cnd:noEmit
#if MODEL_ADDTEST
//+:cnd:noEmit

using System.ComponentModel.DataAnnotations;

using MicroService.Common.Attributes;
using MicroService.Common.Models;

namespace MicroService.Common.Tests
{
    #region TestModel
    [DBConnect(ProvideSeedData = true)]
    public sealed class TestModel: ModelInt32<TestModel>
    {
        #region CONSTRUCTORS
        public TestModel(string name) :
            base(true)
        {
            Name = name;
        }
        public TestModel() :
            base(false)
        { 

        }
        #endregion

        #region PROPERTIES
        [Required]  
        public string? Name { get; set; }
        #endregion

        #region COPY FROM
        protected override Task<Tuple<bool, string>> CopyFrom(IModel model)
        {
            if (!(model is TestModel))
                return Task.FromResult(Tuple.Create(false, GetModelExceptionMessage(ExceptionType.InAppropriateModelSupplied, model?.ToString())));
            Name = ((TestModel)model).Name;
            return Task.FromResult(Tuple.Create(true, "All success"));
        }
        #endregion

        #region GET INITIAL DATA
        protected override IEnumerable<IModel> GetInitialData()
        {
            return new TestModel[]
            {
               new TestModel("John"),
               new TestModel("Timothy"),
               new TestModel("Charlie"),
               new TestModel("Felicity"),
               new TestModel("Margot"),
            };
        }
        #endregion

        #region PARSE
        protected override bool Parse(string? propertyName, object? propertyValue, out object? parsedValue, bool updateValueIfParsed)
        {
            if(base.Parse(propertyName, propertyValue, out parsedValue, updateValueIfParsed))
                return true;

            parsedValue = null;
            propertyName = propertyName?.ToLower();
            switch (propertyName)
            {
                case "name":
                    if (propertyValue is string)
                    {
                        var value = (string)propertyValue;
                        parsedValue = value;
                        if (updateValueIfParsed)
                            Name = value;
                        return true;
                    }
                    break;
                default:
                    break;
            }
            return false;
        }
        #endregion

        //-:cnd:noEmit
#if MODEL_USEDTO
        #region IModelToDTO
        protected override IModel? ToDTO(Type type)
        {
            if (type == typeof(TestModelDTO))
                return new TestModelDTO(this);
            return base.ToDTO(type);
        }
        #endregion
#endif
        //+:cnd:noEmit
    }
    #endregion
}
//-:cnd:noEmit
#endif
//+:cnd:noEmit
