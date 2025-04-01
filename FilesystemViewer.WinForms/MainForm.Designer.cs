using FilesystemViewer.WinForms.Controls;

namespace FilesystemViewer.WinForms
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            FileCollectionView = new CollectionView();
            SuspendLayout();
            // 
            // FileCollectionView
            // 
            FileCollectionView.AutoScroll = true;
            FileCollectionView.BackColor = Color.AliceBlue;
            FileCollectionView.DataTemplate = typeof(Control);
            FileCollectionView.Dock = DockStyle.Fill;
            FileCollectionView.ItemsSource = null;
            FileCollectionView.Location = new Point(2, 2);
            FileCollectionView.Name = "FileCollectionView";
            FileCollectionView.Size = new Size(514, 900);
            FileCollectionView.TabIndex = 1;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.RoyalBlue;
            ClientSize = new Size(518, 904);
            Controls.Add(FileCollectionView);
            Name = "MainForm";
            Padding = new Padding(2);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "FilesystemViewer.WinForms";
            ResumeLayout(false);
        }

        #endregion
        private CollectionView FileCollectionView;
    }
}
