using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TAWRcon_HLL_WPF.Models
{
    public partial class InputBox
    {
        private Control m_box;

        public InputBox(RconCommandParameter parameter, ServerSession session)
        {
            if (parameter.Type == "bool")
                this.CreateBoolSelectionField();
            else if (parameter.GetterToUse == null)
            {
                this.CreateInputField(parameter.Type, "");
            }
            else
            {
                string[] data = new string[0];
                if (session.GetData(parameter.GetterToUse, out data))
                {
                    if (data.Length == 1)
                        this.CreateInputField(parameter.Type, data[0]);
                    else
                        this.CreateSelectionField(data);
                }
                else
                    this.CreateInputField(parameter.Type, "");
            }
        }

        private void CreateInputField(string type, string value = "")
        {
            if (type == "password")
            {
                PasswordBox passwordBox = new PasswordBox();
                passwordBox.MinWidth = 250.0;
                passwordBox.PasswordChar = '*';
                passwordBox.Padding = new Thickness(2.0);
                this.m_box = (Control)passwordBox;
            }
            else
            {
                TextBox textBox = new TextBox();
                textBox.MinWidth = 250.0;
                textBox.Padding = new Thickness(2.0);
                textBox.Text = value;
                this.m_box = (Control)textBox;
            }
        }

        private void CreateSelectionField(string[] options)
        {
            ComboBox comboBox = new ComboBox();
            comboBox.MinWidth = 250.0;
            comboBox.Padding = new Thickness(2.0);
            comboBox.ItemsSource = (IEnumerable)options;
            this.m_box = (Control)comboBox;
        }

        private void CreateBoolSelectionField()
        {
            ComboBox comboBox = new ComboBox();
            comboBox.MinWidth = 250.0;
            comboBox.Padding = new Thickness(2.0);
            comboBox.ItemsSource = (IEnumerable)new string[2]
            {
        "on",
        "off"
            };
            this.m_box = (Control)comboBox;
        }

        public Control ControlBox
        {
            get
            {
                return this.m_box;
            }
        }

        public string Text
        {
            get
            {
                if (this.m_box.GetType().Equals(typeof(PasswordBox)))
                    return (this.m_box as PasswordBox).Password;
                if (this.m_box.GetType().Equals(typeof(ComboBox)))
                    return (this.m_box as ComboBox).SelectedValue as string;
                return (this.m_box as TextBox).Text;
            }
        }
    }
}
