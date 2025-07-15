using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Azuro.Common.Validation
{
	/// <summary>
	/// Define an extended ValidatableObject of type T for DI support
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IExtendedValidatableObject<T> : IValidatableObject { }

	/// <summary>
	/// This class can be inherited by DTO types to call the implicit implementation of the validation code relavent to the type specified by T.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class ExtendedValidatableObjectBase<T> : IExtendedValidatableObject<T> where T : class
	{
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			return (validationContext == null || validationContext.ObjectInstance is not T value)
				? throw new ArgumentNullException(nameof(validationContext), $"{nameof(Validate)}: ValidationContext parameter cannot be null.")
				: Validate(value);
		}

		protected abstract IEnumerable<ValidationResult> Validate(T value);
	}
}