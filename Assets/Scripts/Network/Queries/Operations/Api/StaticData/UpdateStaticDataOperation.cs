using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Network.Queries.Operations.Api.StaticData
{
    public class UpdateStaticDataOperation : BaseApiOperation<UpdateStaticDataOperation.StaticDataRequest, UpdateStaticDataOperation.StaticDataResponse>
    {
        public UpdateStaticDataOperation()
        {
            var obj = new List<FileWithVersion>();

            foreach (var file in StaticDataFileName.ALL)
            {
				obj.Add(new FileWithVersion
				{
					Name = file,
					Ver = FileResourcesLoader.NoGroup().GetMax(file)
				});
			}

            SetRequestObject(new StaticDataRequest {Files = obj});
        }

        public class StaticDataRequest : BaseApiRequest
        {
            [JsonProperty("files")] public List<FileWithVersion> Files { get; set; }

            public StaticDataRequest() : base("model.rawfiles") { }
        }
        
		public class StaticDataResponse : BaseApiResponse, INotJsonResponse
		{
			public string ResponseText { get; set; }
	        
			public Dictionary<string, FileWithVersionData> Files;
			public ModelVersionData ModelData;

			public override void OnParsed(string originalJson)
			{
				base.OnParsed(originalJson);
				UpdateFromRawResponse(originalJson);
			}

			public static StaticDataResponse GetFromRawResponse(string data)
			{
				var result = new StaticDataResponse();
				result.UpdateFromRawResponse(data);
				return result;
			}

			public void UpdateFromRawResponse(string data)
			{
				ModelData = new ModelVersionData();
				Files = new Dictionary<string, FileWithVersionData>();

				var carriage = 0;
				var nextCarriage = 0;
				
				ParseModelData();

				while (carriage < data.Length)
					ParseNextFileData();

				void ParseModelData()
				{
					nextCarriage = data.IndexOf('\n', carriage);
					var verTimeGrp = data.Substring(carriage, nextCarriage - carriage);
					var split = verTimeGrp.Split(';');
					ModelData.Version = int.Parse(split[0]);
					ModelData.Time = long.Parse(split[1]);
					//todo нет группы???
					
					carriage = nextCarriage + 1;
				}

				void ParseNextFileData()
				{
					nextCarriage = data.IndexOf('\n', carriage);
					var nameVerLength = data.Substring(carriage, nextCarriage - carriage);
					carriage = nextCarriage + 1;

					var split = nameVerLength.Split(';');
					var name = split[0];
					var ver = int.Parse(split[1]);
					var length = int.Parse(split[2]);
					nextCarriage = carriage + length;
					var fileData = data.Substring(carriage, nextCarriage - carriage);
					carriage = nextCarriage;

					try
					{
						Files.Add(name, new FileWithVersionData
						{
							Data = fileData,
							Ver = ver,
						});
					}
					catch(Exception e)
					{
						Debug.LogError(e);
					}
				}
			}
		}
    }
}