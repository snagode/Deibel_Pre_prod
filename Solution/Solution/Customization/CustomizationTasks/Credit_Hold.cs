using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.Library;

using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library.FormDefinition;

namespace Customization.Tasks
{
    public class Credit_Hold : DeibelSampleAdminBaseTask
    {
        private DeibelGridValueChangedArgs _cell;
        public IEntity _entity;
        EntityTemplate _jobTemplate;

        protected override bool IsJobWorkflow
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        //Hello Lisa
        internal void HandleGridCellChanged(DeibelGridValueChangedArgs cell, object obj, IEntity entity, FormSampleAdmin m_Form)
        {
            try
            {
                var deibel = (DeibelSampleAdminBaseTask)obj;
                if (cell != null)
                {
                    _cell = cell; 
                    _entity = cell.Entity;
                    var cellValue = (Customer)_cell.Value;
                 //   var cellValue1 = Convert.ToString(_cell.Value);
                    if (cellValue != null)
                    {
                        if (cellValue.CreditHold)
                        {
                            deibel.Library.Utils.FlashMessage("This customer id currently on credit hold.You cannot select this client until the hold has been released.Please contact DeibelAR for further details.", "");
                            RunFunctions(_entity, m_Form);
                            m_Form.Close();
                        }

                    }
                }
                else
                { 
                    Console.WriteLine("Cell is null.");
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        void RunFunctions(IEntity entity, FormSampleAdmin m_Form)
        {

            // Autofill based on after-edit property assignments as well as custom functions
            var et = GetTemplate(entity);
            var etp = et.EntityTemplateProperties.Cast<EntityTemplateProperty>().ToList();

            var customFunctions = etp;//.Where(p => p.DefaultType.PhraseId == PhraseEntTmpDt.PhraseIdFUNCTION).ToList();
            if (customFunctions.Count > 0)
            {
                foreach (var field in customFunctions)
                {
                    //if (field.PropertyName=="CustomerId")
                    //{
                    UpdateGridCell(_entity, field, null, m_Form);

                    // }
                    // PerformAssignment(entity, field);
                    //var property = _jobTemplate.GetProperty("OfficePhone") as EntityTemplateProperty;

                }
            }
        }

        void UpdateGridCell(IEntity entity, EntityTemplateProperty property, object value, FormSampleAdmin m_Form)
        {
            // Don't update anything if the grid doesn't exist or isn't visible..
            //if (_jobType == DeibelJobType.NewJob )
            //    return;
            //if (entity.EntityType == JobHeader.EntityName && m_VisibleGrid != m_Form.GridJobProperties)
            //    return;
            //if (entity.EntityType == Sample.EntityName && m_VisibleGrid != m_Form.GridSampleProperties)
            //    return;

            string propertyName = property.PropertyName;
            UnboundGridRow row;
            UnboundGridColumn column;

            if (entity.EntityType == JobHeader.EntityName)
            {
                row = m_Form.GridJobProperties.GetRowByTag(entity);
                column = m_Form.GridJobProperties.GetColumnByName(propertyName);
            }
            else
            {
                row = m_Form.GridSampleProperties.GetRowByTag(entity);
                column = m_Form.GridSampleProperties.GetColumnByName(propertyName);
            }

            if (row == null || column == null)
                return;

            column.EnableCell(row);
            row.SetValue(propertyName, value);

            if (property.IsReadOnly)
                column.DisableCell(row, DisabledCellDisplayMode.GreyShowContents);
        }
        static EntityTemplateInternal GetTemplate(IEntity entity)
        {
            var template = (EntityTemplateInternal)entity.GetEntity(nameof(EntityTemplate));
            return template;
        }

        protected override void RunDefaultWorkflow()
        {
            throw new NotImplementedException();
        }

        protected override bool InitialiseTopLevelEntities(out IEntityCollection topLevelEntities)
        {
            throw new NotImplementedException();
        }

        protected override string GetTitle()
        {
            throw new NotImplementedException();
        }

        protected override string GetTopLevelTableName()
        {
            throw new NotImplementedException();
        }


    }
}
