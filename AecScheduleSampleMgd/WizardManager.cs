using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AecScheduleSampleMgd
{
    public partial class WizardManager : Form, IWizardManager
    {
        public WizardManager()
        {
            InitializeComponent();
        }

        List<object> sheets = new List<object>();
        public void AddSheet(object sheet)
        {
            if (sheet is UserControl && sheet is IWizardSheet)
                sheets.Add(sheet);
            else
                throw new ArgumentException("Wrong type of wizard sheet");
        }
        public void RemoveSheet(object sheet)
        {
            sheets.Remove(sheet);
        }
        UiData uiData = new UiData();
        public UiData RuntimeData
        {
            get
            {
                return uiData;
            }
            set
            {
                uiData = value;
            }
        }

        public DialogResult ShowWizard()
        {
            return ShowDialog();
        }

        int currentPageIndex = 0;

        void ShowPage(int index)
        {
            if (index < 0 || index >= sheets.Count)
                throw new ArgumentOutOfRangeException();

            if (index != currentPageIndex)
            {
                IWizardSheet prevSheetInterface = sheets[currentPageIndex] as IWizardSheet;
                if (!prevSheetInterface.OnLeave())
                    return;
                UserControl prevSheet = sheets[currentPageIndex] as UserControl;
                prevSheet.Visible = false;
            }

            UserControl sheet = sheets[index] as UserControl;
            IWizardSheet sheetInterface = sheets[index] as IWizardSheet;
            sheet.Visible = true;

            char[] seprator = new char[] { '|' };
            string[] texts = sheet.Tag.ToString().Split(seprator);
            if (texts.Length != 2)
                throw new ArgumentException("Wrong user control tag");
            labelSheetName.Text = texts[0];
            labelSheetDescription.Text = texts[1];

            currentPageIndex = index;

            sheetInterface.OnEnter();
        }
        void UpdateButtonState()
        {
            buttonBack.Enabled = currentPageIndex > 0;
            buttonNext.Enabled = currentPageIndex < sheets.Count - 1;
            buttonFinish.Enabled = currentPageIndex == sheets.Count - 1;
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            GoBack();
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            GoNext();
        }

        private void WizardManager_Load(object sender, EventArgs e)
        {
            panelSheetPlaceHolder.Controls.Clear();
            panelSheetPlaceHolder.SuspendLayout();
            foreach (UserControl sheet in sheets)
            {
                sheet.Visible = false;
                sheet.Dock = DockStyle.Fill;
                IWizardSheet sheetInterface = sheet as IWizardSheet;
                sheetInterface.Data = RuntimeData;
                sheetInterface.Manager = this;
                panelSheetPlaceHolder.Controls.Add(sheet);
            }
            panelSheetPlaceHolder.ResumeLayout();

            ShowPage(0);
            UpdateButtonState();
        }

        #region IWizardManager Members

        public void Goto(int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= sheets.Count)
                throw new IndexOutOfRangeException("page index");

            ShowPage(pageIndex);
        }

        public void GoBack()
        {
            if (currentPageIndex > 0)
            {
                ShowPage(currentPageIndex - 1);
            }
            UpdateButtonState();
            DialogResult = DialogResult.None;
        }

        public void GoNext()
        {
            if (currentPageIndex < sheets.Count - 1)
                ShowPage(currentPageIndex + 1);
            UpdateButtonState();
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public int PageCount
        {
            get { return sheets.Count; }
        }

        public int CurrentPageIndex
        {
            get { return currentPageIndex; }
        }

        #endregion

    }
}
