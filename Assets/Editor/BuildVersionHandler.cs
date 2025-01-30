using System;

public struct BuildVersionHandler
{
	private const string DELIMITER = ".";

	public int Major;
	public int Minor;
	public int Build;

	public BuildVersionHandler(string bandleVersionString)
	{
		var array = bandleVersionString.Split(new[] { DELIMITER }, StringSplitOptions.RemoveEmptyEntries);
		Major = int.Parse(array[0]);
		Minor = int.Parse(array[1]);
		Build = int.Parse(array[2]);
	}

	public override string ToString()
	{
		return string.Join(DELIMITER, Major, Minor, Build);
	}
}
