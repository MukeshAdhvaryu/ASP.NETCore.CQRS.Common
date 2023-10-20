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
    public abstract class TestQuery<TOutDTO, TModel, TID>
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
        IQueryContract<TOutDTO, TModel, TID> Contract;
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
        public TestQuery()
        {
            Fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
            Contract = CreateService();
        }
        #endregion
         
        #region CREATE SERVICE
        protected abstract IQueryContract<TOutDTO, TModel, TID> CreateService();
        #endregion

        #region GET MODEL/S
        //-:cnd:noEmit
#if !MODEL_NONREADABLE && !MODEL_NONQUERYABLE
        [NoArgs]
        public async Task Get_ByIDSuccess()
        {
            var model = Contract.Query.GetFirstModel();
            var result = await Contract.Query.Get(model?.ID);
            Verifier.NotNull(result);
        }

        [NoArgs]
        public async Task Get_ByIDFail()
        {
            var id = default(TID);
            try
            {
                await Contract.Query.Get(id);
                Verifier.Equal(true, true);
            }
            catch (Exception ex)
            {
                var exceptionType = (ex as IModelException)?.Type ?? 0;
                switch (exceptionType)
                {
                    case ExceptionType.NoModelFound:
                    case ExceptionType.NoModelFoundForID:
                        Verifier.Equal(true, true);
                        return;
                    default:
                        break;
                }
                Verifier.Equal(false, false);
            }
        }

        [WithArgs]
        [Args(0)]
        [Args(3)]
        public async Task GetAll_Success(int count = 0)
        {
            if (count == 0)
            {
                count = Contract.Query.GetModelCount();
                var expected = await Contract.Query.GetAll(count);
                Verifier.Equal(count, expected?.Count());
            }
            else
            {
                var expected = await Contract.Query.GetAll(count);
                Verifier.Equal(count, expected?.Count());
            }
        }

        [WithArgs]
        [Args(-1)]
        public async Task GetAll_Fail(int count = 0)
        {
            try
            {
                await Contract.Query.GetAll(count);
                Verifier.Equal(true, true);
            }
            catch (Exception ex)
            {
                var exceptionType = (ex as IModelException)?.Type ?? 0;
                switch (exceptionType)
                {
                    case ExceptionType.NegativeFetchCount:
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
#if (!MODEL_NONREADABLE && !MODEL_NONQUERYABLE)                                            
            return Contract.Query.GetFirstModel();

#elif (MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE)
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
                return (TOutDTO)((IExModel)model).ToDTO(DTOType);
            return (TOutDTO)(object)model;
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
