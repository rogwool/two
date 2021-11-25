using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using two.Data;
using two.Models;
using Microsoft.AspNetCore.Authorization;


namespace two
{
    public class GroupMembersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GroupMembersController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: GroupMembers
        [Authorize]
        
        public async Task<IActionResult> Index()
        {
            string userId = _userManager.GetUserId(User);// needs 
            // get groups where Im admin
            var ImAdmin = (from gm in _context.GroupMembers
                           where (gm.GroupRoleEz == GroupRoleEz.Admin) && gm.Id == userId
                           //join g in _context.DGroups on gm.DGroupId equals g.DGroupId
                           select gm);


            // now get all users where ImAdmin , the GroupName and user details
            var GetMyMembers = (from a in ImAdmin
                                join gm in _context.GroupMembers on a.DGroupId equals gm.DGroupId
                                join g in _context.DGroups on gm.DGroupId equals g.DGroupId
                                join u in _context.Users on gm.Id equals u.Id
                                orderby g.DGroupId
                                select new GroupMember
                                {
                                    DGroupId = g.DGroupId,
                                    DGroup = g,
                                    EnrollmentByUserId = gm.EnrollmentByUserId,
                                    GroupRoleEz = gm.GroupRoleEz,
                                    EnrollmentDate = gm.EnrollmentDate,
                                    ApplicationUser = gm.ApplicationUser,
                                    Id = gm.Id
                                }
              );

            GetMyMembers.GroupBy(c => c.DGroupId);
            return View(await GetMyMembers.ToListAsync());
            /*            var applicationDbContext = _context.GroupMembers
                .Where(e => e.Id == userId.ToString() && e.GroupRoleEz == GroupRoleEz.Admin)
                .Include(g => g.DGroup)
                .Include(f => f.ApplicationUser);


            return View(await applicationDbContext.ToListAsync());*/

        }
    

        // GET: GroupMembers/Details/5
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupMember = await _context.GroupMembers
                .Include(g => g.DGroup)
                .Include(g => g.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (groupMember == null)
            {
                return NotFound();
            }

            return View(groupMember);
        }

