using Microsoft.AspNetCore.Identity;

namespace JoliDay.Config
{
    public class RoleCreator
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private static RoleCreator _instance;

        private RoleCreator(RoleManager<IdentityRole> roleManager)
        {
            this._roleManager = roleManager;
        }

        public static RoleCreator Instance(RoleManager<IdentityRole> roleManager)
        {
            return _instance ?? (_instance = new RoleCreator(roleManager));
        }

        public void CreateRole()
        {
            if (!_roleManager.RoleExistsAsync("Admin").Result)
            {
                string[] roles = { "Admin", "Vacationer" };
                foreach (var role in roles)
                {
                    _roleManager.CreateAsync(new IdentityRole(role)).Wait();
                }
            }
        }
    }
}