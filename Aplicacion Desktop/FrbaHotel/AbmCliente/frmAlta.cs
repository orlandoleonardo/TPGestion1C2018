﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Mail;

namespace FrbaHotel.AbmCliente
{
    public partial class frmAlta : Form
    {

        Utils utilizador = new Utils();

        public frmAlta()
        {
            InitializeComponent();
        }

        private void frmAlta_Load(object sender, EventArgs e)
        {
            //centra el formulario
            this.CenterToScreen();

            //agrego la opción de que los comboBox estén seteados en "Vacío" (aunque el cliente luego deba cambiarlo
            //dado a que es un campo obligatorio)
            cbPaises.Items.Add("Vacío");
            TipoId.Items.Add("Vacío");

            //carga los paises en el combo box cbPaises, extrayendolos de la tabla Pais hecha en sql;
            SqlCommand cmdBuscarPaises = new SqlCommand("SELECT nombrePais FROM [PISOS_PICADOS].Pais", Globals.conexionGlobal);

            SqlDataReader reader = cmdBuscarPaises.ExecuteReader();

            while (reader.Read())
            {
                cbPaises.Items.Add((reader["nombrePais"]).ToString());
            }

            reader.Close();

            //hago que por defecto los valores de los comboBox estén seteados en "Vacío"
            TipoId.SelectedItem = "Vacío";
            cbPaises.SelectedItem = "Vacío";

        }

        //los siguientes KeyPress limitan el uso de caracteres inapropiados en los textbox que los referencien
        private void soloTexto_KeyPress(object sender, KeyPressEventArgs e)
        {
            {
                if (Char.IsLetter(e.KeyChar) || Char.IsSeparator(e.KeyChar) || Char.IsControl(e.KeyChar)) { e.Handled = false; }
                else
                {
                    e.Handled = true;
                    MessageBox.Show("Este campo sólo acepta letras", "Error", MessageBoxButtons.OK);
                }
            }
        }

        private void soloNros_KeyPress(object sender, KeyPressEventArgs e)
        {
            {
                if (Char.IsDigit(e.KeyChar) || Char.IsSeparator(e.KeyChar) || Char.IsControl(e.KeyChar)) { e.Handled = false; }
                else
                {
                    e.Handled = true;
                    MessageBox.Show("Este campo solo admite números", "Error", MessageBoxButtons.OK);
                }
            }
        }

