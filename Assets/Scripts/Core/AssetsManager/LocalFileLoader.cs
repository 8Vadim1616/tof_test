using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Assets.Scripts.Libraries.RSG;
using UnityEngine;
using Application = UnityEngine.Application;

namespace Assets.Scripts.Core.AssetsManager
{
    public class LocalFileLoader
    {
		private static string FOLDER = Application.persistentDataPath + "/" + "img_cache" + "/";
		public static Dictionary<string, Texture2D> TEXTURE_CACHE = new Dictionary<string, Texture2D>();
		public static Dictionary<string, Sprite> SPRITE_CACHE = new Dictionary<string, Sprite>();

        public IPromise<Sprite> LoadSpriteFrom(string path)
        {
            if (!File.Exists(path))
                return Promise<Sprite>.Resolved(null);

            byte[] array = File.ReadAllBytes(path);
            if (array.Length == 0)
                return Promise<Sprite>.Resolved(null);

            Texture2D texture = new Texture2D(2, 2);
            if (!texture.LoadImage(array))
                return Promise<Sprite>.Resolved(null);

            var rect = new Rect(0, 0, texture.width, texture.height);
            var sprite = Sprite.Create(texture, rect, Vector2.zero, 300f);
            return Promise<Sprite>.Resolved(sprite);
        }
        
        private Texture2D DuplicateTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        public void SaveSpriteTo(Sprite sprite, string localPath)
        {
            Texture2D tex = sprite.texture;

            // обходной путь для текстур с запретом на read/write
            // а так же для текстур с включенной компрессией
            tex = DuplicateTexture(tex);

            var array = tex.EncodeToPNG();
            if (array is null || array.Length == 0)
            {
                GameLogger.warning($"Texture '{sprite.name}' convert error");
                return;
            }

            var dir = Path.GetDirectoryName(localPath);
            if (dir != null)
                Directory.CreateDirectory(dir);
            File.WriteAllBytes(localPath, array);
        }

        public void DeleteFileFrom(string localPath)
        {
            if (!string.IsNullOrWhiteSpace(localPath) && File.Exists(localPath))
                File.Delete(localPath);
        }

		public static IPromise<Texture2D> GetTexture(string url, bool useCache = true)
		{
			var result = new Promise<Texture2D>();
			bool needSave = false;

			if (url == null)
			{
				GameLogger.debug("Url has not been set. Use 'load' funtion to set image url.");
				result.Resolve(null);
				return result;
			}

			try
			{
				Uri uri = new Uri(url);
				url = uri.AbsoluteUri;
			}
			catch (Exception ex)
			{
				GameLogger.debug("Url is not correct.");
				result.Resolve(null);
				return result;
			}

			var uniqueHash = CreateMD5(url);

			if (!useCache)
			{
				load();
				return result;
			}

			if (!Directory.Exists(FOLDER))
				Directory.CreateDirectory(FOLDER);

			if (TEXTURE_CACHE.TryGetValue(uniqueHash, out Texture2D cachedTexture))
			{
				OnHaveTexture(cachedTexture);
				return result;
			}

			if (File.Exists(FOLDER + uniqueHash))
			{
				byte[] fileData;
				fileData = File.ReadAllBytes(FOLDER + uniqueHash);
				var texture = new Texture2D(2, 2);
				texture.LoadImage(fileData);

				OnHaveTexture(texture);
			}
			else
			{
				needSave = true;
				load();
			}

			return result;

			void load()
			{
				AssetsManager.Instance?.Loader.LoadURLImage(url)
							 .Then(OnHaveTexture)
							 .Catch(result.Reject);
			}

			void OnHaveTexture(Texture2D texture)
			{
				if (needSave)
					File.WriteAllBytes(FOLDER + uniqueHash, texture.EncodeToPNG());

				result.Resolve(texture);
			}
		}

		public static IPromise<Sprite> GetSprite(string url, bool useCache = true)
		{
			var result = new Promise<Sprite>();

			if (!useCache)
			{
				load();
				return result;
			}

			var uniqueHash = CreateMD5(url);

			if (SPRITE_CACHE.TryGetValue(uniqueHash, out Sprite sprite))
			{
				result.Resolve(sprite);
				return result;
			}

			load();

			return result;

			void load()
			{
				GetTexture(url, useCache)
							   .Then(OnHaveTexture)
							   .Catch(result.Reject);
			}
			void OnHaveTexture(Texture2D texture)
			{
				if (texture == null)
				{
					result.Resolve(null);
					return;
				}

				var sprite = texture.width == 8 && texture.height == 8 ? null : Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

				result.Resolve(sprite);
			}
		}

		public static string CreateMD5(string input)
		{
			// Use input string to calculate MD5 hash
			using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
			{
				byte[] inputBytes = Encoding.ASCII.GetBytes(input);
				byte[] hashBytes = md5.ComputeHash(inputBytes);

				// Convert the byte array to hexadecimal string
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < hashBytes.Length; i++)
				{
					sb.Append(hashBytes[i].ToString("X2"));
				}
				return sb.ToString();
			}
		}
    }
}