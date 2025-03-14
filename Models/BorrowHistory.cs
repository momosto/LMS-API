//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LMS_API.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class BorrowHistory
    {
        public int Id { get; set; }
        public Nullable<int> StudentId { get; set; }
        public Nullable<int> BookId { get; set; }
        public Nullable<System.DateTime> BorrowDate { get; set; }
        public Nullable<System.DateTime> ReturnDate { get; set; }
        public string Status { get; set; }
    
        public virtual Book Book { get; set; }
        public virtual Student Student { get; set; }
    }
}
