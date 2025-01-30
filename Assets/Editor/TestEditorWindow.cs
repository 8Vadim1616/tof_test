using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Editor1
{
	public class TestEditorWindow : EditorWindow
	{
		[MenuItem("Test/Show Test Window")]
		private static void ShowWindow() => GetWindow<TestEditorWindow>().Show();

		private static string Key;
		private static string IV;
		
		private void OnGUI()
		{
			Key = EditorGUILayout.TextField("Key:", Key);
			IV = EditorGUILayout.TextField("IV:", IV);

			if (GUILayout.Button("Try to parse"))
			{
				TryToParse();
			}
		}
		
		static byte[] AES_Decrypt(byte[] cipherData, byte[] Key, byte[] IV)
		{
			try
			{
				byte[] decryptedData = null;
				using (MemoryStream ms = new MemoryStream())
				{
					using (Rijndael alg = Rijndael.Create())
					{
						alg.Key = Key;
						alg.IV = IV;
						using (CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write))
						{
							cs.Write(cipherData, 0, cipherData.Length);
							cs.Close();
						}
						decryptedData = ms.ToArray();
					}
				}
				return decryptedData;
			}
			catch
			{
				throw new Exception("Incorrect password or corrupted context.");
			}
		}
		
		[MenuItem("Test/TryToParse", false, 10000)]
		static void TryToParse()
		{
			var bytes = File.ReadAllBytes("C:/Users/k0syak/Desktop/download.dat");
			var key = new List<byte>();
			key.Add(item:  38);
			key.Add(item:  92);
			key.Add(item:  34);
			key.Add(item:  62);
			key.Add(item:  65);
			key.Add(item:  84);
			key.Add(item:  90);
			key.Add(item:  101);
			key.Add(item:  105);
			key.Add(item:  82);
			key.Add(item:  82);
			key.Add(item:  85);
			key.Add(item:  82);
			key.Add(item:  74);
			key.Add(item:  75);
			key.Add(item:  23);
			
			Debug.Log(string.Join("", key.Select(b => (char) b)));
		   byte[] key2 = Encoding.UTF8.GetBytes("rKJpsM<:OPd4*id)");  
		   byte[] key3 = Encoding.UTF8.GetBytes("n$k~9@kf{cDdsj,S");
		   
		   //AES_Decrypt(bytes, key.ToArray(), null);

		   var ps = new PasswordDeriveBytes(key.ToArray(), null, "SHA1", 2);

		   var allModes = Enum.GetValues(typeof(CipherMode));
		   var allPaddings = Enum.GetValues(typeof(PaddingMode));

		   var keys = new List<List<byte>>();
		   keys.Add(key);
		   keys.Add(key2.ToList());
		   keys.Add(key3.ToList());
		   keys.Add(key.ToList().Concat(key2).ToList());
		   keys.Add(key.ToList().Concat(key3).ToList());
		   keys.Add(key2.ToList().Concat(key).ToList());
		   keys.Add(key3.ToList().Concat(key).ToList());
		   keys.Add(key3.ToList().Concat(key2).ToList());
		   keys.Add(key2.ToList().Concat(key3).ToList());


		   var vs = keys.ToList();

		   foreach (var v in vs)
		   {
			   foreach (var k in keys)
			   {
				   foreach (var type in allModes)
				   {
					   foreach (var padding in allPaddings)
					   {
						   using (Aes myAes = Aes.Create())
						   {
							   try
							   {
								   myAes.Key = k.ToArray();
								   myAes.Mode = (CipherMode) type;
								   myAes.Padding = (PaddingMode) padding;
								   myAes.IV = v.ToArray();
							   
								   using (MemoryStream msDecrypt = new MemoryStream(bytes))
								   {
									   var decryptor = myAes.CreateDecryptor(myAes.Key, myAes.IV);

									   using (CryptoStream csDecrypt =
											  new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
									   {
										   using (StreamReader srDecrypt = new StreamReader(csDecrypt))
										   {

											   var plaintext = srDecrypt.ReadToEnd();
											   Debug.Log("[" + myAes.Padding + " " + myAes.Mode + "] : " + plaintext);
										   }
									   }
								   }
							   }
							   catch (Exception ex)
							   {
								   Debug.LogError(ex);
							   }
						   }
					   }
				   }
			   }
		   }
		}
	}
}