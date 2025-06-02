using System;
using Autodesk.Revit.DB;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RevitServerExporter.Core;

namespace RevitServerExporter.Core
{
    public class ExportOptionsConverter
    {
        private const string EXPORT_VIEW_NAME = "Navisworks";

        public IFCExportOptions ComposeOptions(Document doc, Dictionary<string, string> convertSettings)
        {
            string activeViewId = string.Empty;

            var ifcOption = new IFCExportOptions
            {
                FileVersion = IFCVersion.IFC4RV
            };

            if (convertSettings.ContainsKey(RevitIntegration.COMMON_SETTINGS_NAME) &&
                !string.IsNullOrWhiteSpace(convertSettings[RevitIntegration.COMMON_SETTINGS_NAME]))
            {
                JsonTextReader reader =
                    new JsonTextReader(new StringReader(convertSettings[RevitIntegration.COMMON_SETTINGS_NAME]));
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        var optionName = reader.Value.ToString();
                        reader.Read();
                        var optionValue = reader.Value;

                        switch (optionName)
                        {
                            case "ActiveViewId":
                                activeViewId = optionValue.ToString();
                                break;
                            case "VisibleElementsOfCurrentView":
                                /*if (cadFarmView != null)
                                ifcOption.FilterViewId = cadFarmView.Id;*/
                                //ifcOption.AddOption(optionName, cadFarmView != null ? cadFarmView.Id.ToString() : optionValue.ToString());
                                break;
                            case "ActivePhaseId":
                                reader.Skip();
                                break;
                            case "IFCVersion":
                                ifcOption.FileVersion =
                                    (IFCVersion)Enum.Parse(typeof(IFCVersion), optionValue.ToString());
                                break;
                            default:
                                if (optionValue != null)
                                    ifcOption.AddOption(optionName, optionValue.ToString());
                                break;
                        }
                    }
                }
            }

            var cadFarmView = FindViewToExport(doc, activeViewId);
            if (cadFarmView != null)
            {
                ifcOption.FilterViewId = cadFarmView.Id;
                ifcOption.AddOption("ActiveViewId", cadFarmView.Id.ToString());
                ifcOption.AddOption("VisibleElementsOfCurrentView", "true");
                var currentPhase = cadFarmView.get_Parameter(BuiltInParameter.VIEW_PHASE);
                ifcOption.AddOption("ActivePhaseId", currentPhase.AsElementId().ToString());
            }

            return ifcOption;
        }

        private Element FindViewToExport(Document doc, string activeViewId)
        {
            FilteredElementCollector viewCollector = new FilteredElementCollector(doc);
            viewCollector.OfClass(typeof(View3D));
            var cadFarmView = viewCollector.FirstOrDefault(v => v.Id.ToString() == activeViewId || v.Name == activeViewId);

            return cadFarmView ?? viewCollector.FirstOrDefault(v => v.Name == EXPORT_VIEW_NAME);
        }
    }
}
