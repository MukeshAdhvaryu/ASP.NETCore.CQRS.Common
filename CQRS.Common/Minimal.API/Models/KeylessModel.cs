/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/
//-:cnd:noEmit
#if !TDD
//+:cnd:noEmit

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CQRS.Common.Models
{
    [Keyless]
    public abstract class KeylessModel<TModel>: Model<TModel>, IEntityTypeConfiguration<TModel>
        where TModel : Model<TModel>
    {
        int PsuedoID;

        void IEntityTypeConfiguration<TModel>.Configure(EntityTypeBuilder<TModel> builder)
        {
            Configure(builder);
            builder.HasKey("PsuedoID");
        }
    }
}
//-:cnd:noEmit
#endif
//+:cnd:noEmit
