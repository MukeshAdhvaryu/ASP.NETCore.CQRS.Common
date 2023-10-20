/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
Author: Mukesh Adhvaryu.
*/

//-:cnd:noEmit
#if MODEL_ADDTEST
//+:cnd:noEmit

using AutoFixture;
using AutoFixture.AutoMoq;

using CQRS.Common.Exceptions;
using CQRS.Common.Interfaces;
using CQRS.Common.Models;
using CQRS.Common.Services;
using CQRS.Common.Tests.Attributes;

namespace CQRS.Common.Tests
{
    public abstract class TestCommand<TOutDTO, TModel, TID>
        #region TYPE CONSTRINTS
        where TOutDTO : IModel, new()
        where TModel : Model<TID, TModel>,
        //-:cnd:noEmit
#if (!MODEL_USEDTO)
        TOutDTO,
#endif
        //+:cnd:noEmit
        new()
        where TID : struct
        #endregion
    {
        #region VARIABLES
        ICommandContract<TOutDTO, TModel, TID> Contract;
        //-:cnd:noEmit
#if MODEL_USEDTO
        static readonly Type DTOType = typeof(TOutDTO);
        static readonly bool NeedToUseDTO = !DTOType.IsAssignableFrom(typeof(TModel));
#endif
        //+:cnd:noEmit
        protected readonly IFixture Fixture;
        static readonly IExModelExceptionSupplier DummyModel = new TModel();
        #endregion

        #region CONSTRUCTOR
        public TestCommand()
        {
            Fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
            Contract = CreateService();
        }
        #endregion

        #region CREATE SERVICE
        protected abstract ICommandContract<TOutDTO, TModel, TID> CreateService();
        #endregion

        #region ADD MODEL
        //-:cnd:noEmit
#if MODEL_APPENDABLE
        [NoArgs]
        public async Task Add_Success()
        {
            var model = Fixture.Create<TModel>();
            var expected = await Contract.Command.Add(model);
            Verifier.NotNull(expected);
        }
        [NoArgs]
        public async Task Add_Fail()
        {
            var model = (IExModel<TID>)Fixture.Create<TModel>();
            model.ID = GetFirstModel()?.ID ?? default(TID);

            try
            {
                await Contract.Command.Add(model);
                Verifier.Equal(true, true);
            }
            catch (Exception ex)
            {
                var exceptionType = (ex as IModelException)?.Type ?? 0;
                switch (exceptionType)
                {
                    case ExceptionType.AddOperationFailed:
                    case ExceptionType.NoModelSupplied:
                        Verifier.Equal(true, true);
                        return;
                    default:
                        break;
                }
                Verifier.Equal(false, false);
            }
        }
#endif
        //+:cnd:noEmit
        #endregion

        #region DELETE MODEL
        //-:cnd:noEmit
#if MODEL_DELETABLE
        [NoArgs]
        public async Task Delete_Success()
        {
            TModel? model = GetFirstModel();
            var expected = await Contract.Command.Delete(model?.ID ?? default(TID));
            Verifier.NotNull(expected);
        }

        [NoArgs]
        public async Task Delete_Fail()
        {
            var ID = default(TID);
            try
            {
                await Contract.Command.Delete(ID);
                Verifier.Equal(true, true);
            }
            catch (Exception ex)
            {
                var exceptionType = (ex as IModelException)?.Type ?? 0;
                switch (exceptionType)
                {
                    case ExceptionType.NoModelFoundForID:
                    case ExceptionType.DeleteOperationFailed:
                        Verifier.Equal(true, true);
                        return;
                    default:
                        break;
                }
                Verifier.Equal(false, false);
            }
        }
#endif
        //+:cnd:noEmit
        #endregion

        #region UPDATE MODEL
        //-:cnd:noEmit
#if MODEL_UPDATABLE
        [NoArgs]
        public async Task Update_Success()
        {
            var model = Fixture.Create<TModel>();
            var ID = GetFirstModel()?.ID ?? default(TID);
            var expected = await Contract.Command.Update(ID, model);
            Verifier.NotNull(expected);
        }

        [NoArgs]
        public async Task Update_Fail()
        {
            var ID = default(TID);
            var model = GetFirstModel();
            try
            {
                await Contract.Command.Update(ID, model);
                Verifier.Equal(true, true);
            }
            catch (Exception ex)
            {
                var exceptionType = (ex as IModelException)?.Type ?? 0;
                switch (exceptionType)
                {
                    case ExceptionType.UpdateOperationFailed:
                    case ExceptionType.NoModelSupplied:
                    case ExceptionType.ModelCopyOperationFailed:
                        Verifier.Equal(true, true);
                        return;
                    default:
                        break;
                }
                Verifier.Equal(false, false);
            }
        }
#endif
        //+:cnd:noEmit
        #endregion

        #region GET FIRST MODEL
        protected TModel? GetFirstModel()
        {
            //-:cnd:noEmit
#if (MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE)
            return ((IExCommand<TOutDTO, TModel, TID>)Contract.Command).GetFirstModel();
#else
            return default(TModel?);
#endif
            //+:cnd:noEmit
        }
        #endregion

        #region TO DTO
        //-:cnd:noEmit
#if (MODEL_USEDTO)
        protected TOutDTO? ToDTO(TModel? model)
        {
            if (model == null)
                return default(TOutDTO);
            if (NeedToUseDTO)
                return (TOutDTO?)((IExModel)model).ToDTO(DTOType);
            return (TOutDTO?)(object)model;
        }
#else
        protected TOutDTO? ToDTO(TModel? model)
        {
            if(model == null)
                return default(TOutDTO);
            return (TOutDTO)(object)model;
        }
#endif
        //+:cnd:noEmit
        #endregion

        #region CLASS DATA & MEMBER DATA EXAMPLE 
        /*      
        //This is an example on how to use source member data.
        //To use member data, you must define a static method or property returning IEnumerable<object[]>.
        [WithArgs]
        [ArgSource(typeof(MemberDataExample), "GetData")]
        public Task GetAll_ReturnAllUseMemberData(int count = 0)
        {
            //
        }

        //This is an example on how to use source class data.
        //To use class data, ArgSource<source> will suffice.
        [WithArgs]
        [ArgSource<ClassDataExample>]
        public Task GetAll_ReturnAllUseClassData(int count = 0)
        {
            //
        }

        class MemberDataExample 
        {
            public static IEnumerable<object[]> GetData
            {
                get
                {
                    yield return new object[] { 0 };
                    yield return new object[] { 3 };
                    yield return new object[] { -1 };
                }
            }
        }

        class ClassDataExample: ArgSource 
        {
            public override IEnumerable<object[]> Data
            {
                get
                {
                    yield return new object[] { 0 };
                    yield return new object[] { 3 };
                    yield return new object[] { -1 };
                }
            }
        }
        */
        #endregion
    }
}
//-:cnd:noEmit
#endif
//+:cnd:noEmit
