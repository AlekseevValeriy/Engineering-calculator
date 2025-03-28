﻿namespace FFEC
{
    public partial class ControlsForm : Form, ICloseTrackable
    {
        public Boolean closed { get; set; } = true;
        public ControlsForm()
        {
            InitializeComponent();

            foreach (KeyValuePair<String, Dictionary<String, String>> sector in JsonStorage.Controls)
            {
                TabPage tab = new TabPage(JsonStorage.GetTranslate(sector.Key));

                IRemovableGenerator generator = sector.Key != "Display" ? NewButton : NewTextBox;

                FlowLayoutPanel flowPanel = new FlowLayoutPanel() { Dock = DockStyle.Fill, AutoScroll = true };
                foreach (KeyValuePair<String, String> controlPair in sector.Value)
                {
                    GroupBox groupBox = new GroupBox() { Text = JsonStorage.GetTranslate(controlPair.Key), Height = 110, Width = 200 };
                    IDataStorable control = generator(sector.Key, controlPair, groupBox.Size);
                    groupBox.Controls.Add(Wrap.DragDrop((Control)control));
                    flowPanel.Controls.Add(groupBox);
                }
                tab.Controls.Add(flowPanel);

                tabControl.TabPages.Add(tab);
            }
        }

        private IDataStorable NewButton(String sector, KeyValuePair<String, String> data, System.Drawing.Size boxSize)
        {
            SButton button = new SButton(new JObject { { "Sector", sector }, { "Name", data.Key } }.ToString()) { Text = data.Value };
            button.Size = new Size(70, 40);
            button.Location = new System.Drawing.Point(
                (boxSize.Width / 2) - (button.Size.Width / 2),
                (boxSize.Height / 2) - (button.Size.Height / 2));
            return button;
        }

        private IDataStorable NewTextBox(String sector, KeyValuePair<String, String> data, System.Drawing.Size boxSize)
        {
            STextBox textBox = new STextBox(new JObject { { "Sector", sector }, { "Name", data.Key } }.ToString()) { ReadOnly = true };
            textBox.Location = new System.Drawing.Point(
                (boxSize.Width / 2) - (textBox.Size.Width / 2),
                (boxSize.Height / 2) - (textBox.Size.Height / 2));
            return textBox;
        }

        private delegate IDataStorable IRemovableGenerator(String sector, KeyValuePair<String, String> data, System.Drawing.Size boxSize);

        private void ControlsFormShown(object sender, EventArgs e)
        {
            Handler.SetSubFormPosition((Form)Owner, this, customY: ((Form)Owner).Location.Y);
        }
    }
}
