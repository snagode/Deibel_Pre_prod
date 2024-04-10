using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
    /// <summary>
    /// Lot Details Task
    /// BSmock, 3-Jan-2018;  Fixed Lot details string/phrase mistype
    /// </summary>
    [SampleManagerTask("LotReconcileTask", "LABTABLE", "LOT_DETAILS")]
    public class LotReconcileTask : DefaultSingleEntityTask
    {
        #region Member Variables

        private FormLotReconcile m_Form;
        private LotDetails m_LotDetails;
        private double m_Quantity;
        private string m_Comments;
        private bool m_ExcludeReservations;

        #endregion

        #region Overrides

        /// <summary>
        /// Called when the <see cref="P:Thermo.SampleManager.Tasks.DefaultFormTask.MainForm" /> has been created.
        /// </summary>
        protected override void MainFormCreated()
        {
            m_Form = (FormLotReconcile)MainForm;
            m_LotDetails = (LotDetails)MainForm.Entity;

            base.MainFormCreated();
        }

        /// <summary>
        /// Called to allow the consumer to extend the query
        /// </summary>
        protected override void FindSingleEntityQuery(IQuery query)
        {
            query.AddEquals(LotDetailsPropertyNames.Status, PhraseLotStat.PhraseIdV);
            base.FindSingleEntityQuery(query);
        }

        /// <summary>
        /// Called when the <see cref="P:Thermo.SampleManager.Tasks.GenericLabtableTask.MainForm" /> has been loaded.
        /// </summary>
        protected override void MainFormLoaded()
        {
            m_Form.CalcAdjustment.Click += CalcAdjustmentOnClick;

            m_Form.PageUsage.SetSelected();
            m_Form.Quantity.Focus();

            base.MainFormLoaded();
        }

        /// <summary>
        /// Calculates the adjustment on click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CalcAdjustmentOnClick(object sender, EventArgs e)
        {
            try
            {
                string warning;
                double[] adjustments = m_LotDetails.EstimateReconciledQuantity(m_Form.Quantity.Number, m_Form.ExclReserved.Checked, out warning);

                if (adjustments.Length > 0)
                {
                    m_Form.AdjustedRemaining.Text = String.Format("{0:F3} {1}", adjustments[0], m_LotDetails.Units);
                    m_Form.AdjustedAvailable.Text = String.Format("{0:F3} {1}", adjustments[1], m_LotDetails.Units);
                    m_Form.AdjustmentReserved.Text = String.Format("{0:F3} {1}", adjustments[2], m_LotDetails.Units);

                    string strDif = "-{0:F3} {1}";
                    if (adjustments[3] < 0)
                    {
                        strDif = "{0:#,0;#,0} {1}";
                    }
                    m_Form.Difference.Text = String.Format(strDif, adjustments[3], m_LotDetails.Units);
                }

                if (!String.IsNullOrEmpty(warning))
                {
                    Library.Utils.FlashMessage(warning, "Reservation Impact Warning",
                        MessageButtons.OK, MessageIcon.Warning, MessageDefaultButton.Button1);
                }
            }
            catch (SampleManagerError sme)
            {
                m_Form.Quantity.ShowError(sme.Message);
            }
        }

        /// <summary>
        /// Called before the property sheet or wizard is saved.
        /// </summary>
        /// <returns>
        /// true to allow the save to continue, false to abort the save.
        ///             Please also ensure that you call the base.OnPreSave when continuing
        ///             successfully.
        /// </returns>
        protected override bool OnPreSave()
        {
            try
            {
                m_Quantity = m_Form.Quantity.Number;
                m_Comments = m_Form.Comments.Text;
                m_ExcludeReservations = m_Form.ExclReserved.Checked;

                // Standard
                // m_LotDetails.InventoryReconcile(m_LotDetails, null, m_Quantity, m_LotDetails.Units, m_Comments, m_ExcludeReservations);

                // BSmock - change phrase to string
                m_LotDetails.InventoryReconcile(m_LotDetails, null, m_Quantity, m_LotDetails.Units.PhraseId, m_Comments, m_ExcludeReservations);
                return base.OnPreSave();
            }
            catch (SampleManagerError sme)
            {
                m_Form.Quantity.ShowError(sme.Message);
                return false;
            }
        }

        #endregion
    }
}