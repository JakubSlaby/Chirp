using System;
using UnityEngine;

namespace WhiteSparrow.Shared.Logging
{
	public abstract class AbstractLogChannelList : ScriptableObject
	{
		public abstract Type[] GetChannelList();
	}
}