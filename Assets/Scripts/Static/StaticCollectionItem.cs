﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Assets.Scripts.Static
{
    public class StaticCollectionItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
