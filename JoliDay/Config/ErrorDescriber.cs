using Microsoft.AspNetCore.Identity;

namespace JoliDay.Config
{
    public class ErrorDescriber : IdentityErrorDescriber
    {

        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError()
            {
                Code= nameof(PasswordRequiresDigit),
                Description = "Les mots de passe doivent comporter au moins un chiffre ('0' - '9')."
            };
        }

        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError()
            {
                Code = nameof(PasswordRequiresLower),
                Description = "Les mots de passe doivent comporter au moins une minuscule ('a' - 'z')."
            };
        }

        public override IdentityError PasswordRequiresUpper() {
            return new IdentityError()
            {
                Code= nameof(PasswordRequiresUpper),
                Description = "Les mots de passe doivent comporter au moins une majuscule ('A' - 'Z')."
            };
        }

        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError()
            {
                Code = nameof(PasswordRequiresNonAlphanumeric),
                Description = "Les mots de passe doivent comporter au moins un caractère non alphanumérique."
            };
        }


        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError()
            {
                Code = nameof(DuplicateEmail),
                Description = "Email indisponible"
            };
        }

        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError()
            {
                Code = nameof(PasswordTooShort),
                Description = $"Les mots de passe doivent comporter au moins {length} caractères."
            };
        }
    }
}
