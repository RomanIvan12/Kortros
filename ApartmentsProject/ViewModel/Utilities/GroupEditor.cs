using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace ApartmentsProject.ViewModel.Utilities
{
    public class GroupEditor
    {
        private static Schema _schema;
        private static Field _groupNameField, _pinnedField, _locationPointField, _groupIdField, _membersField;

        private readonly Document _doc;
        private Group _group;

        private Entity _storageEntity;
        private DataStorage _dataStorage;

        private readonly string _groupName;
        private readonly bool _pinned;
        private readonly XYZ _locationPoint;
        private readonly ElementId _groupId;
        private IList<ElementId> _members;
        public GroupEditor(Group group)
        {
            _doc = group.Document;
            _group = group;
            _groupName = group.Name;

            GetDataStorageMembers();
            if (_storageEntity != null)
                throw new ArgumentException(
                    $"Another instance of a the GroupType \"{_groupName}\" is already being edited");

            _pinned = group.Pinned;
            _locationPoint = (group.Location as LocationPoint).Point;
            _groupId = group.Id;
            _members = group.GetMemberIds();

        }
        #region Start/Finish Editing
        public void StartEditing()
        {
            if (_group == null)
                throw new InvalidOperationException("Group is already being edited");
            if (_dataStorage != null)
                throw new InvalidOperationException($"Another instance of \"{_groupName}\" is already being edited");

            SetDataStorageFields(_groupName, _pinned, _locationPoint, _groupId, _members);

            var groupType = _group.GroupType;
            bool groupTypeMustBeDeleted = groupType.Groups.Size == 1;
            _members = (IList<ElementId>)_group.UngroupMembers();
            if (groupTypeMustBeDeleted)
                _doc.Delete(groupType.Id);

            _group = null;
        }

        public Group FinishEditing()
        {
            if (!GroupElements().Any())
                throw new InvalidOperationException(
                    "None of the elements listed as group members is valid.\\n\\nPerhaps they have been deleted?");

            _group = _doc.Create.NewGroup(_members);

            // Find other instances of the same GroupType and update them
            var oldGroupType = new FilteredElementCollector(_doc)
                .OfClass(typeof(GroupType))
                .Cast<GroupType>()
                .FirstOrDefault(group => group.Name == _groupName);

            if (oldGroupType != null)
            {
                // testing against the group.Id is required because calling both Group.UngroupMembers and
                // GroupType.Groups in the same transaction causes GroupType.Groups to return the ungrouped group.
                // See https://forums.autodesk.com/t5/revit-api-forum/why-does-grouptype-groups-contain-ungrouped-groups/m-p/10292162
                foreach (Group group in oldGroupType.Groups)
                {
                    if (group.Id == _groupId)
                        continue;

                    group.GroupType = _group.GroupType;
                    group.Location.Move((_group.Location as LocationPoint).Point - _locationPoint);
                }

                _doc.Delete(oldGroupType.Id);
            }

            _group.GroupType.Name = _groupName;
            _group.Pinned = _pinned;

            _doc.Delete(_dataStorage.Id);
            _dataStorage = null;
            _storageEntity = null;

            return _group;
        }
        #endregion


        #region Collectors
        public IEnumerable<Element> GroupElements()
        {
            return _members
                .Select(elementId => _doc.GetElement(elementId))
                .Where(element => element != null)
                .ToList();
        }
        #endregion

        #region Schema and DataStorage managment
        private void GetDataStorageMembers()
        {
            GetSchema();

            var dataStorages = new FilteredElementCollector(_doc).OfClass(typeof(DataStorage));
            //MessageBox.Show(dataStorages.Count().ToString());
            foreach (var element in dataStorages)
            {
                var entity = element.GetEntity(_schema);
                //Functions.ShowProperties(entity);
                if (entity.IsValid() && entity.Get<string>(_groupNameField) == _groupName)
                {
                    _storageEntity = entity;
                    _dataStorage = element as DataStorage;
                    Functions.ShowProperties(entity);
                    return;
                }
            }

        }

        private void SetDataStorageFields(string groupName = null, bool? pinned = null, XYZ localPoint = null,
            ElementId groupId = null, IList<ElementId> members = null)
        {
            if (_dataStorage == null)
            {
                _dataStorage = DataStorage.Create(_doc);
                _storageEntity = new Entity(_schema);
            }

            if (groupName != null)
                _storageEntity.Set("GroupName", groupName);
            if (pinned != null)
                _storageEntity.Set("Pinned", (bool)pinned);
            if (localPoint != null)
                _storageEntity.Set("LocationPoint", localPoint, UnitTypeId.Meters);
            if (groupId != null)
                _storageEntity.Set("GroupId", groupId);
            if (members != null)
                _storageEntity.Set("Members", (IList<ElementId>) members.Distinct().ToList());

            _dataStorage.SetEntity(_storageEntity);
        }

        private static void GetSchema()
        {
            if (_schema != null)
                return;
            Guid schemaGuid = new Guid("86BF617A-CB0C-41AC-8A41-C3D268C9A1B0");
            _schema = Schema.Lookup(schemaGuid);

            if (_schema == null)
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(schemaGuid);
                schemaBuilder.SetSchemaName("GroupEditor");
                schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
                schemaBuilder.AddSimpleField("GroupName", typeof(string));
                schemaBuilder.AddSimpleField("Pinned", typeof(bool));
                schemaBuilder.AddSimpleField("LocationPoint", typeof(XYZ)).SetSpec(SpecTypeId.Length);
                schemaBuilder.AddSimpleField("GroupId", typeof(ElementId));
                schemaBuilder.AddArrayField("Members", typeof(ElementId));
                _schema = schemaBuilder.Finish();
            }

            _groupNameField = _schema.GetField("GroupName");
            _pinnedField = _schema.GetField("Pinned");
            _locationPointField = _schema.GetField("LocationPoint");
            _groupIdField = _schema.GetField("GroupId");
            _membersField = _schema.GetField("Members");
        }
        #endregion
    }
}
