using AccountManagement.Infrastructure.Exceptions;
using FluentValidation;
using MediatR;

namespace AccountManagement.Application.Common.Behavior
{
    public class ValidationBehavior<Trequest, Tresponse> : IPipelineBehavior<Trequest, Tresponse>
    {
        private readonly List<IValidator<Trequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<Trequest>> validators)
        {
            _validators = validators.ToList();
        }
        public async Task<Tresponse> Handle(Trequest request, RequestHandlerDelegate<Tresponse> next, CancellationToken cancellationToken)
        {
            var context = new ValidationContext<Trequest>(request);
            var failures = _validators
                .Select(v => v.Validate(context))
                .SelectMany(result => result.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                throw new CustomValidationException(failures);
            }

            return await next();
        }
    }
}
