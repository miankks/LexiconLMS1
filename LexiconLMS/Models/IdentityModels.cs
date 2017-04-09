﻿using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LexiconLMS.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
        
        [Display(Name = "Förnamn")]
        public string FirstName { get; set; }
        [Display(Name = "Efternamn")]
        public string LastName { get; set; }
        [Display(Name = "Namn")]
        public string Fullname { get { return FirstName + " " + LastName; } }

        [Display(Name = "Kurs")]
        public int? CourseId { get; set; }
        [Display(Name = "Kurs")]
        public virtual Course Course {get;set;}

        public virtual ICollection<Document> Documents { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<LexiconLMS.Models.Course> Courses { get; set; }

        public System.Data.Entity.DbSet<LexiconLMS.Models.Module> Modules { get; set; }

        public System.Data.Entity.DbSet<LexiconLMS.Models.Activity> Activities { get; set; }

        public System.Data.Entity.DbSet<LexiconLMS.Models.ActivityType> ActivityTypes { get; set; }

        public System.Data.Entity.DbSet<LexiconLMS.Models.Document> Documents { get; set; }

        //public System.Data.Entity.DbSet<LexiconLMS.Models.ApplicationUser> ApplicationUsers { get; set; }

        //public System.Data.Entity.DbSet<LexiconLMS.Models.ApplicationUser> ApplicationUsers { get; set; }
    }
}