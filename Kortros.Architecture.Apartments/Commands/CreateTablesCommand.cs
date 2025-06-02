using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace Kortros.Architecture.Apartments.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateTablesCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Application app = commandData.Application.Application;

            StringBuilder sb = new StringBuilder();

            #region MAIN TABLE
            //Create Rooms_Shedule
            using (Transaction t = new Transaction(doc, "CreateShedule"))
            {
                t.Start();
                ViewSchedule newShedule = ViewSchedule.CreateSchedule(doc, new ElementId(BuiltInCategory.OST_Rooms));
                try
                {
                    newShedule.Name = "_ПОМЕЩЕНИЯ";
                    newShedule.HasStripedRows = true;
                    ScheduleDefinition sheduleDefinition = newShedule.Definition;

                    //Add Feilds
                    foreach (var sField in sheduleDefinition.GetSchedulableFields())
                    {
                        if (sField.GetName(doc) == "Уровень")
                        {
                            ScheduleField roomLevelField = sheduleDefinition.AddField(sField);
                            ScheduleSortGroupField levelSort = new ScheduleSortGroupField(roomLevelField.FieldId, ScheduleSortOrder.Ascending)
                            {
                                ShowHeader = true
                            };
                            sheduleDefinition.AddSortGroupField(levelSort);
                        }
                    }
                    foreach (var sField in sheduleDefinition.GetSchedulableFields())
                    {
                        if (sField.GetName(doc) == "ADSK_Этаж")
                        {
                            var roomAdskLevelField = sheduleDefinition.AddField(sField);
                        }
                    }

                    ScheduleField roomCommentField = sheduleDefinition.AddField(
                        ScheduleFieldType.Instance,
                        new ElementId(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS));
                    ScheduleSortGroupField commentSort = new ScheduleSortGroupField(roomCommentField.FieldId, ScheduleSortOrder.Ascending)
                    {
                        ShowHeader = true,
                        ShowFooter = true
                    };
                    sheduleDefinition.AddSortGroupField(commentSort);

                    foreach (var sField in sheduleDefinition.GetSchedulableFields())
                    {
                        if (sField.GetName(doc) == "ADSK_Номер квартиры")
                        {
                            var roomNumField = sheduleDefinition.AddField(sField);
                            ScheduleSortGroupField roomNumSort = new ScheduleSortGroupField(roomNumField.FieldId, ScheduleSortOrder.Ascending);
                            sheduleDefinition.AddSortGroupField(roomNumSort);
                        }
                    }
                    foreach (var sField in sheduleDefinition.GetSchedulableFields())
                    {
                        if (sField.GetName(doc) == "ADSK_Номер помещения квартиры")
                        {
                            var aptNumField = sheduleDefinition.AddField(sField);
                            ScheduleSortGroupField aptNumSort = new ScheduleSortGroupField(aptNumField.FieldId, ScheduleSortOrder.Ascending);
                            sheduleDefinition.AddSortGroupField(aptNumSort);
                        }
                    }

                    var roomNumberField = sheduleDefinition.AddField(
                        ScheduleFieldType.Instance,
                        new ElementId(BuiltInParameter.ROOM_NUMBER));
                    var roomNameField = sheduleDefinition.AddField(
                        ScheduleFieldType.Instance,
                        new ElementId(BuiltInParameter.ROOM_NAME));

                    foreach (var sField in sheduleDefinition.GetSchedulableFields())
                    {
                        if (sField.GetName(doc) == "Площадь")
                        {
                            ScheduleField roomAreaField = sheduleDefinition.AddField(sField);
                            roomAreaField.DisplayType = ScheduleFieldDisplayType.Totals;
                        }
                    }

                    // table newShedule
                    TableData tableData = newShedule.GetTableData();
                    TableSectionData headerSectionData = tableData.GetSectionData(SectionType.Header);
                    headerSectionData.InsertRow(1);
                    headerSectionData.SetCellText(1, 0, "Необходимо, чтобы параметры Уровень и Этаж были связаны и значения не были разные." +
                        "\r\nЕсли на этаже предполагается несколько вариантов компановки - то в поле \"Комментарий\" необходимо вписать \"Вариант NN\", где N - номер варианта" +
                        "\r\nВ обратном случае поле должно быть пустым" +
                        "\r\nНе рекомендуется изменять сортировку и фильтры в таблице, для других целей рекомендуется скопировать таблицу и править копию");
                    headerSectionData.SetRowHeight(1, 0.082021);

                    t.Commit();
                    sb.AppendLine("Таблица _ПОМЕЩЕНИЯ создана");
                }
                catch (Exception ex)
                {
                    t.RollBack();
                    sb.AppendLine("Таблица _ПОМЕЩЕНИЯ не была создана, тк она либо существует, либо произошла ошибка");
                    //LOG DATA OR MESSAGEBOX
                }
            }
            #endregion

            #region BHC TABLE
            //Create AREA BHC_Shedule
            using (Transaction t = new Transaction(doc, "CreateShedule"))
            {
                t.Start();
                var items = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_AreaSchemes).ToElements().Where(i => i.Name == "ВНС").ToList();
                try
                {
                    if (items.Any())
                    {
                        ViewSchedule newShedule = ViewSchedule.CreateSchedule(doc, new ElementId(BuiltInCategory.OST_Areas), items.First().Id) ;
                        newShedule.Name = "_ВНС";
                        newShedule.HasStripedRows = true;

                        ScheduleDefinition sheduleDefinition = newShedule.Definition;

                        foreach (var sField in sheduleDefinition.GetSchedulableFields())
                        {
                            if (sField.GetName(doc) == "Уровень")
                            {
                                ScheduleField roomLevelField = sheduleDefinition.AddField(sField);

                                ScheduleSortGroupField levelSort = new ScheduleSortGroupField(roomLevelField.FieldId, ScheduleSortOrder.Ascending);
                                sheduleDefinition.AddSortGroupField(levelSort);
                            }
                        }
                        foreach (var sField in sheduleDefinition.GetSchedulableFields())
                        {
                            if (sField.GetName(doc) == "Имя")
                            {
                                ScheduleField roomLevelField = sheduleDefinition.AddField(sField);
                            }
                        }
                        foreach (var sField in sheduleDefinition.GetSchedulableFields())
                        {
                            if (sField.GetName(doc) == "Площадь")
                            {
                                ScheduleField roomLevelField = sheduleDefinition.AddField(sField);
                            }
                        }
                        TableData tableData = newShedule.GetTableData();
                        TableSectionData headerSectionData = tableData.GetSectionData(SectionType.Header);
                        headerSectionData.InsertRow(1);
                        headerSectionData.SetCellText(1, 0, "Таблица, созданная автоматически. " +
                            "\r\nЕсли на этаже предполагается несколько вариантов компановки - то в поле \"Имя\" необходимо вписать \"Вариант NN\", где N - номер варианта");
                        headerSectionData.SetRowHeight(1, 0.082021);
                        headerSectionData.SetColumnWidth(0, 0.410105);
                        TableSectionData tableSectionData = tableData.GetSectionData(SectionType.Body);
                        tableSectionData.SetColumnWidth(0, 0.131234);
                        tableSectionData.SetColumnWidth(1, 0.19685);
                        tableSectionData.SetColumnWidth(2, 0.082021);

                        TableCellStyle cellStyle = new TableCellStyle()
                        {
                            TextColor = new Color(255, 0, 0),
                            FontHorizontalAlignment = HorizontalAlignmentStyle.Left
                        };
                        TableCellStyleOverrideOptions overrideOptions = new TableCellStyleOverrideOptions()
                        {
                            FontColor = true,
                            HorizontalAlignment = true
                        };
                        cellStyle.SetCellStyleOverrideOptions(overrideOptions);

                        headerSectionData.SetCellStyle(1,0, cellStyle);
                        sb.AppendLine("Таблица _ВНС создана");
                        t.Commit();
                    }
                    else
                    {
                        t.RollBack();
                        sb.AppendLine("Таблица _ВНС НЕ создана, т.к. отсутствует тип зоны с именем 'ВНС'");
                    }
                }
                catch (Exception ex)
                {
                    t.RollBack();
                    sb.AppendLine("Таблица _ВНС не была создана, тк она либо существует, либо произошла ошибка");
                }
            }
            #endregion

            #region ГНС_250 TABLE
            //Create AREA ГНС 250_Shedule
            using (Transaction t = new Transaction(doc, "CreateShedule"))
            {
                t.Start();
                var items = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_AreaSchemes).ToElements().Where(i => i.Name == "Всего по зданию").ToList();
                try
                {
                    if (items.Any())
                    {
                        ViewSchedule newShedule = ViewSchedule.CreateSchedule(doc, new ElementId(BuiltInCategory.OST_Areas), items.First().Id);
                        newShedule.Name = "_ГНС_250";
                        newShedule.HasStripedRows = true;

                        ScheduleDefinition sheduleDefinition = newShedule.Definition;

                        foreach (var sField in sheduleDefinition.GetSchedulableFields())
                        {
                            if (sField.GetName(doc) == "Уровень")
                            {
                                ScheduleField roomLevelField = sheduleDefinition.AddField(sField);

                                ScheduleSortGroupField levelSort = new ScheduleSortGroupField(roomLevelField.FieldId, ScheduleSortOrder.Ascending);
                                sheduleDefinition.AddSortGroupField(levelSort);
                            }
                        }
                        foreach (var sField in sheduleDefinition.GetSchedulableFields())
                        {
                            if (sField.GetName(doc) == "Имя")
                            {
                                ScheduleField roomLevelField = sheduleDefinition.AddField(sField);
                            }
                        }
                        foreach (var sField in sheduleDefinition.GetSchedulableFields())
                        {
                            if (sField.GetName(doc) == "Площадь")
                            {
                                ScheduleField roomLevelField = sheduleDefinition.AddField(sField);
                            }
                        }
                        TableData tableData = newShedule.GetTableData();
                        TableSectionData headerSectionData = tableData.GetSectionData(SectionType.Header);
                        headerSectionData.InsertRow(1);
                        headerSectionData.SetCellText(1, 0, "Таблица, созданная автоматически. " +
                            "\r\nЕсли на этаже предполагается несколько вариантов компановки - то в поле \"Имя\" необходимо вписать \"Вариант NN\", где N - номер варианта");
                        headerSectionData.SetRowHeight(1, 0.082021);
                        headerSectionData.SetColumnWidth(0, 0.410105);
                        TableSectionData tableSectionData = tableData.GetSectionData(SectionType.Body);
                        tableSectionData.SetColumnWidth(0, 0.131234);
                        tableSectionData.SetColumnWidth(1, 0.19685);
                        tableSectionData.SetColumnWidth(2, 0.082021);

                        TableCellStyle cellStyle = new TableCellStyle()
                        {
                            TextColor = new Color(255, 0, 0),
                            FontHorizontalAlignment = HorizontalAlignmentStyle.Left
                        };
                        TableCellStyleOverrideOptions overrideOptions = new TableCellStyleOverrideOptions()
                        {
                            FontColor = true,
                            HorizontalAlignment = true
                        };
                        cellStyle.SetCellStyleOverrideOptions(overrideOptions);

                        headerSectionData.SetCellStyle(1, 0, cellStyle);
                        sb.AppendLine("Таблица _ГНС_250 создана");
                        t.Commit();
                    }
                    else
                    {
                        t.RollBack();
                        sb.AppendLine("Таблица _ГНС_250 НЕ создана, т.к. отсутствует тип зоны с именем 'Всего по зданию'");
                    }
                }
                catch (Exception ex)
                {
                    t.RollBack();
                    sb.AppendLine("Таблица _ГНС_250 не была создана, тк она либо существует, либо произошла ошибка");
                }
            }
            #endregion

            #region ГНС_370 TABLE
            //Create AREA ГНС 370_Shedule
            using (Transaction t = new Transaction(doc, "CreateShedule"))
            {
                t.Start();
                var items = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_AreaSchemes).ToElements().Where(i => i.Name == "ГНС_370").ToList();
                try
                {
                    if (items.Any())
                    {
                        ViewSchedule newShedule = ViewSchedule.CreateSchedule(doc, new ElementId(BuiltInCategory.OST_Areas), items.First().Id);
                        newShedule.Name = "_ГНС_370";
                        newShedule.HasStripedRows = true;

                        ScheduleDefinition sheduleDefinition = newShedule.Definition;

                        foreach (var sField in sheduleDefinition.GetSchedulableFields())
                        {
                            if (sField.GetName(doc) == "Уровень")
                            {
                                ScheduleField roomLevelField = sheduleDefinition.AddField(sField);

                                ScheduleSortGroupField levelSort = new ScheduleSortGroupField(roomLevelField.FieldId, ScheduleSortOrder.Ascending);
                                sheduleDefinition.AddSortGroupField(levelSort);
                            }
                        }
                        foreach (var sField in sheduleDefinition.GetSchedulableFields())
                        {
                            if (sField.GetName(doc) == "Имя")
                            {
                                ScheduleField roomLevelField = sheduleDefinition.AddField(sField);
                            }
                        }
                        foreach (var sField in sheduleDefinition.GetSchedulableFields())
                        {
                            if (sField.GetName(doc) == "Площадь")
                            {
                                ScheduleField roomLevelField = sheduleDefinition.AddField(sField);
                            }
                        }
                        TableData tableData = newShedule.GetTableData();
                        TableSectionData headerSectionData = tableData.GetSectionData(SectionType.Header);
                        headerSectionData.InsertRow(1);
                        headerSectionData.SetCellText(1, 0, "Таблица, созданная автоматически. " +
                            "\r\nЕсли на этаже предполагается несколько вариантов компановки - то в поле \"Имя\" необходимо вписать \"Вариант NN\", где N - номер варианта");
                        headerSectionData.SetRowHeight(1, 0.082021);
                        headerSectionData.SetColumnWidth(0, 0.410105);
                        TableSectionData tableSectionData = tableData.GetSectionData(SectionType.Body);
                        tableSectionData.SetColumnWidth(0, 0.131234);
                        tableSectionData.SetColumnWidth(1, 0.19685);
                        tableSectionData.SetColumnWidth(2, 0.082021);

                        TableCellStyle cellStyle = new TableCellStyle()
                        {
                            TextColor = new Color(255, 0, 0),
                            FontHorizontalAlignment = HorizontalAlignmentStyle.Left
                        };
                        TableCellStyleOverrideOptions overrideOptions = new TableCellStyleOverrideOptions()
                        {
                            FontColor = true,
                            HorizontalAlignment = true
                        };
                        cellStyle.SetCellStyleOverrideOptions(overrideOptions);

                        headerSectionData.SetCellStyle(1, 0, cellStyle);
                        sb.AppendLine("Таблица _ГНС_370 создана");
                        t.Commit();
                    }
                    else
                    {
                        t.RollBack();
                        sb.AppendLine("Таблица _ГНС_370 НЕ создана, т.к. отсутствует тип зоны с именем 'ГНС_370'");
                    }
                }
                catch (Exception ex)
                {
                    t.RollBack();
                    sb.AppendLine("Таблица _ГНС_370 не была создана, тк она либо существует, либо произошла ошибка");
                }
            }
            #endregion

            MessageBox.Show(sb.ToString());
            return Result.Succeeded;
        }
    }
}
