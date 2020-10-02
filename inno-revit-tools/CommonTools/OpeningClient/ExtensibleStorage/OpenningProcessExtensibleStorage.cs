using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using CommonTools.OpeningClient.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CommonTools.OpeningClient.Support;

namespace CommonTools.OpeningClient.ExtensibleStorage
{
    public static class OpenningProcessExtensibleStorage
    {
        #region SetValue

        public static void SetValueToExtensibleStorage(ComparisonCoupleElement openningModel, Element openning, string subject)
        {
            Schema schema = DefineSchemaBuilder(subject);
            Entity entity = new Entity(schema);

            SetValueStringGeometryToExtensibleStorage(ref schema, ref entity, openningModel);
            openning.SetEntity(entity);
        }

        public static void SetValueStringGeometryToExtensibleStorage(ref Schema schema, ref Entity entity, ComparisonCoupleElement openningModel)
        {
            try {
                entity.Set<string>(schema.GetField(Define.CommonId), openningModel.IdManager);
                //entity.Set<string>(schema.GetField(Define.ElementId), openningModel.IdLocal);
                //entity.Set<string>(schema.GetField(Define.DrawingId), openningModel.IdDrawing);
                //entity.Set<string>(schema.GetField(Define.CreateDate), openningModel.CreateDate);
                //entity.Set<string>(schema.GetField(Define.UpdatedDate), openningModel.Geometry.UpdatedDate);
                if (openningModel.LocalStatus != null)
                    entity.Set<string>(schema.GetField(Define.ElementVersionStatus), openningModel.LocalStatus);
                //entity.Set<string>(schema.GetField(Define.UserID), openningModel.Geometry.UserID);
                //entity.Set<string>(schema.GetField(Define.VersionId), openningModel.Geometry.VersionId);
                //entity.Set<string>(schema.GetField(Define.RevitElementCommonId), openningModel.Geometry.RevitElementCommonId);
                if (openningModel.ServerGeometry.Geometry != null)
                    entity.Set<string>(schema.GetField(Define.Geometry), openningModel.ServerGeometry.Geometry);
                if (openningModel.ServerGeometry.Original != null)
                    entity.Set<string>(schema.GetField(Define.Original), openningModel.ServerGeometry.Original);
                if (openningModel.ServerGeometry.Direction != null)
                    entity.Set<string>(schema.GetField(Define.Direction), openningModel.ServerGeometry.Direction);
                //entity.Set<string>(schema.GetField(Define.Status), openningModel.Geometry.Status.ToString());
                //entity.Set<string>(schema.GetField(Define.Description), openningModel.Geometry.Description);
                //entity.Set<string>(schema.GetField(Define.UpdatedDate), openningModel.Geometry.UpdatedDate);
                //entity.Set<string>(schema.GetField(Define.CreateUserId), openningModel.CreateUserId);
            }
            catch (Exception ex) {
                TaskDialog.Show(" ", ex.ToString());
            }
        }

        #endregion SetValue

        #region GetValue

        public static OpeningModel GetValueToExtensibleStorage(Element openning, string subject)
        {
            OpeningModel openningModel = new OpeningModel();
            Schema schema = DefineSchemaBuilder(subject);
            Entity entity = openning.GetEntity(schema);

            if (entity.Schema != null) {
                GetValueStringOpenningByExtensibleStorage(ref schema, ref entity, openningModel);
                return openningModel;
            }
            else {
                return null;
            }
        }

