using Bygdrift.Warehouse.DataLake.CsvTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Module.Refine
{
    internal class GenericHelper
    {
        private Dictionary<object, int> attributeHeaders = new Dictionary<object, int>();
        private Dictionary<object, int> layerHeaders = new Dictionary<object, int>();

        public void AddAttributes(int r, XElement element, CsvSet csv, object[] excludedColNames = null)
        {
            int col;
            foreach (var atr in element.Attributes())
            {
                if (excludedColNames == null || Array.IndexOf(excludedColNames, atr.Name.ToString()) == -1)
                {
                    if (attributeHeaders.ContainsKey(atr.Name))
                        col = attributeHeaders[atr.Name];
                    else
                    {
                        col = csv.GetOrCreateHeader(atr.Name);
                        attributeHeaders.Add(atr.Name, col);
                    }
                    csv.AddRecord(col, r, atr.Value);
                }
            }
        }

        public void AddLayerDatas(int r, XElement estate, CsvSet csv)
        {
            int col;
            foreach (var layerData in estate.Elements("LayerData"))
            {
                var id = layerData.Attribute("ID").Value;
                var name = layerData.Attribute("Name").Value;
                var value = layerData.Attribute("Value").Value;

                if (!layerHeaders.ContainsKey(id))
                {
                    csv.AddHeader(name, out col);
                    layerHeaders.Add(id, col);
                }
                else
                    col = layerHeaders[id];

                csv.AddRecord(col, r, value);
            }
        }

        public static bool GetGravityPoint(List<(float Lat, float Lon)> coords, out (float Lat, float Lon) gps)
        {
            if (coords.Any())
            {
                var latMin = coords.Min(o => o.Lat);
                var latMax = coords.Max(o => o.Lat);
                var lonMin = coords.Min(o => o.Lon);
                var lonMax = coords.Max(o => o.Lon);
                var lat = (latMax - latMin) / 2 + latMin;
                var lon = (lonMax - lonMin) / 2 + lonMin;
                gps = (lat, lon);
                return true;
            }
            gps = default;
            return false;
        }
    }
}
