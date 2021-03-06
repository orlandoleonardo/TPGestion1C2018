﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FrbaHotel.AbmHabitacion
{
    public partial class frmCrearHabitacion : Form
    {
        frmHabitacion instanciaformHabitacion;
        public frmCrearHabitacion(frmHabitacion instanciafrmHab)
        {
            InitializeComponent();
            instanciaformHabitacion = instanciafrmHab;
        }

        private void frmCrearHabitacion_Load(object sender, EventArgs e)
        {
            //inicializo formulario
            this.CenterToScreen();
            comboBoxUbicacion.Items.Add("Frente");
            comboBoxUbicacion.Items.Add("Interno");
            Utils.cargarTiposDeCamas(comboBoxTipo);
            Utils.cargarHoteles(comboBoxHotel);
        }

        private void btnCrear_Click(object sender, EventArgs e)
        {
            //chequeos
            int verificacion = 1;

            if (comboBoxHotel.Text == "")
            {
                MessageBox.Show("Seleccione un hotel.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                verificacion = 0;
            }

            if (txtNumero.Text == "")
            {
                MessageBox.Show("Ingrese número de habitación.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                verificacion = 0;
            }

            if (txtPiso.Text == "")
            {
                MessageBox.Show("Ingrese número de piso.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                verificacion = 0;
            }

            if (comboBoxTipo.Text == "")
            {
                MessageBox.Show("Seleccione tipo de habitación.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                verificacion = 0;
            }

            if (comboBoxUbicacion.Text == "")
            {
                MessageBox.Show("Seleccione ubicación de la habitación.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                verificacion = 0;
            }

            if (txtDescripcion.Text == "")
            {
                MessageBox.Show("Ingrese descripción de la habitación.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                verificacion = 0;
            }

            if (verificacion == 0)
            {
                return;
            }
            //fin chequeos

            //creo comando para ejecutar el SP
            string spAltaHabitacion = "[PISOS_PICADOS].SPAltaHabitacion";

            SqlCommand crearHabitacion = new SqlCommand(spAltaHabitacion, Globals.conexionGlobal);
            crearHabitacion.CommandType = CommandType.StoredProcedure;

            //Agrego parametros
            crearHabitacion.Parameters.Add("@numero", SqlDbType.Int);
            crearHabitacion.Parameters.Add("@IDhotel", SqlDbType.Int);
            crearHabitacion.Parameters.Add("@frente", SqlDbType.Char);
            crearHabitacion.Parameters.Add("@tipo", SqlDbType.Int);
            crearHabitacion.Parameters.Add("@descripcion", SqlDbType.VarChar);
            crearHabitacion.Parameters.Add("@piso", SqlDbType.Int);
            crearHabitacion.Parameters.Add("@habilitado", SqlDbType.Bit);

            //Cargo valores en parametros
            crearHabitacion.Parameters["@numero"].Value = txtNumero.Text;
            if (comboBoxUbicacion.SelectedItem == "Frente")
            {
                crearHabitacion.Parameters["@frente"].Value = 'S';
            }
            else
            {
                crearHabitacion.Parameters["@frente"].Value = 'N';

            }
            crearHabitacion.Parameters["@descripcion"].Value = txtDescripcion.Text;
            crearHabitacion.Parameters["@piso"].Value = Int64.Parse(txtPiso.Text);
            if (checkBoxEstado.Checked)
            {
                crearHabitacion.Parameters["@habilitado"].Value = 1;
            }
            else
            {
                crearHabitacion.Parameters["@habilitado"].Value = 0;
            }

            //busco el id del hotel seleccionado
            SqlCommand cmdBuscarIdHotel = new SqlCommand("SELECT idHotel FROM [PISOS_PICADOS].Hotel as h WHERE h.nombre = @nombre or LTRIM(RTRIM(ciudad)) + ' - ' + LTRIM(RTRIM(calle)) + ' - ' + LTRIM(RTRIM(nroCalle)) = @nombre", Globals.conexionGlobal);
            cmdBuscarIdHotel.Parameters.Add("@nombre", SqlDbType.VarChar);
            cmdBuscarIdHotel.Parameters["@nombre"].Value = comboBoxHotel.Text;
            int idHotel = (int)cmdBuscarIdHotel.ExecuteScalar();
            crearHabitacion.Parameters["@IDhotel"].Value = idHotel;

            //busco el id del tipo de habitación seleccionado
            SqlCommand cmdBuscarIdTipo = new SqlCommand("SELECT idTipo FROM [PISOS_PICADOS].Tipo as t WHERE t.tipoCamas = @tipo", Globals.conexionGlobal);
            cmdBuscarIdTipo.Parameters.Add("@tipo", SqlDbType.VarChar);
            cmdBuscarIdTipo.Parameters["@tipo"].Value = comboBoxTipo.Text;
            int idTipo = (int)cmdBuscarIdTipo.ExecuteScalar();
            crearHabitacion.Parameters["@tipo"].Value = idTipo;

            //comando para chequear si la habitación ya existe
            SqlCommand cmdExisteHab = new SqlCommand("SELECT [PISOS_PICADOS].existeNumEnHotel(@idHotel, @numero)", Globals.conexionGlobal);
            cmdExisteHab.Parameters.Add("@idHotel", SqlDbType.VarChar);
            cmdExisteHab.Parameters["@idHotel"].Value = idHotel;
            cmdExisteHab.Parameters.Add("@numero", SqlDbType.VarChar);
            cmdExisteHab.Parameters["@numero"].Value = Int64.Parse(txtNumero.Text);
            //ejecuto y recibo valor
            int existeHab = (int)cmdExisteHab.ExecuteScalar();

            //chequeo si ya existe la habitación. Si ya existe no lo dejo crear
            if (existeHab == 0)
            {
                crearHabitacion.ExecuteNonQuery();
                MessageBox.Show("Alta realizada correctamente.");
                instanciaformHabitacion.recargarHabitaciones(null);
                this.Close();
            }
            else
            {
                MessageBox.Show("Ya existe ese número de habitación en el hotel.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtPiso_KeyPress(object sender, KeyPressEventArgs e)
        {
            Utils.txtSoloAceptaNumeros(txtPiso, sender, e);
        }

        private void txtNumero_KeyPress(object sender, KeyPressEventArgs e)
        {
            Utils.txtSoloAceptaNumeros(txtNumero, sender, e);
        }

    }
}
