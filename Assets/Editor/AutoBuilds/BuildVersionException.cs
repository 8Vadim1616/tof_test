using Assets.Scripts.BuildSettings;
using System;
using UnityEditor;

class BuildVersionException : Exception
{
	public BuildVersionException(ServerTypeEnum serverType)
		: base($"Номер версии (<b>{PlayerSettings.bundleVersion}</b>) не соответствует типу сборки <b>{serverType}</b>") 
	{
	}
	public override string StackTrace => null;
}
