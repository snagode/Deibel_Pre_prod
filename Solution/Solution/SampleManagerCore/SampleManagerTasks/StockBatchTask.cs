using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
    /// <summary>
    /// Stock Batch Task
    /// </summary>
    [SampleManagerTask("StockBatchTask", "LABTABLE", "STOCK_BATCH")]
    public class StockBatchTask : GenericLabtableTask
    {

        #region Member Variables

        private FormStockBatch m_Form;
        private StockBatch m_StockBatch;

        #endregion

        #region Overrides

        /// <summary>
        /// Main Form Created
        /// </summary>
        protected override void MainFormCreated()
        {
            base.MainFormCreated();
            m_Form = (FormStockBatch)MainForm;
            m_StockBatch = (StockBatch)MainForm.Entity;
            m_StockBatch.PropertyChanged += m_StockBatch_PropertyChanged;
        }

        #endregion

        #region Events

        /// <summary>
        /// Check the property change is valid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_StockBatch_PropertyChanged(object sender, PropertyEventArgs e)
        {
            if (e.PropertyName == StockBatchPropertyNames.Status)
            {
                if ((m_StockBatch.Status.PhraseId == PhraseStkBStat.PhraseIdC && m_StockBatch.CurrentQuantity > 0) ||
                    (m_StockBatch.Status.PhraseId == PhraseStkBStat.PhraseIdV && m_StockBatch.CurrentQuantity == 0))
                {
                    Library.Utils.FlashMessage(m_Form.StringTable.StatusChangeCaption,
                                               m_Form.StringTable.StatusChangeTitle);
                }
            }
        }

        #endregion
    }
}
