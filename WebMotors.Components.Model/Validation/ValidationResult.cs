using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebMotors.Components.Model.Validation
{
	[Serializable]
	public class ValidationResult
	{
		public List<string> Errors;
		public bool HasError
		{
			get { return Errors != null && Errors.Count > 0; }
		}

		public void Append(ValidationResult result)
		{
			if (Errors == null) Errors = new List<string>();
			if (result.HasError)
				foreach (var message in result.Errors)
					Errors.Add(DefaultMessage(message));
		}

		public void Append(string message)
		{
			if (Errors == null) Errors = new List<string>();
			Errors.Add(DefaultMessage(message));
		}

		string DefaultMessage(string message)
		{
			if (string.IsNullOrWhiteSpace(message))
				message = "Erro não definido no atributo.";
			return message;
		}
	}
}
