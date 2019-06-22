using System;

namespace DbgEng
{
	public enum DebugExecute
	{
		Default,
		Echo,
		NotLogged_,
		NoRepeat = 4,
		UserTyped = 8,
		UserClicked = 16,
		Extension = 32,
		Internal = 64,
		Script = 128,
		Toolbar = 256,
		Menu = 512,
		Hotkey = 1024,
		Event = 2048
	}
}