        // GET: GroupMembers/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["DGroupId"] = new SelectList(_context.DGroups, "DGroupId", "DGroupId");
            ViewData["Id"] = new SelectList(_context.ApplicationUser, "Id", "Id");
            return View();
        }

        // POST: GroupMembers/Create
        [Authorize]
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DGroupId,GroupRole")] GroupMember groupMember)
        {
            if (ModelState.IsValid)
            {
                _context.Add(groupMember);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DGroupId"] = new SelectList(_context.DGroups, "DGroupId", "DGroupId", groupMember.DGroupId);
            ViewData["Id"] = new SelectList(_context.ApplicationUser, "Id", "Id", groupMember.Id);
            return View(groupMember);
        }

        // GET: GroupMembers/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(string id, Guid DGroupId)
        {
            if (id == null)
            {
                return NotFound();
            }
            

            var groupMember = await _context.GroupMembers.FindAsync(id, DGroupId);
            if (groupMember == null)
            {
                return NotFound();
            }
            ViewData["DGroupId"] = new SelectList(_context.DGroups, "DGroupId", "DGroupId", groupMember.DGroupId);
            ViewData["Id"] = new SelectList(_context.ApplicationUser, "Id", "Id", groupMember.Id);
            return View(groupMember);
        }

        // POST: GroupMembers/Edit/5
        
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,DGroupId,GroupRole")] GroupMember groupMember)
        {
            if (id != groupMember.Id)
            {
                return NotFound();
            }
            if (!IsAdmin(groupMember.DGroupId))
            {
                return Content("You must be admin of this group to Edit it.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(groupMember);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupMemberExists(groupMember.Id))
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
            ViewData["DGroupId"] = new SelectList(_context.DGroups, "DGroupId", "DGroupId", groupMember.DGroupId);
            ViewData["Id"] = new SelectList(_context.ApplicationUser, "Id", "Id", groupMember.Id);
            return View(groupMember);
        }
        [Authorize]
        // GET: GroupMembers/AddMemberToGroup
        public async Task<IActionResult> AddMemberToGroup(Guid DGroupId)
        { //allow adding members to groups by admin
            string userId = _userManager.GetUserId(User);

            //show all users
            ViewData["UserId"] = new SelectList(_context.ApplicationUser, "Id", "Email");

            // only show groups where user is admin
            ViewData["DGroupIdX"] = new SelectList(_context.GroupMembers.Where(e => e.Id == userId.ToString() && e.GroupRoleEz == GroupRoleEz.Admin).Include(b => b.ApplicationUser).Include(e => e.DGroup), "DGroupId", "DGroup.GroupName");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddMemberToGroup(GroupMember groupMember)
        //  public async Task<IActionResult> AddMemberToGroup([Bind("UserId,DGroupId,GroupRole,GroupRoleEz")] GroupMember groupMember)
        {
            string userId = _userManager.GetUserId(User);
            if (!IsAdmin(groupMember.DGroupId))
            {
                return Content("You must be admin of this group to Add a Member.");
            }
            groupMember.EnrollmentByUserId = userId;
            groupMember.EnrollmentDate = DateTime.Now;
            if (ModelState.IsValid)
            {

                _context.Add(groupMember);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. Duplicate entry.");
                }
                 return RedirectToAction(nameof(Index));
                }
            else { ModelState.AddModelError("", "Unable to save changes. Invalid Model state."); }
            ViewData["UserId"] = new SelectList(_context.ApplicationUser, "Id", "Email");
            ViewData["DGroupIdX"] = new SelectList(_context.GroupMembers.Where(e => e.Id == userId.ToString() && e.GroupRoleEz == GroupRoleEz.Admin), "DGroupId", "DGroup.GroupName");
            
            return RedirectToAction("Index", "GroupMembers");
        }
        [Authorize]
        // GET: GroupMembers/EditMemberRole/5
        public async Task<IActionResult> EditMemberRole(string id, Guid DGroupId)
        {
            if (id == null || DGroupId == null)
            {
                return Content("Details not found.");
            }

            // var groupMember  = _context.GroupMembers.Where(e => e.UserId == id && e.GroupRole == "Admin").Include(e => e.Group);
            var groupMember = await _context.GroupMembers
                     .Include(g => g.DGroup)
                         .AsNoTracking()
                         .FirstOrDefaultAsync(g => (g.Id == id) && (g.DGroupId == DGroupId));
            if (groupMember == null)
            {
                return Content("Details not found.");
            }
            ViewData["UserId"] = new SelectList(_context.ApplicationUser, "Id", "Id", groupMember.Id);

            //only show groups where ImAdmin
            string userId = _userManager.GetUserId(User);
            ViewData["DGroupId"] = new SelectList(_context.GroupMembers.Where(e => e.Id == userId.ToString() && e.GroupRoleEz == GroupRoleEz.Admin), "DGroupId", "DGroup.GroupName");

            return View(groupMember);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        //   public async Task<IActionResult> EditMemberRole(string id, [Bind("UserId,DGroupId,GroupRole,EnrollmentDate,EnrollmentByUserId")] GroupMember groupMember)
        public async Task<IActionResult> EditMemberRole(string id, GroupMember groupMember)

        {
            if (id != groupMember.Id)
            {
                return Content("Details not found for user in group.");
            }

            if (!IsAdmin(groupMember.DGroupId))
            {
                return Content("You must be admin of this group to edit member role.");
            }

            var GM = await _context.GroupMembers
                                    .Include(g => g.ApplicationUser)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(g => (g.Id == id) && (g.DGroupId == groupMember.DGroupId));
            //TryUpdateModelAsync from the original input only the field c.GroupRoleEz gets saved
            if (await TryUpdateModelAsync<GroupMember>(GM, "", c => c.GroupRoleEz))
            {

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(GM);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!GroupMemberExists(groupMember.Id))
                        {
                            return Content("Details not found.");
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                  
                }
            }
            ViewData["UserId"] = new SelectList(_context.ApplicationUser, "Id", "Email", groupMember.Id);
            string userId = _userManager.GetUserId(User);
            ViewData["DGroupId"] = new SelectList(_context.GroupMembers.Where(e => e.Id == userId.ToString() && e.GroupRoleEz == GroupRoleEz.Admin), "DGroupId", "DGroup.GroupName");
            return View(groupMember);
        }

        // GET: GroupMembers/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(string Id, Guid DGroupId)
        {
            if (Id == null || DGroupId == Guid.Empty)
            {
                return Content("Details not found.");
            }
            if (!IsAdmin(DGroupId))
            {
                return Content("You must be admin of this group to Delete a member.");
            }
            var groupMember = await _context.GroupMembers
                .Include(g => g.ApplicationUser)
                .Include(g => g.DGroup)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == Id && m.DGroupId == DGroupId);
            if (groupMember == null)
            {
                return Content("Details not found.");
            }
            return View(groupMember);
        }

        // POST: GroupMembers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(string id, Guid DGroupId)
        {
            if (id == null || DGroupId == Guid.Empty)
            {
                return Content("Details not found.");
            }
            if (!IsAdmin(DGroupId))
            {
                return Content("You must be admin of this group to Delete a member.");
            }
            var groupMember = await _context.GroupMembers.FindAsync(id, DGroupId);
            _context.GroupMembers.Remove(groupMember);
            await _context.SaveChangesAsync();
             return RedirectToAction(nameof(Index));
            //return RedirectToAction("Index", "DGroups");
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
            var user = _userManager.GetUserAsync(User);
            return _context.GroupMembers.Any(e => e.DGroupId == id && e.Id == user.Id.ToString()
            && (e.GroupRoleEz == GroupRoleEz.Edit || e.GroupRoleEz == GroupRoleEz.View || e.GroupRoleEz == GroupRoleEz.Admin));
        }

        private bool IsRead(Guid id)
        {
            var userId = _userManager.GetUserId(User);
            return _context.GroupMembers.Any(e => e.DGroupId == id && e.Id == userId.ToString() && e.GroupRoleEz == GroupRoleEz.View);
        }
        private bool GroupMemberExists(string id)
        {
            return _context.GroupMembers.Any(e => e.Id == id);
        }
    }
}
