/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/
//-:cnd:noEmit
#if !TDD  
//+:cnd:noEmit
namespace CQRS.Common.Services
{
    #region IExContract
    /// <summary>
    /// This interface represents an internal contract of operations.
    /// </summary>
    partial interface IExContract
    { 
        void Configure(IEndpointRouteBuilder app); 
    }
    #endregion

    partial class Contract: IExContract
    {
        protected abstract void Configure(IEndpointRouteBuilder app);
        void IExContract.Configure(IEndpointRouteBuilder app) =>
            Configure(app);
    }
}
//-:cnd:noEmit
#endif 
//+:cnd:noEmit
