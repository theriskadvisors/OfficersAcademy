//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SEA_Application.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class AspNetLessonPlanBreakdown
    {
        public int Id { get; set; }
        public Nullable<int> LessonPlanID { get; set; }
        public Nullable<int> BreakDownHeadingID { get; set; }
        public string Description { get; set; }
        public Nullable<int> Minutes { get; set; }
        public string Resources { get; set; }
        public Nullable<int> Priority { get; set; }
    
        public virtual AspNetLessonPlan AspNetLessonPlan { get; set; }
        public virtual AspNetLessonPlanBreakdownHeading AspNetLessonPlanBreakdownHeading { get; set; }
    }
}
