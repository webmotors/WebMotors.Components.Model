using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace WebMotors.Components.Model.Core.Security
{
	public class Password
	{
		public Password()
		{

		}

		~Password()
		{

		}

		public static string createSaltOld(int tamanhoSalt)
		{
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			byte[] buff = new byte[tamanhoSalt];
			rng.GetBytes(buff);
			return Convert.ToBase64String(buff);
		}

		/// <summary>
		/// Gera um salt value usando a funcionalidade de geração de número randômico
		/// provida pela classeRNGCryptoServiceProvider que está dentro do namespace System.
		/// Security.Cryptography.
		/// </summary>
		/// <param name="tamanhoSalt"></param>
		public static string createSalt(int tamanhoSalt)
		{
			string retorno = "";
			string strAuxiliar = "ABCDEFGH3456789";
			Random rnd = new Random();
			for (int i = 0; i < tamanhoSalt; i++)
			{
				retorno += strAuxiliar.Substring(rnd.Next(strAuxiliar.Length - 1), 1);
			}
			return retorno;
		}

		public static string createHash(string password, string salt)
		{
			string saltAndPwd = string.Concat(password, salt);
			string hashedPwd = FormsAuthentication.HashPasswordForStoringInConfigFile(saltAndPwd, "SHA1");
			return hashedPwd;
		}
	}
}