        //la siguiente función nos provee seguridad con la validación de lo ingresado por el usuario como su mail
        static bool validarEmail(string email)
        {
            try
            {
                new MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private void BotonCrear_Click(object sender, EventArgs e)
        {
            //Validaciones
            String tipoIdCliente = TipoId.Text;
            if (string.IsNullOrEmpty(nroId.Text)) { MessageBox.Show("Completar numero de id cliente"); return; }
            int nroIdCliente = int.Parse(nroId.Text);
            if (string.IsNullOrEmpty(NroCalle.Text)) { MessageBox.Show("Completar numero de calle"); return; }
            int nroCalleCliente = int.Parse(NroCalle.Text);
            DateTime fechaNacimientoCliente = FechaNacimiento.Value;
            string selectDateAsString = FechaNacimiento.Value.ToString("yyyy-MM-dd");

            if (Nombre.Text == "") { MessageBox.Show("Complete nombre", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (Apellido.Text == "") { MessageBox.Show("Complete apellido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (tipoIdCliente == "" || tipoIdCliente == "Vacío") { MessageBox.Show("Complete tipoId", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (nroIdCliente < 0) { MessageBox.Show("Complete nroID correctamente", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (Mail.Text == "") { MessageBox.Show("Complete mail correctamente", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (!validarEmail(Mail.Text) || utilizador.estaRepetidoMail(Mail.Text)) { MessageBox.Show("mail inválido", "Error"); return; }
            if (Calle.Text == "") { MessageBox.Show("Complete calle", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (nroCalleCliente < 0) { MessageBox.Show("Complete nro de calle", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (Localidad.Text == "") { MessageBox.Show("Complete localidad", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (cbPaises.Text == "" || cbPaises.SelectedItem.ToString() == "Vacío") { MessageBox.Show("Complete país", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (Nacionalidad.Text == "") { MessageBox.Show("Complete nacionalidad", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            
            //si un usuario intenta ingresar un tipo de identificación junto con un nro de identificacion que coincide con
            //el de otro cliente, no debe permitirse que lo ingrese
            if (utilizador.estaRepetidoIdentificacion(nroIdCliente, tipoIdCliente))
            {
                MessageBox.Show("Identificación Repetida");
                return;
            }

            //comienzo del proceso de dar de alta en sí
            String cadenaAltaCliente = "PISOS_PICADOS.SPAltaCliente";

            SqlCommand comandoAltaCliente = new SqlCommand(cadenaAltaCliente, Globals.conexionGlobal);
            comandoAltaCliente.CommandType = CommandType.StoredProcedure;

            //agregar parametros al sp que se encarga de dar de alta a un cliente
            comandoAltaCliente.Parameters.Add("@nombre", SqlDbType.VarChar);
            comandoAltaCliente.Parameters.Add("@apellido", SqlDbType.VarChar);
            comandoAltaCliente.Parameters.Add("@tipo", SqlDbType.VarChar);
            comandoAltaCliente.Parameters.Add("@numeroI", SqlDbType.Int);
            comandoAltaCliente.Parameters.Add("@mail", SqlDbType.VarChar);
            comandoAltaCliente.Parameters.Add("@telefono", SqlDbType.VarChar);
            comandoAltaCliente.Parameters.Add("@calle", SqlDbType.VarChar);
            comandoAltaCliente.Parameters.Add("@numeroC", SqlDbType.Int);
            comandoAltaCliente.Parameters.Add("@localidad", SqlDbType.VarChar);
            comandoAltaCliente.Parameters.Add("@pais", SqlDbType.VarChar);
            comandoAltaCliente.Parameters.Add("@nacionalidad", SqlDbType.VarChar);
            comandoAltaCliente.Parameters.Add("@fechaNacimiento", SqlDbType.DateTime);

            //cargar valores a los paramtros agregados en el paso anterior
            comandoAltaCliente.Parameters["@nombre"].Value = Nombre.Text;
            comandoAltaCliente.Parameters["@apellido"].Value = Apellido.Text;
            comandoAltaCliente.Parameters["@tipo"].Value = tipoIdCliente;
            comandoAltaCliente.Parameters["@numeroI"].Value = nroIdCliente;
            comandoAltaCliente.Parameters["@mail"].Value = Mail.Text;
            comandoAltaCliente.Parameters["@telefono"].Value = Telefono.Text;
            comandoAltaCliente.Parameters["@calle"].Value = Calle.Text;
            comandoAltaCliente.Parameters["@numeroC"].Value = nroCalleCliente;
            comandoAltaCliente.Parameters["@localidad"].Value = Localidad.Text;
            comandoAltaCliente.Parameters["@pais"].Value = cbPaises.Text;
            comandoAltaCliente.Parameters["@nacionalidad"].Value = Nacionalidad.Text;
            comandoAltaCliente.Parameters["@fechaNacimiento"].Value = fechaNacimientoCliente.ToString("yyyy-MM-dd");

            //ejecuta el sp que da alta al cliente tomando los valores ingresados en el form
            comandoAltaCliente.ExecuteNonQuery();
            MessageBox.Show("Alta realizada correctamente");

            //reinicio de los textbox
            Nombre.ResetText();
            Apellido.ResetText();
            TipoId.ResetText();
            nroId.ResetText();
            Mail.ResetText();
            Telefono.ResetText();
            Calle.ResetText();
            NroCalle.ResetText();
            Localidad.ResetText();
            Nacionalidad.ResetText();
            FechaNacimiento.ResetText();
        }

        //cierra el formulario
        private void BotonCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //Vacía los valores ingresados en los textBox y setea en "Vacío" los comboBox
        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            Nombre.ResetText();
            Apellido.ResetText();
            TipoId.SelectedItem = "Vacío";
            nroId.ResetText();
            Mail.ResetText();
            Telefono.ResetText();
            Calle.ResetText();
            NroCalle.ResetText();
            cbPaises.SelectedItem = "Vacío";
            Localidad.ResetText();
            Nacionalidad.ResetText();
            FechaNacimiento.ResetText();
        }





    }
}