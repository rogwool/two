using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace two.Models
{

    public enum GroupRoleEz
    {
        // GroupRoleEz is a field type and a field name in GroupMember
        // this enum trys to save with integer value, are converted by the field in 
        // ApplicationDbContext  .HasConversion(v => v.ToString(), v => (GroupRoleEz) Enum.Parse(typeof(GroupRoleEz), v));

        Admin,
        Edit,
        View
    }

    public class DGroup
    {
        [Key]
        public Guid DGroupId { get; set; }   
        public string GroupName { get; set; }
        [DataType(DataType.Date)]
        //  [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CreateDate { get; set; }
        public string CreatedByUserId { get; set; }
        public ICollection<GroupMember>? GroupMember { get; set; } //note state   [ForeignKey("RelatedEntity_Id")] need in table with many


    }

    public class ApplicationUser: IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public virtual ICollection<GroupMember>? GroupMember { get; set; }
    }
}
