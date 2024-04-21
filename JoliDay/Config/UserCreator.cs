using System.Diagnostics;
using JoliDay.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Activity = System.Diagnostics.Activity;

namespace JoliDay.Config;

public class UserCreator
{
    private readonly UserManager<User> _userManager;
    private static UserCreator? _instance;
    private readonly JoliDayContext _dbContext;

    private UserCreator(UserManager<User> userManager, JoliDayContext dbContext)
    {
        this._userManager = userManager;
        this._dbContext = dbContext;
    }

    public static UserCreator Instance(UserManager<User> userManager, JoliDayContext dbContext)
    {
        return _instance ??= new UserCreator(userManager, dbContext);
    }

    public void CreateUsers()
    {
        if (_userManager.Users.Any())
        {
            return;
        }

        //Un utilisateur propriétaire d'une période de vacances
        var owner = new User
        {
            FirstName = "Satoru",
            Name = "Gojo",
            UserName = "InfiniteSpace",
            Email = "s.gojo@gmail.com",
            NormalizedEmail = "s.gojo@gmail.com"
        };
        //Un utilisateur membre d'une période de vacances
        var member = new User
        {
            FirstName = "Yuji",
            Name = "Itadori",
            UserName = "Sukuna",
            Email = "y.itadori@gmail.com",
            NormalizedEmail = "y.itadori@gmail.com"
        };
        //Un utilisateur invité dans une période de vacances
        var invited = new User
        {
            FirstName = "Kento",
            Name = "Nanami",
            UserName = "Binding",
            Email = "k.nanami@gmail.com",
            NormalizedEmail = "k.nanami@gmail.com"
        };
       
        _userManager.CreateAsync(owner, "Test1234$").Wait();
        _userManager.AddToRoleAsync(owner, "Vacationer").Wait();

        _userManager.CreateAsync(member, "Test1234$").Wait();
        _userManager.AddToRoleAsync(member, "Vacationer").Wait();

        _userManager.CreateAsync(invited, "Test1234$").Wait();
        _userManager.AddToRoleAsync(invited, "Vacationer").Wait();

        var now = DateTime.Today;
        var oneMonthLater = new DateTime(now.Year + 1, 1, now.Day);
        if (now.Month < 12)
        {
            oneMonthLater = new DateTime(now.Year, now.Month + 1, now.Day);
        }

        var insertedHoliday = _dbContext.Holidays.AddAsync(new Holiday
        {
            Address = new Address
            {
                StreetNumber = "16",
                StreetName = "Rue de Harlez",
                City = "Liège",
                Country = "Belgique",
                PostalCode = "4000"
            },
            Name = "Vacances à HELMo",
            Owner = owner,
            Users = new List<User> { member },
            StartDate = now,
            EndDate = oneMonthLater,
        }).Result.Entity;

        _dbContext.SaveChangesAsync().Wait();

        var result = _dbContext.Invites.AddAsync(new Invite
        {
            Holiday = insertedHoliday,
            User = invited,
        }).Result;
        _dbContext.SaveChangesAsync().Wait();
    }
}