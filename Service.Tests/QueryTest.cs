//-:cnd:noEmit
#if MODEL_ADDTEST 
//+:cnd:noEmit

using CQRS.Common.Interfaces;
using CQRS.Common.Services;
using CQRS.Common.Tests;
using CQRS.Common.Tests.Attributes;
using CQRS.Common.Contexts;
/*
* Yo can choose your own model to test by changing the using statements given beolow:
* For example:
* using TOutDTO = UserDefined.Models.ISubject;
* using TID = System.Int32;
* using TModel = UserDefined.Models.Subject;
* OR
* using TOutDTO = UserDefined.DTOs.ISubjectDTO;
* using TID = System.Int32;
* using TModel = UserDefined.Models.Subject;
* 
* Please note that TModel must be a concrete class deriving from the base Model class.
*/
//-:cnd:noEmit
#if !MODEL_USEDTO
using TOutDTO = CQRS.Common.Tests.TestModel;
using TID = System.Int32;
using TModel = CQRS.Common.Tests.TestModel;
#else
using TOutDTO = CQRS.Common.Tests.TestModelDTO;
using TID = System.Int32;
using TModel = CQRS.Common.Tests.TestModel;
#endif
//+:cnd:noEmit

namespace UserDefined.Tests
{
    [Testable]
    public class QueryTest : TestQuery<TOutDTO, TModel, TID>
    {
        protected override IQueryContract<TOutDTO, TModel, TID> CreateService()
        {
            return new QueryService<TOutDTO, TModel, TID, ModelContext>(new ModelContext());
        }
    }
}
//-:cnd:noEmit
#endif
//+:cnd:noEmit
