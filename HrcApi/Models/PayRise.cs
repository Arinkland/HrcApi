//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace HrcApi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class PayRise
    {
        public int PayRiseID { get; set; }
        public Nullable<int> UserID { get; set; }
        public Nullable<decimal> PayRiseMoney { get; set; }
        public string Reason { get; set; }
        public Nullable<System.DateTime> ApplicationTime { get; set; }
        public string ApprovalContent { get; set; }
        public Nullable<int> ApprovalState { get; set; }
        public Nullable<System.DateTime> ApprovalTime { get; set; }
    
        public virtual UserInfo UserInfo { get; set; }
    }
}
