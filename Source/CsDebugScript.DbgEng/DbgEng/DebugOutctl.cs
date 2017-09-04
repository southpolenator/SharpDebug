using System;

namespace DbgEng
{
	public enum DebugOutctl
	{
		ThisClient,
		AllClients,
		AllOtherClients,
		Ignore,
		LogOnly,
		SendMask = 7,
		NotLogged,
		OverrideMask = 16,
		Dml = 32,
		AmbientDml = -2,
		AmbientText
	}
}