        public static void GetValueStringOpenningByExtensibleStorage(ref Schema schema, ref Entity entity, OpeningModel openningModel)
        {
            openningModel.IdManager = entity.Get<string>(schema.GetField(Define.CommonId));
            openningModel.IdLocal = entity.Get<string>(schema.GetField(Define.ElementId));
            openningModel.IdDrawing = entity.Get<string>(schema.GetField(Define.DrawingId));
            //openningModel.CreateDate = entity.Get<string>(schema.GetField(Define.CreateDate));
            //openningModel.CreateUserId = entity.Get<string>(schema.GetField(Define.CreateUserId));
            openningModel.Status = entity.Get<string>(schema.GetField(Define.ElementVersionStatus));
            //openningModel.Geometry.UserID = entity.Get<string>(schema.GetField(Define.UserID));
            //openningModel.Geometry.VersionId = entity.Get<string>(schema.GetField(Define.VersionId));
            //openningModel.Geometry.RevitElementCommonId = entity.Get<string>(schema.GetField(Define.RevitElementCommonId));
            openningModel.Geometry.Geometry = entity.Get<string>(schema.GetField(Define.Geometry));
            openningModel.Geometry.Original = entity.Get<string>(schema.GetField(Define.Original));
            openningModel.Geometry.Direction = entity.Get<string>(schema.GetField(Define.Direction));
            openningModel.Geometry.Version = entity.Get<string>(schema.GetField(Define.Version));
            //openningModel.Geometry.Status = Common.StringToStatus(entity.Get<string>(schema.GetField(Define.Status)));
            //openningModel.Geometry.Description = entity.Get<string>(schema.GetField(Define.Description));
            //openningModel.Geometry.UpdatedDate = entity.Get<string>(schema.GetField(Define.UpdatedDate));
        }

        #endregion GetValue

        #region schemaBuilder

        public static Schema DefineSchemaBuilder(string enumSubject)
        {
            string GUID = "";
            switch (enumSubject) {
                case "MEP":
                    GUID = Define.GUID_OPENNING_MEP_EXTENSIBLE_STORAGE_NEW;
                    break;

                case "Structure":
                    GUID = Define.GUID_OPENNING_STRUCTURE_EXTENSIBLE_STORAGE_NEW;
                    break;

                case "Architecture":
                    GUID = Define.GUID_OPENNING_ARCHITECHTURE_EXTENSIBLE_STORAGE_NEW;
                    break;
            }
            Schema schema = Schema.Lookup(new Guid((GUID)));
            if (schema == null || schema.IsValidObject
                || schema.GUID == null || schema.GUID != Guid.Empty) {
                SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid(GUID));
                schemaBuilder.SetSchemaName(Define.SCHEMA_NAME);
                ShemaBuilderSuportToExtensibleStorage(schemaBuilder);
                return schemaBuilder.Finish();
            }
            else {
                return schema;
            }
        }

        public static void ShemaBuilderSuportToExtensibleStorage(SchemaBuilder schemaBuilder)
        {
            schemaBuilder.AddSimpleField(Define.CommonId, typeof(string));
            schemaBuilder.AddSimpleField(Define.ElementId, typeof(string));
            schemaBuilder.AddSimpleField(Define.DrawingId, typeof(string));
            schemaBuilder.AddSimpleField(Define.CreateDate, typeof(string));
            schemaBuilder.AddSimpleField(Define.UpdatedDate, typeof(string));
            schemaBuilder.AddSimpleField(Define.ElementVersionStatus, typeof(string));
            schemaBuilder.AddSimpleField(Define.UserID, typeof(string));
            schemaBuilder.AddSimpleField(Define.VersionId, typeof(string));
            schemaBuilder.AddSimpleField(Define.RevitElementCommonId, typeof(string));
            schemaBuilder.AddSimpleField(Define.Geometry, typeof(string));
            schemaBuilder.AddSimpleField(Define.Original, typeof(string));
            schemaBuilder.AddSimpleField(Define.Direction, typeof(string));
            schemaBuilder.AddSimpleField(Define.Version, typeof(string));
            schemaBuilder.AddSimpleField(Define.Status, typeof(string));
            schemaBuilder.AddSimpleField(Define.Description, typeof(string));
            schemaBuilder.AddSimpleField(Define.CreateUserId, typeof(string));
        }

        #endregion schemaBuilder
    }

    public static class ConversionStringAndArray
    {
        public static string ListStringToSimpleString(List<string> guids)
        {
            XmlSerializer xml = new XmlSerializer(typeof(List<string>));
            StringWriter writer = new StringWriter();
            xml.Serialize(writer, guids);
            return writer.ToString();
        }

        public static List<string> StringToListString(string serializerGuids)
        {
            XmlSerializer xml = new XmlSerializer(typeof(List<string>));
            StringReader read = new StringReader(serializerGuids);
            return xml.Deserialize(read) as List<string>;
        }
    }
}