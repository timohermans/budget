    using FluentValidation;

    namespace Budget.App.Apis.LoginLogout;
        
    public class LoginModel
    {
        public required string Username { get; set; }
        public required string Password { get; set; }

        public class Validator : AbstractValidator<LoginModel>
        {
            public Validator()
            {
                RuleFor(x => x.Username).NotEmpty();
                RuleFor(x => x.Password).NotEmpty();
            }
        }
    }
