using System;
using System.Collections.Specialized;
using System.Web;

namespace PaynearmeCallbacks
{
	public interface IAuthorizationHandler
	{
		void HandleAuthorizationRequest(NameValueCollection parameters, HttpResponse response);
	}
}

