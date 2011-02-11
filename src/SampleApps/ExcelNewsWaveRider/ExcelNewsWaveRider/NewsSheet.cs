using NewsWaveRider.Core.ViewModels;

namespace ExcelNewsWaveRider
{
    public partial class NewsSheet
    {
        private NewsSheetViewModel _sheetViewModel = new NewsSheetViewModel();
        private void Sheet1_Startup(object sender, System.EventArgs e)
        {
            newsWavesBindingSource.DataSource = _sheetViewModel.NewsWaves;
            newsEventsBindingSource.DataSource = _sheetViewModel.NewsEvents;

            _sheetViewModel.DataUpdated += (s, a) => RefreshData();
            _sheetViewModel.Init();
        }

        private void RefreshData()
        {
            newsWavesBindingSource.ResetBindings(false);
            newsEventsBindingSource.ResetBindings(false);
        }

        private void Sheet1_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(this.Sheet1_Startup);
            this.Shutdown += new System.EventHandler(this.Sheet1_Shutdown);

        }

        #endregion

    }
}
