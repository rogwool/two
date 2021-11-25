using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace two.Models
{
    public class GroupMember
    {

        [ForeignKey("Id")]
        public string Id { get; set; }

        [ForeignKey("DGroupId")]
        public Guid DGroupId { get; set; }
       public  GroupRoleEz GroupRoleEz { get; set; }
       
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EnrollmentDate { get; set; }
        public string EnrollmentByUserId { get; set; }
        public virtual DGroup DGroup { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

    }
}
