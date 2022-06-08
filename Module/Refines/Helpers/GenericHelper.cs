using Bygdrift.CsvTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Module.Refines.Helpers
{
    internal class GenericHelper
    {
        private Dictionary<object, int> attributeHeaders = new Dictionary<object, int>();
        private Dictionary<object, int> layerHeaders = new Dictionary<object, int>();

        public void AddAttributes(int r, XElement element, Csv csv, object[] excludedColNames = null)
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
                        csv.AddHeader(atr.Name.ToString(), false, out col);
                        attributeHeaders.Add(atr.Name, col);
                    }
                    csv.AddRecord(r, col, atr.Value);
                }
            }
        }

        public void AddLayerDatas(int r, XElement estate, Csv csv)
        {
            int col;
            foreach (var layerData in estate.Elements("LayerData"))
            {
                var id = layerData.Attribute("ID").Value;
                var name = layerData.Attribute("Name").Value;
                var value = layerData.Attribute("Value").Value;

                if (name != "LastDrawingChange")
                {
                    if (!layerHeaders.ContainsKey(id))
                    {
                        csv.AddHeader(name, out col);
                        layerHeaders.Add(id, col);
                    }
                    else
                        col = layerHeaders[id];

                    csv.AddRecord(r, col, value);
                }
            }
        }
    }
}
