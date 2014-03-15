using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using log4net;

namespace PaynearmeCallbacks
{
	public class SignatureUtils
	{
		private static List<string> REJECT = new List<string> (new string[] { "signature" });

		private SignatureUtils ()
		{
		}

		public static string Signature (NameValueCollection dict, string secret)
		{
            ILog logger = LogManager.GetLogger(typeof(SignatureUtils));

			StringBuilder buffer = new StringBuilder ();
			List<string> keys = new List<string> (dict.AllKeys);
			keys.Sort ();
            logger.Debug("Signature Params: ");
			foreach (string key in keys) {
				if (!REJECT.Contains (key)) {
					buffer.Append (key)
						.Append (dict [key]);
                    logger.Debug("  " + key + ": " + dict[key]);
				}
			}
            logger.Debug("secret: '" + secret + "'");
			buffer.Append (secret);

            logger.Debug("Signing String: " + buffer.ToString());

			MD5 md5 = MD5.Create ();
			byte[] hash = md5.ComputeHash (Encoding.UTF8.GetBytes (buffer.ToString ()));
			StringBuilder hexdigest = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
			{
				hexdigest.Append(hash[i].ToString("x2"));
			}

            string sig = hexdigest.ToString();
            logger.Debug("Signature: " + sig);
			return hexdigest.ToString ();
		}
	}
}

