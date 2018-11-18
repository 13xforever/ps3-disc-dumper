using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IrdLibraryClient.POCOs
{
    public class SearchResult
    {
        public List<SearchResultItem> Data;
        public int Draw;

        [JsonProperty(PropertyName = "recordsFiltered")]
        public int RecordsFiltered;

        [JsonProperty(PropertyName = "recordsTotal")]
        public int RecordsTotal;
    }

    public class SearchResultItem
    {
        public string Id; // product code
        public string AppVersion;
        public string GameVersion;
        public string UpdateVersion;
        public DateTime? Date;
        public string Title; // <span class="text-success glyphicon glyphicon-ok-sign"></span> MLB® 15 The Show™
        public string Filename; // <a class="btn btn-primary btn-xs" href="ird/BCUS00236-7ECC6C2A9C12DABB875342DFF80E9A97.ird">Download</a>\r\n                <a class="btn btn-info btn-xs" href="info.php?file=ird/BCUS00236-7ECC6C2A9C12DABB875342DFF80E9A97.ird"><span class="glyphicon glyphicon-info-sign" ></span></a>
        public string State; //always 1?
    }
}
