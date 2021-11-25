using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using two.Data;
using two.Models;

namespace two
{
    public class DGroupsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public DGroupsController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: DGroups
        [Authorize]
        public async Task<IActionResult> Index()
        {
            //Only show groups where user is member

            var userId = _userManager.GetUserId(User);

            var group = await _context.DGroups
                                        .Include(a => a.GroupMember)
                                        .Where(c => c.GroupMember.Any(b => b.Id == userId && b.GroupRoleEz == GroupRoleEz.Admin)).ToListAsync();


            return View(group);
        }
        // GET: Groups/Details/5
        [Authorize]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            // should only let them view if group member or is example
            if (!IsGrpMember((Guid)id))
            {
                return Content("You must be a member of the group to view details");
            }
            var group = await _context.DGroups
                .FirstOrDefaultAsync(m => m.DGroupId == id);
            if (group == null)
            {
                return NotFound();
            }

            return View(group);
        }

        /**      // GET: DGroups/Details/5
              [Authorize]
              public async Task<IActionResult> Details(Guid? id)
              {
                  if (id == null)
                  {
                      return NotFound();
                  }
                  // show members of the group
                  var dGroup = await _context.DGroups
                              .Include(a => a.GroupMember)
                              .ThenInclude(t => t.ApplicationUser)
                               .FirstOrDefaultAsync(g => g.DGroupId == id);
                  //  .Where(a => a.DGroupId == id).ToListAsync();
                  // (g => g.PostCode == "ABC" && g.Latitude > 10 && g.Longitude < 50)
                  if (dGroup == null)
                  {
                      return NotFound();
                  }

                  return View(dGroup);
              }
        **/
        // GET: Groups/Create
        [Authorize]
        public IActionResult Create()
        {// must be loged int to create a group
            return View();
        }

        // POST: Groups/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("DGroupId,GroupName,CreateDate,CreatedByUserId")] DGroup group)
        {
            //Any logged in user can Create a group
            //this puts signed in user as creator and as a member with admin rights
            var user = await _userManager.GetUserAsync(User);
            try
            {
                if (ModelState.IsValid)
                {
                    group.CreatedByUserId = user.Id; //should be Guids

                    group.CreateDate = DateTime.Now; //should be Guids
                    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                    {

                        _context.Add(group);
                        var MemberGrp = new GroupMember
                        {
                            DGroupId = group.DGroupId,
                            Id = user.Id,// user becomes a member
                            GroupRoleEz = GroupRoleEz.Admin,
                            EnrollmentDate = DateTime.Now,
                            EnrollmentByUserId = user.Id // user enroles themself
                        };

                        _context.Add(MemberGrp);

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists " +
                    "see your system administrator.");
            }
            return View(group);
        }


        // GET: DGroups/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (!IsAdmin((Guid)id))
            {
                return Content("You must be admin to edit group");
            }
            var dGroup = await _context.DGroups.FindAsync(id);
            if (dGroup == null)
            {
                return NotFound();
            }
            return View(dGroup);
        }

        // POST: DGroups/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("DGroupId,GroupName")] DGroup dGroup)
        {
            if (id != dGroup.DGroupId)
            {
                return NotFound();
            }

            if (!IsAdmin((Guid)id))
            {
                return Content("You must be admin to edit group");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dGroup);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DGroupExists(dGroup.DGroupId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(dGroup);
        }

        // GET: DGroups/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (!IsAdmin((Guid)id))
            {
                return Content("You must be admin to delete group");
            }
            var dGroup = await _context.DGroups
                .FirstOrDefaultAsync(m => m.DGroupId == id);
            if (dGroup == null)
            {
                return NotFound();
            }

            return View(dGroup);
        }

        // POST: DGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (!IsAdmin((Guid)id))
            {
                return Content("You must be admin to delete group");
            }
            var dGroup = await _context.DGroups.FindAsync(id);
            _context.DGroups.Remove(dGroup);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GroupExists(Guid id)
        {
            return _context.DGroups.Any(e => e.DGroupId == id);
        }
        private bool IsAdmin(Guid id)
        {
            var userId = _userManager.GetUserId(User);
            return _context.GroupMembers.Any(e => e.DGroupId == id && e.Id == userId.ToString() && e.GroupRoleEz == GroupRoleEz.Admin);
        }
        private bool IsEditor(Guid id)
        {
            var userId = _userManager.GetUserId(User);
            return _context.GroupMembers.Any(e => e.DGroupId == id && e.Id == userId.ToString() && e.GroupRoleEz == GroupRoleEz.Edit);
        }
        private bool IsGrpMember(Guid id)
        {
            var userId = _userManager.GetUserId(User);
            return _context.GroupMembers.Any(e => e.DGroupId == id && e.Id == userId.ToString()
            && (e.GroupRoleEz == GroupRoleEz.Edit || e.GroupRoleEz == GroupRoleEz.View || e.GroupRoleEz == GroupRoleEz.Admin));
        }

        private bool IsRead(Guid id)
        {
            var userId = _userManager.GetUserId(User);
            return _context.GroupMembers.Any(e => e.DGroupId == id && e.Id == userId.ToString() && e.GroupRoleEz == GroupRoleEz.View);
        }
        private bool DGroupExists(Guid id)
        {
            return _context.DGroups.Any(e => e.DGroupId == id);
        }
    }
}
